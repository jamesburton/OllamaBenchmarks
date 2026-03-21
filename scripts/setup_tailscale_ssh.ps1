<#
.SYNOPSIS
    Set up OpenSSH Server on Windows for Tailscale-secured SSH access.

.DESCRIPTION
    Guides through installing OpenSSH Server, configuring key-based
    authentication, and exchanging keys between machines on a Tailscale
    network. Handles the Windows-specific gotchas (admin authorized_keys
    path, Microsoft account limitations, firewall scoping to Tailscale).

    Must be run as Administrator.

.PARAMETER SetupServer
    Install and configure OpenSSH Server on this machine.

.PARAMETER ExchangeKeys
    Generate an SSH key pair (if needed) and copy the public key to a
    remote Tailscale host.

.PARAMETER RemoteHost
    Tailscale IP or hostname of the remote machine (for key exchange).

.PARAMETER RemoteUser
    Username on the remote machine (defaults to current user).

.EXAMPLE
    # Full server setup on this machine
    .\setup_tailscale_ssh.ps1 -SetupServer

    # Exchange keys with a remote machine
    .\setup_tailscale_ssh.ps1 -ExchangeKeys -RemoteHost 100.79.118.92

    # Both at once
    .\setup_tailscale_ssh.ps1 -SetupServer -ExchangeKeys -RemoteHost 100.80.83.33
#>

param(
    [switch]$SetupServer,
    [switch]$ExchangeKeys,
    [string]$RemoteHost,
    [string]$RemoteUser = $env:USERNAME
)

$ErrorActionPreference = "Stop"

function Test-Admin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]$identity
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Get-TailscaleIP {
    try {
        $output = & tailscale ip -4 2>&1
        return $output.Trim()
    } catch {
        return $null
    }
}

function Install-OpenSSHServer {
    Write-Host "`n=== Installing OpenSSH Server ===" -ForegroundColor Cyan

    $capability = Get-WindowsCapability -Online | Where-Object Name -like 'OpenSSH.Server*'
    if ($capability.State -eq 'Installed') {
        Write-Host "  OpenSSH Server is already installed." -ForegroundColor Green
    } else {
        Write-Host "  Installing OpenSSH Server..."
        Add-WindowsCapability -Online -Name 'OpenSSH.Server~~~~0.0.1.0'
        Write-Host "  Installed." -ForegroundColor Green
    }
}

function Configure-SSHServer {
    Write-Host "`n=== Configuring OpenSSH Server ===" -ForegroundColor Cyan

    $configPath = "$env:ProgramData\ssh\sshd_config"
    $config = Get-Content $configPath -Raw

    # Enable public key authentication
    if ($config -match '#PubkeyAuthentication yes') {
        $config = $config -replace '#PubkeyAuthentication yes', 'PubkeyAuthentication yes'
        Write-Host "  Enabled PubkeyAuthentication"
    }

    # Keep password auth enabled (needed for Microsoft accounts)
    if ($config -match '#PasswordAuthentication yes') {
        $config = $config -replace '#PasswordAuthentication yes', 'PasswordAuthentication yes'
        Write-Host "  Enabled PasswordAuthentication (required for Microsoft accounts)"
    }

    # Ensure the admin authorized_keys match block exists and is correct
    # Windows OpenSSH uses a special path for admin users
    $adminBlock = @"

# Admin users use a separate authorized_keys file
Match Group administrators
       AuthorizedKeysFile __PROGRAMDATA__/ssh/administrators_authorized_keys
"@

    if ($config -notmatch 'administrators_authorized_keys') {
        # Check if there's a commented version
        if ($config -match '#Match Group administrators') {
            # Uncomment the existing block
            $config = $config -replace '#Match Group administrators', 'Match Group administrators'
            $config = $config -replace '#\s*AuthorizedKeysFile __PROGRAMDATA__/ssh/administrators_authorized_keys',
                '       AuthorizedKeysFile __PROGRAMDATA__/ssh/administrators_authorized_keys'
            Write-Host "  Uncommented admin authorized_keys block"
        }
    } else {
        Write-Host "  Admin authorized_keys block already configured"
    }

    Set-Content $configPath -Value $config
    Write-Host "  Config saved to $configPath" -ForegroundColor Green
}

