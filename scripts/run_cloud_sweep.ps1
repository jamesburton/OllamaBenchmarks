param(
  [string[]]$Models = @(
    'glm-5.2:cloud',
    'kimi-k2.7-code:cloud',
    'minimax-m3:cloud',
    'deepseek-v4-flash:cloud',
    'deepseek-v4-pro:cloud'
  ),
  [int]$ExpectedL3Tasks = 50,
  [switch]$SkipQuality,
  [switch]$SkipL3
)
# Sequential quality + L3 sweep across Ollama cloud model tags.
#
# Two layers of resume, matching the pattern in run_overnight_strix.ps1:
#   1. Inside each HTTP call (task_runner.call_ollama / benchmark_quality.post_json),
#      a 429/503 now retries with Retry-After-aware backoff instead of failing the task.
#   2. At this wrapper's level, each (model, stage) pair is skipped if its output
#      file already looks complete, so re-running this script after a crash/kill
#      only repeats the interrupted stage, not the whole sweep.
#
# Cloud models are chat-tuned, so only quality + L3 run (matches the existing
# glm-5:cloud / minimax-m2.7:cloud / deepseek-v3.2:cloud / cogito-2.1:cloud
# entries already in results/ — none of those have L2 data either).
$ErrorActionPreference = 'Continue'
Set-Location 'C:\Development\OllamaBenchmarks'

# Cloud throttling means a single stalled/rate-limited call can legitimately
# take minutes; give call_ollama/post_json more retry headroom than the local
# default.
if (-not $env:OLLAMA_MAX_RETRIES) { $env:OLLAMA_MAX_RETRIES = '8' }

# Known gotcha (MODEL_QUIRKS.md): OLLAMA_HOST is sometimes left set to a bare
# "0.0.0.0:11434" (no scheme) from prior sessions, which urllib rejects as an
# unknown URL type and silently zeroes every result. Normalize it here.
if ($env:OLLAMA_HOST -and $env:OLLAMA_HOST -notmatch '^https?://') {
  Write-Host "Normalizing stale OLLAMA_HOST='$env:OLLAMA_HOST' -> http://127.0.0.1:11434"
  $env:OLLAMA_HOST = 'http://127.0.0.1:11434'
}

$logDir = 'C:\Development\OllamaBenchmarks\results\overnight-logs'
New-Item -ItemType Directory -Path $logDir -Force | Out-Null
$summary = Join-Path $logDir ("cloud-sweep-{0:yyyyMMdd-HHmmss}.log" -f (Get-Date))

function Log {
  param([string]$Message)
  $ts = Get-Date -Format 'HH:mm:ss'
  "$ts $Message" | Tee-Object -FilePath $summary -Append
}

function Get-Slug {
  param([string]$Model)
  $slug = $Model -replace ':latest$', ''
  $slug = $slug -replace '[:/\\]', '_'
  return $slug
}

function Test-QualityDone {
  param([string]$Slug)
  $path = "results\quality-$Slug.json"
  if (-not (Test-Path $path)) { return $false }
  try {
    $data = Get-Content -Raw $path | ConvertFrom-Json
  } catch {
    return $false
  }
  return [bool]($data.results -and $data.results.Count -gt 0)
}

function Test-L3Done {
  param([string]$Slug, [int]$Expected)
  $path = "results\coding-$Slug.json"
  if (-not (Test-Path $path)) { return $false }
  try {
    $data = Get-Content -Raw $path | ConvertFrom-Json
  } catch {
    return $false
  }
  return [bool]($data.layer3_results -and $data.layer3_results.Count -ge $Expected)
}

Log "=== Cloud sweep starting (models: $($Models -join ', ')) ==="
Log "OLLAMA_MAX_RETRIES=$($env:OLLAMA_MAX_RETRIES)"

foreach ($model in $Models) {
  $slug = Get-Slug $model
  Log "--- $model (slug=$slug) ---"

  if (-not $SkipQuality) {
    if (Test-QualityDone $slug) {
      Log "  [skip] quality-$slug.json already complete"
    } else {
      Log "  Running quality suite ..."
      $log = Join-Path $logDir "quality-$slug.log"
      python scripts/benchmark_quality.py --models $model *>&1 | Tee-Object -FilePath $log
      Log "  quality done (log: $log)"
    }
  }

  if (-not $SkipL3) {
    if (Test-L3Done $slug $ExpectedL3Tasks) {
      Log "  [skip] coding-$slug.json already has >= $ExpectedL3Tasks L3 tasks"
    } else {
      Log "  Running L3 (.NET practical suite) ..."
      $log = Join-Path $logDir "l3-$slug.log"
      python scripts/benchmark_coding_layer3.py --models $model *>&1 | Tee-Object -FilePath $log
      Log "  L3 done (log: $log)"
    }
  }
}

Log "=== Cloud sweep finished ==="
