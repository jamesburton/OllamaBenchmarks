<#
.SYNOPSIS
  Service-restart-based performance lever sweep for a single Ollama model.

  Tests levers that require the Ollama service to be relaunched with different
  environment variables (unlike scripts/benchmark_sweep.py, which only needs
  per-request "options" overrides): KV cache quantization (OLLAMA_KV_CACHE_TYPE)
  and concurrent-decode batching (OLLAMA_NUM_PARALLEL).

  Each variant: stop ollama + "ollama app", clear the tunable env vars, apply
  the variant's overrides, relaunch "ollama app.exe", poll /api/version until
  ready, warm up the model, then measure. Ends by restarting once more with NO
  overrides so the box is left in its normal (User-env) default state.

.NOTES
  Restarting the Ollama tray app evicts whatever model is currently loaded.
  Only run this when the GPU/model is actually idle (check `ollama ps` and
  that no benchmark harness is mid-run first).
#>
param(
  [string]$Model = "mistral-medium-3.5:iq3m",
  [string]$OllamaAppPath = "C:\Users\james\AppData\Local\Programs\Ollama\ollama app.exe",
  [int]$NumPredict = 192,
  [int]$NumCtx = 8192,
  [int]$SingleStreamRepeats = 3,
  [string]$Prompt = "Write a concise explanation of dependency injection with one short Python example.",
  [string]$OutputPath = ".\results\perf-levers\service-sweep.json"
)

$ErrorActionPreference = "Stop"
$TUNABLE_VARS = @('OLLAMA_KV_CACHE_TYPE', 'OLLAMA_NUM_PARALLEL')

function Set-OllamaEnvAndRestart {
  param([hashtable]$Overrides = @{})

  # Stop-Process on "ollama"/"ollama app" does NOT kill the llama-server.exe child --
  # it orphans instead, leaking the entire model's memory (~60GB) until manually killed.
  # Must stop llama-server explicitly too. See PLATFORM_QUIRKS.md (2026-07-05).
  Get-Process -Name "ollama", "ollama app", "llama-server" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
  Start-Sleep -Seconds 3

  foreach ($name in $TUNABLE_VARS) {
    Remove-Item "Env:$name" -ErrorAction SilentlyContinue
  }
  foreach ($kv in $Overrides.GetEnumerator()) {
    Set-Item -Path "Env:$($kv.Key)" -Value $kv.Value
  }

  Start-Process -FilePath $OllamaAppPath

  $ready = $false
  for ($i = 0; $i -lt 60; $i++) {
    try {
      Invoke-RestMethod -Uri "http://127.0.0.1:11434/api/version" -TimeoutSec 3 | Out-Null
      $ready = $true
      break
    } catch {
      Start-Sleep -Seconds 2
    }
  }
  if (-not $ready) { throw "Ollama server did not become ready after restart (waited 120s)" }
}

function Invoke-Generate {
  param([int]$PredictCount, [int]$Seed = 42)
  $body = @{
    model   = $Model
    prompt  = $Prompt
    stream  = $false
    options = @{
      num_predict = $PredictCount
      num_ctx     = $NumCtx
      temperature = 0
      top_p       = 1
      seed        = $Seed
    }
  } | ConvertTo-Json -Depth 8 -Compress
  Invoke-RestMethod -Uri "http://127.0.0.1:11434/api/generate" -Method Post -ContentType "application/json" -Body $body
}

function Measure-SingleStream {
  param([int]$Repeats)
  $null = Invoke-Generate -PredictCount 16   # warmup / reload
  $runs = @()
  for ($i = 0; $i -lt $Repeats; $i++) {
    $resp = Invoke-Generate -PredictCount $NumPredict -Seed (42 + $i)
    $evalS = [double]$resp.eval_duration / 1e9
    $tps = if ($evalS -gt 0) { [double]$resp.eval_count / $evalS } else { 0 }
    $runs += [pscustomobject]@{ eval_count = $resp.eval_count; eval_s = [math]::Round($evalS, 3); tps = [math]::Round($tps, 3) }
  }
  return $runs
}

