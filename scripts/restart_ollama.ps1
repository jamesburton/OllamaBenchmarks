<#
.SYNOPSIS
  Restart the Ollama service with the given environment variable overrides.

  Stop-Process on "ollama"/"ollama app" alone orphans the llama-server.exe
  child, leaking the whole model's memory (see PLATFORM_QUIRKS.md, 2026-07-05).
  This script kills all three by name, applies the requested env var
  overrides (clearing any previously-set tunables not in the override set),
  relaunches "ollama app.exe", and polls /api/version until the server
  responds.

.PARAMETER EnvOverrides
  Hashtable of NAME=VALUE env var overrides to apply before restart, e.g.
  @{ OLLAMA_NUM_PARALLEL = "2" }. Omit / pass @{} to restore plain defaults
  (falls back to whatever is set at User/Machine scope).

.NOTES
  Only restart Ollama when the model is actually idle -- this evicts
  whatever is currently loaded.
#>
param(
  [hashtable]$EnvOverrides = @{},
  [string]$OllamaAppPath = "C:\Users\james\AppData\Local\Programs\Ollama\ollama app.exe",
  [string[]]$TunableVars = @('OLLAMA_KV_CACHE_TYPE', 'OLLAMA_NUM_PARALLEL')
)

$ErrorActionPreference = "Stop"

Get-Process -Name "ollama", "ollama app", "llama-server" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3

foreach ($name in $TunableVars) {
  Remove-Item "Env:$name" -ErrorAction SilentlyContinue
}
foreach ($kv in $EnvOverrides.GetEnumerator()) {
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

Write-Host "Ollama restarted with overrides: $($EnvOverrides | ConvertTo-Json -Compress)"
