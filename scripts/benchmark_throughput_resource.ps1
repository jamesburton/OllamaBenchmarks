param(
  [string[]]$Models,
  [int]$NumPredict = 192,
  [string]$Prompt = "Write a concise explanation of dependency injection with one short Python example.",
  [int]$Seed = 42,
  [string]$OutputPath = ".\\results\\throughput-resource.json"
)

$ErrorActionPreference = "Stop"

function Invoke-OllamaGenerate {
  param(
    [string]$Model,
    [string]$PromptText,
    [int]$PredictCount,
    [int]$SeedValue
  )

  $body = @{
    model = $Model
    prompt = $PromptText
    stream = $false
    options = @{
      num_predict = $PredictCount
      temperature = 0
      top_p = 1
      seed = $SeedValue
    }
  } | ConvertTo-Json -Depth 8 -Compress

  Invoke-RestMethod -Uri "http://127.0.0.1:11434/api/generate" -Method Post -ContentType "application/json" -Body $body
}

function Get-OllamaSample {
  $procs = Get-Process -Name "ollama*" -ErrorAction SilentlyContinue
  if (-not $procs) {
    return $null
  }

  $pidPatterns = $procs.Id | ForEach-Object { "pid_$($_)_*" }
  $gpuCtr = (Get-Counter "\GPU Engine(*)\Utilization Percentage").CounterSamples |
    Where-Object {
      $inst = $_.InstanceName
      foreach ($p in $pidPatterns) {
        if ($inst -like $p) { return $true }
      }
      return $false
    }
  $gpuMemCtr = (Get-Counter "\GPU Process Memory(*)\Dedicated Usage").CounterSamples |
    Where-Object {
      $inst = $_.InstanceName
      foreach ($p in $pidPatterns) {
        if ($inst -like $p) { return $true }
      }
      return $false
    }

  [pscustomobject]@{
    CpuSeconds = [double](($procs | Measure-Object CPU -Sum).Sum)
    RamBytes = [double](($procs | Measure-Object WorkingSet64 -Sum).Sum)
    GpuUtil = [double](($gpuCtr | Measure-Object CookedValue -Sum).Sum)
    GpuMemBytes = [double](($gpuMemCtr | Measure-Object CookedValue -Sum).Sum)
  }
}

if (-not $Models -or $Models.Count -eq 0) {
  throw "Pass one or more model names with -Models."
}

$logical = [double][Environment]::ProcessorCount
$results = @()

foreach ($model in $Models) {
  $null = Invoke-OllamaGenerate -Model $model -PromptText "Warmup: one short sentence." -PredictCount 16 -SeedValue $Seed

  $body = @{
    model = $model
    prompt = $Prompt
    stream = $false
    options = @{
      num_predict = $NumPredict
      temperature = 0
      top_p = 1
      seed = $Seed
    }
  } | ConvertTo-Json -Depth 8 -Compress

  $job = Start-Job -ScriptBlock {
    param($json)
    Invoke-RestMethod -Uri "http://127.0.0.1:11434/api/generate" -Method Post -ContentType "application/json" -Body $json | ConvertTo-Json -Depth 10 -Compress
  } -ArgumentList $body

  $cpuSamples = @()
  $ramSamples = @()
  $gpuUtilSamples = @()
  $gpuMemSamples = @()
  $prevCpu = $null
  $prevTime = $null

  while ((Get-Job -Id $job.Id).State -eq "Running") {
    $now = Get-Date
    $sample = Get-OllamaSample
    if ($sample) {
      $ramSamples += $sample.RamBytes
      $gpuUtilSamples += $sample.GpuUtil
      $gpuMemSamples += $sample.GpuMemBytes
      if ($prevCpu -ne $null) {
        $dt = ($now - $prevTime).TotalSeconds
        if ($dt -gt 0) {
          $cpuPct = ((($sample.CpuSeconds - $prevCpu) / $dt) * 100.0) / $logical
          if ($cpuPct -lt 0) { $cpuPct = 0 }
          $cpuSamples += $cpuPct
        }
      }
      $prevCpu = $sample.CpuSeconds
      $prevTime = $now
    }
    Start-Sleep -Milliseconds 1000
  }

  $raw = Receive-Job -Id $job.Id -Wait
  Remove-Job -Id $job.Id -Force
  $resp = $raw | ConvertFrom-Json
  $evalSec = [double]$resp.eval_duration / 1e9
  $tokps = if ($evalSec -gt 0) { [double]$resp.eval_count / $evalSec } else { 0 }
  $psLine = (ollama ps | Select-String -Pattern ([regex]::Escape($model)) | Select-Object -First 1).Line

  $results += [pscustomobject]@{
    model = $model
    eval_count = [int]$resp.eval_count
    eval_s = [math]::Round($evalSec, 3)
    toks_per_s = [math]::Round($tokps, 2)
    total_s = [math]::Round(([double]$resp.total_duration / 1e9), 3)
    load_s = [math]::Round(([double]$resp.load_duration / 1e9), 3)
    cpu_avg_pct = if ($cpuSamples.Count) { [math]::Round((($cpuSamples | Measure-Object -Average).Average), 1) } else { 0 }
    cpu_peak_pct = if ($cpuSamples.Count) { [math]::Round((($cpuSamples | Measure-Object -Maximum).Maximum), 1) } else { 0 }
    ram_peak_gb = if ($ramSamples.Count) { [math]::Round((($ramSamples | Measure-Object -Maximum).Maximum / 1GB), 2) } else { 0 }
    gpu_util_avg = if ($gpuUtilSamples.Count) { [math]::Round((($gpuUtilSamples | Measure-Object -Average).Average), 1) } else { 0 }
    gpu_util_peak = if ($gpuUtilSamples.Count) { [math]::Round((($gpuUtilSamples | Measure-Object -Maximum).Maximum), 1) } else { 0 }
    gpu_mem_peak_gb = if ($gpuMemSamples.Count) { [math]::Round((($gpuMemSamples | Measure-Object -Maximum).Maximum / 1GB), 2) } else { 0 }
    ollama_ps = $psLine
  }
}

$dir = Split-Path -Parent $OutputPath
if ($dir) {
  New-Item -ItemType Directory -Force -Path $dir | Out-Null
}
$results | ConvertTo-Json -Depth 6 | Set-Content -Path $OutputPath
$results | ConvertTo-Json -Depth 6

