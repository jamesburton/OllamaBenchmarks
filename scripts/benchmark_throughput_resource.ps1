param(
  [string[]]$Models,
  [int]$NumPredict = 192,
  [string]$Prompt = "Write a concise explanation of dependency injection with one short Python example.",
  [int]$Seed = 42,
  [string]$OutputPath,
  [string]$CheckpointDir
)

$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

function Get-HostDetails {
  $helper = Join-Path $scriptRoot "collect_host_info.py"
  try {
    $json = python $helper --compact
    if ($LASTEXITCODE -eq 0 -and $json) {
      return $json | ConvertFrom-Json
    }
  }
  catch {}
  return [pscustomobject]@{
    hostname = $env:COMPUTERNAME
  }
}

function Invoke-OllamaGenerate {
  param(
    [string]$Model,
    [string]$PromptText,
    [int]$PredictCount,
    [int]$SeedValue
  )

  $sampling = Get-SamplingOptions -Model $Model -UseCase "general"

  $body = @{
    model = $Model
    prompt = $PromptText
    stream = $false
    options = @{
      num_predict = $PredictCount
      temperature = $sampling.temperature
      top_p = $sampling.top_p
      seed = $SeedValue
    }
  } | ConvertTo-Json -Depth 8 -Compress

  Invoke-RestMethod -Uri "http://127.0.0.1:11434/api/generate" -Method Post -ContentType "application/json" -Body $body
}

function Get-ModelSlug {
  param(
    [string]$Model
  )

  return (($Model -replace "[:/\\]", "_") -replace "[^\w\.-]", "_")
}

function Get-SamplingOptions {
  param(
    [string]$Model,
    [string]$UseCase = "general"
  )

  if ($Model -like "nemotron-3-super*" -or $Model -like "nemotron-3-nano*") {
    if ($UseCase -eq "tool") {
      return @{
        temperature = 0.6
        top_p = 0.95
      }
    }

    return @{
      temperature = 1.0
      top_p = 1.0
    }
  }

  return @{
    temperature = 0
    top_p = 1
  }
}

function Get-CheckpointPath {
  param(
    [string]$Directory,
    [string]$Model
  )

  Join-Path $Directory ("throughput-resource-{0}.json" -f (Get-ModelSlug -Model $Model))
}

function Write-JsonFile {
  param(
    [string]$Path,
    $Payload
  )

  $dir = Split-Path -Parent $Path
  if ($dir) {
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
  }
  $resolved = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Path)
  $Payload | ConvertTo-Json -Depth 6 | Set-Content -Path $resolved
}

function Get-IncompleteModels {
  $completed = @($results | ForEach-Object { $_.model })
  $failed = @($failedModels | ForEach-Object { $_.model })
  return [string[]]($Models | Where-Object { $_ -notin $completed -and $_ -notin $failed })
}

function Get-OllamaSample {
  $procs = Get-Process -Name "ollama*" -ErrorAction SilentlyContinue
  if (-not $procs) {
    return $null
  }

  $pidPatterns = $procs.Id | ForEach-Object { "pid_$($_)_*" }
  $gpuCtr = @((Get-SafeCounterSamples -Path "\GPU Engine(*)\Utilization Percentage")) |
    Where-Object {
      $inst = $_.InstanceName
      foreach ($p in $pidPatterns) {
        if ($inst -like $p) { return $true }
      }
      return $false
    }
  $gpuMemCtr = @((Get-SafeCounterSamples -Path "\GPU Process Memory(*)\Dedicated Usage")) |
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

function Get-SafeCounterSamples {
  param(
    [string]$Path
  )

  try {
    $counter = Get-Counter $Path -ErrorAction Stop
    return @($counter.CounterSamples | Where-Object { $_.Status -eq 0 })
  }
  catch {
    return @()
  }
}

if (-not $Models -or $Models.Count -eq 0) {
  throw "Pass one or more model names with -Models."
}

$runStarted = Get-Date
if (-not $OutputPath) {
  $stamp = $runStarted.ToString("yyyyMMdd-HHmmss")
  $OutputPath = ".\\results\\throughput-resource-$stamp.json"
}
if (-not $CheckpointDir) {
  $CheckpointDir = Split-Path -Parent $OutputPath
}

$logical = [double][Environment]::ProcessorCount
$results = @()
$failedModels = @()
$hostDetails = Get-HostDetails

function New-AggregatePayload {
  return [pscustomobject]@{
    benchmark = "throughput_resource"
    run_started_at = $runStarted.ToString("o")
    run_finished_at = (Get-Date).ToString("o")
    output_path = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath)
    host = $env:COMPUTERNAME
    ollama_host = "http://127.0.0.1:11434"
    host_details = $hostDetails
    models = $Models
    completed_models = @($results | ForEach-Object { $_.model })
    failed_models = $failedModels
    incomplete_models = [string[]]@((Get-IncompleteModels))
    prompt = $Prompt
    num_predict = $NumPredict
    seed = $Seed
    results = $results
  }
}

foreach ($model in $Models) {
  try {
    $null = Invoke-OllamaGenerate -Model $model -PromptText "Warmup: one short sentence." -PredictCount 16 -SeedValue $Seed

    $body = @{
      model = $model
      prompt = $Prompt
      stream = $false
      options = @{
        num_predict = $NumPredict
        temperature = (Get-SamplingOptions -Model $model -UseCase "general").temperature
        top_p = (Get-SamplingOptions -Model $model -UseCase "general").top_p
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

    $modelPayload = [pscustomobject]@{
      benchmark = "throughput_resource"
      run_started_at = $runStarted.ToString("o")
      run_finished_at = (Get-Date).ToString("o")
      output_path = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath((Get-CheckpointPath -Directory $CheckpointDir -Model $model))
      host = $env:COMPUTERNAME
      ollama_host = "http://127.0.0.1:11434"
      host_details = $hostDetails
      models = @($model)
      prompt = $Prompt
      num_predict = $NumPredict
      seed = $Seed
      results = @(
        [pscustomobject]@{
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
      )
    }

    $results += $modelPayload.results[0]
    Write-JsonFile -Path (Get-CheckpointPath -Directory $CheckpointDir -Model $model) -Payload $modelPayload
  }
  catch {
    $failedModels += [pscustomobject]@{
      model = $model
      error = $_.Exception.Message
    }
    Write-JsonFile -Path (Get-CheckpointPath -Directory $CheckpointDir -Model $model) -Payload ([pscustomobject]@{
      benchmark = "throughput_resource"
      run_started_at = $runStarted.ToString("o")
      run_finished_at = (Get-Date).ToString("o")
      output_path = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath((Get-CheckpointPath -Directory $CheckpointDir -Model $model))
      host = $env:COMPUTERNAME
      ollama_host = "http://127.0.0.1:11434"
      host_details = $hostDetails
      models = @($model)
      completed_models = @()
      failed_models = @([pscustomobject]@{ model = $model; error = $_.Exception.Message })
      incomplete_models = @()
      prompt = $Prompt
      num_predict = $NumPredict
      seed = $Seed
      results = @()
    })
  }

  Write-JsonFile -Path $OutputPath -Payload (New-AggregatePayload)
}

$payload = New-AggregatePayload
$payload | ConvertTo-Json -Depth 6