function Measure-Concurrent {
  param([int]$Concurrency)
  $null = Invoke-Generate -PredictCount 16   # warmup / reload
  $jobs = @()
  $wallStart = Get-Date
  for ($i = 0; $i -lt $Concurrency; $i++) {
    $jobs += Start-Job -ScriptBlock {
      param($model, $prompt, $predictCount, $numCtx, $seed)
      $body = @{
        model   = $model
        prompt  = $prompt
        stream  = $false
        options = @{ num_predict = $predictCount; num_ctx = $numCtx; temperature = 0; top_p = 1; seed = $seed }
      } | ConvertTo-Json -Depth 8 -Compress
      $r = Invoke-RestMethod -Uri "http://127.0.0.1:11434/api/generate" -Method Post -ContentType "application/json" -Body $body
      return $r | ConvertTo-Json -Depth 10 -Compress
    } -ArgumentList $Model, $Prompt, $NumPredict, $NumCtx, (42 + $i)
  }
  $jobs | Wait-Job | Out-Null
  $wallSeconds = ((Get-Date) - $wallStart).TotalSeconds
  $responses = $jobs | ForEach-Object { Receive-Job -Id $_.Id | ConvertFrom-Json }
  $jobs | Remove-Job -Force
  $totalEvalCount = ($responses | Measure-Object -Property eval_count -Sum).Sum
  $perStreamTps = $responses | ForEach-Object {
    $s = [double]$_.eval_duration / 1e9
    if ($s -gt 0) { [math]::Round([double]$_.eval_count / $s, 3) } else { 0 }
  }
  [pscustomobject]@{
    concurrency        = $Concurrency
    wall_seconds        = [math]::Round($wallSeconds, 3)
    total_eval_count    = $totalEvalCount
    aggregate_tps       = if ($wallSeconds -gt 0) { [math]::Round($totalEvalCount / $wallSeconds, 3) } else { 0 }
    per_stream_tps      = $perStreamTps
  }
}

$results = [ordered]@{}

Write-Host "=== Variant: baseline (no overrides) - single stream ==="
Set-OllamaEnvAndRestart -Overrides @{}
$results["baseline_single"] = Measure-SingleStream -Repeats $SingleStreamRepeats

Write-Host "=== Variant: OLLAMA_KV_CACHE_TYPE=q8_0 - single stream ==="
Set-OllamaEnvAndRestart -Overrides @{ OLLAMA_KV_CACHE_TYPE = "q8_0" }
$results["kv_q8_0_single"] = Measure-SingleStream -Repeats $SingleStreamRepeats

Write-Host "=== Variant: OLLAMA_KV_CACHE_TYPE=q4_0 - single stream ==="
Set-OllamaEnvAndRestart -Overrides @{ OLLAMA_KV_CACHE_TYPE = "q4_0" }
$results["kv_q4_0_single"] = Measure-SingleStream -Repeats $SingleStreamRepeats

Write-Host "=== Variant: OLLAMA_NUM_PARALLEL=1, concurrency=2 (expect serialized) ==="
Set-OllamaEnvAndRestart -Overrides @{ OLLAMA_NUM_PARALLEL = "1" }
$results["parallel1_concurrency2"] = Measure-Concurrent -Concurrency 2

Write-Host "=== Variant: OLLAMA_NUM_PARALLEL=2, concurrency=2 (expect batching win if any) ==="
Set-OllamaEnvAndRestart -Overrides @{ OLLAMA_NUM_PARALLEL = "2" }
$results["parallel2_concurrency2"] = Measure-Concurrent -Concurrency 2

Write-Host "=== Restoring baseline (no overrides) ==="
Set-OllamaEnvAndRestart -Overrides @{}

$payload = [pscustomobject]@{
  benchmark      = "perf_lever_service_sweep"
  model          = $Model
  num_predict    = $NumPredict
  num_ctx        = $NumCtx
  run_finished_at = (Get-Date).ToString("o")
  results        = $results
}

$dir = Split-Path -Parent $OutputPath
if ($dir) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
$payload | ConvertTo-Json -Depth 10 | Set-Content -Path $OutputPath
$payload | ConvertTo-Json -Depth 10
