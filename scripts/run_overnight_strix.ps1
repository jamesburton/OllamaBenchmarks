param()
# Sequential runner for overnight Strix work:
#   1) devstral q4_K_M Layer 2 (158 tasks)
#   2) think:true L3 variants for thinking-capable installed models
$ErrorActionPreference = 'Continue'
Set-Location 'C:\Development\OllamaBenchmarks'

$logDir = 'C:\Development\OllamaBenchmarks\results\overnight-logs'
New-Item -ItemType Directory -Path $logDir -Force | Out-Null
$summary = Join-Path $logDir ("run-{0:yyyyMMdd-HHmmss}.log" -f (Get-Date))

function Log {
  param([string]$Message)
  $ts = Get-Date -Format 'HH:mm:ss'
  "$ts $Message" | Tee-Object -FilePath $summary -Append
}

Log "=== Overnight Strix run starting ==="

# Step 1: devstral L2
$devstralL2 = 'C:\Development\OllamaBenchmarks\results\coding-devstral-small-2_24b-instruct-2512-q4_K_M.json'
if (Test-Path $devstralL2) {
  $existing = Get-Content -Raw $devstralL2 | ConvertFrom-Json
  if ($existing.layer2_results -and $existing.layer2_results.Count -ge 158) {
    Log "devstral L2 already complete ($($existing.layer2_results.Count)/158) — skipping"
  } else {
    Log "devstral L2 partial ($($existing.layer2_results.Count)/158) — re-running"
    $partial = $true
  }
}
if (-not (Test-Path $devstralL2) -or $partial) {
  Log "Running devstral q4_K_M Layer 2 (158 HumanEval-CS tasks) ..."
  $log = Join-Path $logDir 'devstral-l2.log'
  python scripts/benchmark_coding_layer2.py `
    --models 'devstral-small-2:24b-instruct-2512-q4_K_M' `
    --dataset-path scripts/coding_tasks/datasets/data/humaneval-cs-reworded.json `
    *>&1 | Tee-Object -FilePath $log
  Log "devstral L2 complete (log: $log)"
}

# Step 2: think:true L3 variants
Log "Running think:true L3 variants (run_think_variants_l3.py) ..."
$log = Join-Path $logDir 'think-variants.log'
python scripts/run_think_variants_l3.py *>&1 | Tee-Object -FilePath $log
Log "think variants complete (log: $log)"

Log "=== Overnight Strix run finished ==="