function Start-SSHService {
    Write-Host "`n=== Starting SSH Service ===" -ForegroundColor Cyan

    Set-Service sshd -StartupType Automatic
    Start-Service sshd
    Write-Host "  sshd service started and set to auto-start." -ForegroundColor Green
}

function Configure-Firewall {
    Write-Host "`n=== Configuring Firewall ===" -ForegroundColor Cyan

    # Get Tailscale interface IP range
    $tsIP = Get-TailscaleIP
    if (-not $tsIP) {
        Write-Host "  WARNING: Tailscale not detected. Firewall rule will allow all SSH." -ForegroundColor Yellow
        $scope = "Any"
    } else {
        Write-Host "  Tailscale IP: $tsIP"
        # Tailscale CGNAT range
        $scope = "100.64.0.0/10"
        Write-Host "  Restricting SSH firewall rule to Tailscale range ($scope)"
    }

    # Remove default OpenSSH rule if it exists (allows from anywhere)
    $existing = Get-NetFirewallRule -Name "OpenSSH-Server-In-TCP" -ErrorAction SilentlyContinue
    if ($existing) {
        Remove-NetFirewallRule -Name "OpenSSH-Server-In-TCP"
        Write-Host "  Removed default OpenSSH firewall rule"
    }

    # Create Tailscale-scoped rule
    $ruleName = "OpenSSH-Server-Tailscale"
    $existingTS = Get-NetFirewallRule -Name $ruleName -ErrorAction SilentlyContinue
    if ($existingTS) {
        Remove-NetFirewallRule -Name $ruleName
    }

    if ($scope -eq "Any") {
        New-NetFirewallRule -Name $ruleName -DisplayName 'OpenSSH Server (All)' `
            -Enabled True -Direction Inbound -Protocol TCP -Action Allow -LocalPort 22
    } else {
        New-NetFirewallRule -Name $ruleName -DisplayName 'OpenSSH Server (Tailscale only)' `
            -Enabled True -Direction Inbound -Protocol TCP -Action Allow -LocalPort 22 `
            -RemoteAddress $scope
    }
    Write-Host "  Firewall rule created." -ForegroundColor Green
}

function Ensure-SSHKey {
    Write-Host "`n=== SSH Key Setup ===" -ForegroundColor Cyan

    $keyPath = "$env:USERPROFILE\.ssh\id_ed25519"
    if (Test-Path $keyPath) {
        Write-Host "  SSH key already exists: $keyPath" -ForegroundColor Green
    } else {
        Write-Host "  Generating new ED25519 key pair..."
        ssh-keygen -t ed25519 -f $keyPath -N '""' -C "$env:USERNAME@$env:COMPUTERNAME"
        Write-Host "  Key generated." -ForegroundColor Green
    }

    $pubKey = Get-Content "$keyPath.pub"
    Write-Host "  Public key: $pubKey"
    return $pubKey
}

function Copy-KeyToRemote {
    param([string]$Host, [string]$User, [string]$PubKey)

    Write-Host "`n=== Copying Public Key to $User@$Host ===" -ForegroundColor Cyan
    Write-Host "  You will be prompted for the remote password (one time only)."
    Write-Host ""

    # Determine if remote user is admin — assume yes for simplicity
    # On Windows, admin keys go to C:\ProgramData\ssh\administrators_authorized_keys
    # On non-admin, they go to C:\Users\<user>\.ssh\authorized_keys

    Write-Host "  Is '$User' an administrator on the remote machine? (Y/n): " -NoNewline
    $isAdmin = Read-Host
    if ($isAdmin -eq '' -or $isAdmin -match '^[Yy]') {
        $remotePath = "C:/ProgramData/ssh/administrators_authorized_keys"
        Write-Host "  Target: $remotePath (admin path)"

        # Append key and fix permissions
        $commands = @(
            "powershell -Command `"Add-Content -Path '$remotePath' -Value '$PubKey' -Force`"",
            "powershell -Command `"icacls '$remotePath' /inheritance:r /grant 'SYSTEM:(F)' /grant 'BUILTIN\Administrators:(F)'`""
        )
        foreach ($cmd in $commands) {
            Write-Host "  Running: ssh $User@$Host $cmd"
            ssh "$User@$Host" $cmd
        }
    } else {
        $remotePath = "C:/Users/$User/.ssh/authorized_keys"
        Write-Host "  Target: $remotePath (user path)"

        ssh "$User@$Host" "powershell -Command `"New-Item -Path 'C:/Users/$User/.ssh' -ItemType Directory -Force | Out-Null; Add-Content -Path '$remotePath' -Value '$PubKey' -Force`""
    }

    Write-Host "`n  Key deployed! Testing connection..." -ForegroundColor Green
    ssh -o BatchMode=yes "$User@$Host" "echo 'SSH key auth working!'" 2>&1
}

# =====================================================================
# Main
# =====================================================================

Write-Host ""
Write-Host "====================================" -ForegroundColor Cyan
Write-Host " Tailscale SSH Setup for Windows"    -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan

$tsIP = Get-TailscaleIP
if ($tsIP) {
    Write-Host "Tailscale IP: $tsIP" -ForegroundColor Green
} else {
    Write-Host "WARNING: Tailscale not detected!" -ForegroundColor Yellow
}

if (-not $SetupServer -and -not $ExchangeKeys) {
    Write-Host ""
    Write-Host "Usage:"
    Write-Host "  .\setup_tailscale_ssh.ps1 -SetupServer                 # Install & configure SSH server"
    Write-Host "  .\setup_tailscale_ssh.ps1 -ExchangeKeys -RemoteHost IP # Copy your key to a remote host"
    Write-Host "  .\setup_tailscale_ssh.ps1 -SetupServer -ExchangeKeys -RemoteHost IP  # Both"
    Write-Host ""
    Write-Host "Must be run as Administrator."
    exit 0
}

if (-not (Test-Admin)) {
    Write-Host ""
    Write-Host "ERROR: This script must be run as Administrator." -ForegroundColor Red
    Write-Host "Right-click PowerShell -> 'Run as administrator' and try again."
    exit 1
}

if ($SetupServer) {
    Install-OpenSSHServer
    Configure-SSHServer
    Start-SSHService
    Configure-Firewall

    Write-Host ""
    Write-Host "=== Server Setup Complete ===" -ForegroundColor Green
    Write-Host "This machine ($env:COMPUTERNAME) is now accepting SSH on port 22."
    if ($tsIP) {
        Write-Host "Connect from any Tailscale device:  ssh $env:USERNAME@$tsIP"
    }
    Write-Host ""
    Write-Host "NOTE: Microsoft account users will authenticate with their Microsoft"
    Write-Host "account password (not PIN). For key-based auth, a local Windows account"
    Write-Host "is required, or use the -ExchangeKeys flag to set up keys."
}

if ($ExchangeKeys) {
    if (-not $RemoteHost) {
        Write-Host ""
        Write-Host "Available Tailscale devices:" -ForegroundColor Cyan
        tailscale status
        Write-Host ""
        $RemoteHost = Read-Host "Enter Tailscale IP of remote machine"
    }

    $pubKey = Ensure-SSHKey
    Copy-KeyToRemote -Host $RemoteHost -User $RemoteUser -PubKey $pubKey

    Write-Host ""
    Write-Host "=== Key Exchange Complete ===" -ForegroundColor Green
    Write-Host "You can now connect without a password:"
    Write-Host "  ssh $RemoteUser@$RemoteHost"
}

Write-Host ""
