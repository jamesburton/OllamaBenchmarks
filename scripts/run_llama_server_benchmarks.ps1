param(
  [string[]]$Models = @('qwen3.5:35b-a3b-q2_k_l'),
  [string]$Port = '8081',
  [string]$LlamaServerPath = 'llama-server.exe',
  [string]$SummaryPath = '.\results\llama-server-summary-current.md',
  [switch]$SkipQuality
)

$ErrorActionPreference = 'Stop'

function Get-ModelBlobPath {
  param([string]$Model)

  $modelfile = & 'C:\Users\james\AppData\Local\Programs\Ollama\ollama.exe' show $Model --modelfile
  foreach ($line in $modelfile) {
    if ($line -like 'FROM *:\*') {
      return $line.Substring(5).Trim()
    }
  }

  throw "Could not resolve GGUF blob path for $Model"
}

function Wait-ServerReady {
  param([string]$BaseUrl)

  $deadline = (Get-Date).AddMinutes(10)
  while ((Get-Date) -lt $deadline) {
    try {
      $response = Invoke-WebRequest -UseBasicParsing -TimeoutSec 5 -Uri "$BaseUrl/health"
      if ($response.StatusCode -eq 200) {
        return
      }
    }
    catch {}
    Start-Sleep -Seconds 5
  }

  throw "Timed out waiting for llama-server at $BaseUrl"
}

function Get-ModelSlug {
  param([string]$Model)
  return (($Model -replace '[:/\\]', '_') -replace '[^\w\.-]', '_')
}

$summaryRows = @()

foreach ($model in $Models) {
  $slug = Get-ModelSlug -Model $model
  $blob = Get-ModelBlobPath -Model $model
  $baseUrl = "http://127.0.0.1:$Port"
  $stdoutLog = ".\results\llama-server-$slug.log"
  $stderrLog = ".\results\llama-server-$slug.err.log"

  Get-Process llama-server -ErrorAction SilentlyContinue | Stop-Process -Force

  $proc = Start-Process -FilePath $LlamaServerPath `
    -ArgumentList @('-m', $blob, '--host', '127.0.0.1', '--port', $Port, '--alias', $model, '--ctx-size', '32768', '--n-gpu-layers', '99', '--jinja', '--reasoning', 'off') `
    -RedirectStandardOutput $stdoutLog `
    -RedirectStandardError $stderrLog `
    -PassThru

  try {
    Wait-ServerReady -BaseUrl $baseUrl

    python .\scripts\benchmark_throughput_openai.py --model $model --base-url $baseUrl
    if (-not $SkipQuality) {
      python .\scripts\benchmark_quality_openai.py --models $model --base-url $baseUrl
    }

    $throughputPath = ".\results\throughput-resource-$slug.json"
    $qualityPath = ".\results\quality-$slug.json"
    $throughput = Get-Content -Raw $throughputPath | ConvertFrom-Json
    $quality = if ((-not $SkipQuality) -and (Test-Path $qualityPath)) { Get-Content -Raw $qualityPath | ConvertFrom-Json } else { $null }
    $throughputRow = $throughput.results[0]
    $qualityRow = if ($quality) { $quality.results[0] } else { $null }

    $summaryRows += [pscustomobject]@{
      Model = $model
      TokPerS = $throughputRow.toks_per_s
      RamPeakGb = $throughputRow.ram_peak_gb
      GpuMemPeakGb = $throughputRow.gpu_mem_peak_gb
      Score = if ($qualityRow) { '{0}/{1}' -f $qualityRow.score, $qualityRow.score_max } else { 'throughput-only' }
    }
  }
  finally {
    if ($proc -and -not $proc.HasExited) {
      Stop-Process -Id $proc.Id -Force
    }
    Start-Sleep -Seconds 2
  }
}

$lines = @(
  '# Llama-Server Summary',
  '',
  '| Model | tok/s | RAM peak (GB) | GPU mem peak (GB) | Quick quality |',
  '| --- | ---: | ---: | ---: | ---: |'
)
foreach ($row in $summaryRows) {
  $lines += ('| `{0}` | {1} | {2} | {3} | {4} |' -f $row.Model, $row.TokPerS, $row.RamPeakGb, $row.GpuMemPeakGb, $row.Score)
}

$lines -join "`r`n" | Set-Content $SummaryPath
