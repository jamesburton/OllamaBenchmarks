param(
  [string]$Model,
  [string[]]$Backends = @("auto", "vulkan", "rocm"),
  [string]$OutputPath = ".\\results\\backend-comparison.json"
)

$ErrorActionPreference = "Stop"

if (-not $Model) {
  throw "Pass -Model <name>."
}

function Test-Lib {
  param(
    [string]$Lib,
    [int]$Port,
    [string]$ModelName
  )

  $job = Start-Job -ScriptBlock {
    param($lib, $port)
    $env:OLLAMA_HOST = "127.0.0.1:$port"
    if ($lib -ne "auto") {
      $env:OLLAMA_LLM_LIBRARY = $lib
    } else {
      Remove-Item Env:OLLAMA_LLM_LIBRARY -ErrorAction SilentlyContinue
    }
    $env:OLLAMA_DEBUG = "1"
    ollama serve
  } -ArgumentList $Lib, $Port

  try {
    $ready = $false
    for ($i = 0; $i -lt 40; $i++) {
      Start-Sleep -Milliseconds 500
      try {
        $null = Invoke-RestMethod -Uri "http://127.0.0.1:$Port/api/tags" -TimeoutSec 2
        $ready = $true
        break
      } catch {}
    }

    if (-not $ready) {
      return [pscustomobject]@{ lib = $Lib; status = "startup_failed" }
    }

    $warm = @{
      model = $ModelName
      prompt = "Warmup"
      stream = $false
      options = @{ num_predict = 16; temperature = 0; seed = 42 }
    } | ConvertTo-Json -Depth 8 -Compress
    $null = Invoke-RestMethod -Uri "http://127.0.0.1:$Port/api/generate" -Method Post -ContentType "application/json" -Body $warm -TimeoutSec 600

    $body = @{
      model = $ModelName
      prompt = "Write a concise explanation of dependency injection with one short Python example."
      stream = $false
      options = @{ num_predict = 192; temperature = 0; top_p = 1; seed = 42 }
    } | ConvertTo-Json -Depth 8 -Compress
    $r = Invoke-RestMethod -Uri "http://127.0.0.1:$Port/api/generate" -Method Post -ContentType "application/json" -Body $body -TimeoutSec 1200

    $evalS = [double]$r.eval_duration / 1e9
    $tokps = if ($evalS -gt 0) { [double]$r.eval_count / $evalS } else { 0 }
    $e2e = if ([double]$r.total_duration -gt 0) { [double]$r.eval_count / ([double]$r.total_duration / 1e9) } else { 0 }
    return [pscustomobject]@{
      lib = $Lib
      status = "ok"
      toks_per_s = [math]::Round($tokps, 2)
      e2e_toks_per_s = [math]::Round($e2e, 2)
      total_s = [math]::Round(([double]$r.total_duration / 1e9), 3)
    }
  }
  finally {
    Stop-Job $job -ErrorAction SilentlyContinue
    Remove-Job $job -Force -ErrorAction SilentlyContinue
  }
}

$results = @()
for ($i = 0; $i -lt $Backends.Count; $i++) {
  $results += Test-Lib -Lib $Backends[$i] -Port (11435 + $i) -ModelName $Model
}

$dir = Split-Path -Parent $OutputPath
if ($dir) {
  New-Item -ItemType Directory -Force -Path $dir | Out-Null
}
$results | ConvertTo-Json -Depth 5 | Set-Content -Path $OutputPath
$results | ConvertTo-Json -Depth 5

