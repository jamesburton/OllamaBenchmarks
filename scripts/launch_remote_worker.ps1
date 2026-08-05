<#
.SYNOPSIS
  Launch a run_parallel_workers.py stage as a WMI-detached process so it
  survives the SSH session closing (Windows OpenSSH kills Start-Process
  fire-and-forget children when the launching session ends -- see
  OTHER_MACHINES.md). Run this ON the remote box (e.g. via ssh framework).
#>
param(
  [Parameter(Mandatory=$true)][string]$Stage,
  [Parameter(Mandatory=$true)][string]$Model,
  [string]$DatasetPath = "",
  [string]$OllamaHost = "http://strix:11434",
  [string]$RepoRoot = "C:\Development\OllamaBenchmarks",
  [string]$LogName = "",
  [string]$WorkRoot = "results\perf-parallel",
  [string]$CheckpointDir = "results",
  [int]$Limit = 0
)

if (-not $LogName) { $LogName = "perf-parallel-$Stage" }
$logPath = Join-Path $RepoRoot "results\$LogName.log"

$pyArgs = "scripts\run_parallel_workers.py --stage $Stage --model `"$Model`" --checkpoint-dir `"$CheckpointDir`" --work-root `"$WorkRoot`""
if ($DatasetPath) { $pyArgs += " --dataset-path `"$DatasetPath`"" }
if ($Limit -gt 0) { $pyArgs += " --limit $Limit" }

$cmd = "cmd /c `"cd /d $RepoRoot && set OLLAMA_HOST=$OllamaHost && python $pyArgs > `"$logPath`" 2>&1`""

$result = Invoke-CimMethod -ClassName Win32_Process -MethodName Create -Arguments @{ CommandLine = $cmd }
Write-Output "ProcessId: $($result.ProcessId) ReturnValue: $($result.ReturnValue)"
Write-Output "Log: $logPath"
