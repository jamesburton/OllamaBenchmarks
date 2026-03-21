# RustDesk + Tailscale Migration Guide

Migrate RustDesk remote desktop connections from relay-based numeric IDs to direct Tailscale IP connections for faster, more reliable, and fully encrypted remote access.

## Why?

RustDesk's default setup routes traffic through public relay servers. This can be:
- **Slow** — relay servers add latency and bandwidth limits
- **Unreliable** — free relays go down, CGNAT blocks hole-punching
- **Less secure** — traffic passes through third-party infrastructure

Using Tailscale, connections go **directly between your machines** over an encrypted WireGuard tunnel. No relays, no port forwarding, no firewall rules needed.

## Prerequisites

### Tailscale

Install and sign in to [Tailscale](https://tailscale.com/download) on every machine you want to connect. All devices must be on the same tailnet (signed in with the same account).

- [Windows install guide](https://tailscale.com/kb/1022/install-windows)
- [Linux install guide](https://tailscale.com/kb/1031/install-linux)
- [macOS install guide](https://tailscale.com/kb/1016/install-mac)

Verify with `tailscale status` — you should see all your machines listed with `100.x.x.x` IPs.

### RustDesk

Install [RustDesk](https://rustdesk.com/) on each machine. No account or server setup is required for direct IP connections.

## Setup

### Step 1: Enable Direct IP Access on Each Host

Every machine you want to connect **to** needs direct IP access enabled.

**Via RustDesk UI:**
Settings → Security → Enable "Direct IP Access" (port 21118)

**Via config file (headless / no GUI access):**

Edit `RustDesk2.toml` in the RustDesk config directory:

| OS | Config path |
|----|------------|
| Windows | `%APPDATA%\RustDesk\config\RustDesk2.toml` |
| Linux | `~/.config/rustdesk/RustDesk2.toml` |
| macOS | `~/Library/Application Support/RustDesk/config/RustDesk2.toml` |

Add these lines under `[options]`:

```toml
[options]
direct-server = 'Y'
direct-access-port = '21118'
```

Restart RustDesk after editing. On Windows the service runs elevated, so restart via Task Manager (admin) or:

```powershell
# PowerShell (Admin)
Stop-Process -Name RustDesk -Force
Start-Process "C:\Program Files\RustDesk\RustDesk.exe" -ArgumentList "--tray"
```

### Step 2: Set a Permanent Password

Each host needs a permanent password for unattended access. Set this in:
Settings → Security → Permanent Password

### Step 3: Run the Migration Script

The migration script auto-detects Tailscale IPs for your RustDesk peers by matching hostnames and creates new peer entries using the Tailscale IPs.

```bash
# Preview what will be migrated (no changes made)
python scripts/rustdesk_tailscale_migrate.py --dry-run

# Auto-match all peers by hostname
python scripts/rustdesk_tailscale_migrate.py --auto

# Interactive mode — prompts for unmatched peers
python scripts/rustdesk_tailscale_migrate.py

# Just create a backup without migrating
python scripts/rustdesk_tailscale_migrate.py --backup-only
```

The script will:
1. **Back up** the entire RustDesk config to a timestamped directory
2. **Query Tailscale** for all devices on your tailnet
3. **Match** RustDesk peers to Tailscale devices by hostname
4. **Create** new peer files keyed by Tailscale IP (e.g. `100.98.151.94.toml`)
5. **Preserve** original numeric peers (nothing is deleted)

Example output:

```
Tailscale devices found: 4
  framework            -> 100.80.83.33
  i9desktop            -> 100.98.151.94
  strix                -> 100.120.157.35
  t5500                -> 100.79.118.92

Migration plan:
  217735547    (i9desktop      ) -> 100.98.151.94    [auto] NEW
  27308452     (framework      ) -> 100.80.83.33     [auto] NEW
  481244071    (strix          ) -> 100.120.157.35   [auto] NEW

Unmatched peers (no Tailscale, kept as-is):
  221479174    (oldframework)
```

### Step 4: Connect

After restarting RustDesk, you can connect using either:

- **Tailscale IP** — enter `100.x.x.x` in the RustDesk connection field
- **Original numeric ID** — still works if the relay servers are available

Tailscale connections bypass relays entirely and go through the encrypted WireGuard tunnel.

## Troubleshooting

### "Connection refused" when using Tailscale IP
- Ensure `direct-server = 'Y'` is set on the **remote** machine
- Check the port: `netstat -an | findstr 21118` (Windows) or `ss -tlnp | grep 21118` (Linux)
- Restart RustDesk with admin privileges

### Tailscale device not showing up
- Run `tailscale status` on both machines
- Ensure both are signed in to the same tailnet
- Check if the remote device shows "offline" — it may need reauthentication

### Hostnames don't match between RustDesk and Tailscale
- Use interactive mode (without `--auto`) to manually enter Tailscale IPs
- Or rename the Tailscale device: `tailscale set --hostname=myname`

### RustDesk won't restart on Windows
The service component runs elevated. Use Task Manager as admin, or:
```powershell
Stop-Process -Name RustDesk -Force
Start-Process "C:\Program Files\RustDesk\RustDesk.exe" -ArgumentList "--tray"
```

## Running on Multiple Machines

Run the script on each machine where you use RustDesk as a **client** (the machine you connect *from*). The script reads that machine's local peer list and Tailscale status.

The `direct-server` config change (Step 1) is needed on each machine you connect **to**.

## Reverting

Backups are created automatically at:

| OS | Backup location |
|----|----------------|
| Windows | `%APPDATA%\RustDesk\config-backup-YYYYMMDD-HHMMSS\` |
| Linux | `~/.config/rustdesk-backup-YYYYMMDD-HHMMSS/` |

To revert, delete the current `config/peers/` directory and copy the backed-up one back.

---

## Using Ollama Remotely via Tailscale

Once Ollama is listening on `0.0.0.0:11434` and Tailscale is connected, any device on your tailnet can use the remote GPU as if it were local.

### Server Setup (one-time)

On the machine running Ollama, set `OLLAMA_HOST=0.0.0.0:11434` as a system environment variable and restart Ollama. See [Ollama FAQ](https://docs.ollama.com/faq) for OS-specific instructions.

**Windows (PowerShell as Admin):**
```powershell
[Environment]::SetEnvironmentVariable("OLLAMA_HOST", "0.0.0.0:11434", "Machine")
# Restart Ollama after setting
```

**Linux:**
```bash
sudo systemctl edit ollama
# Add: Environment="OLLAMA_HOST=0.0.0.0:11434"
sudo systemctl restart ollama
```

### Client Usage

From any Tailscale-connected device, replace `localhost` with the server's Tailscale IP.

**curl:**
```bash
curl http://100.79.118.92:11434/api/chat -d '{
  "model": "qwen3.5:9b",
  "messages": [{"role": "user", "content": "Hello"}],
  "stream": false
}'
```

**Python (official ollama library):**
```python
from ollama import Client

client = Client(host='http://100.79.118.92:11434')
response = client.chat(model='qwen3.5:9b', messages=[
    {'role': 'user', 'content': 'Explain dependency injection'}
])
print(response['message']['content'])
```

**Environment variable (makes all Ollama CLI commands use the remote server):**
```bash
export OLLAMA_HOST=http://100.79.118.92:11434
ollama list          # lists models on the REMOTE machine
ollama run qwen3.5:9b   # runs on the REMOTE GPU
```

This is powerful — a lightweight laptop can run `ollama run` and the inference happens on the remote GPU server.

### Integration with Dev Tools

Many tools support Ollama's API endpoint and can be pointed at a remote Tailscale address:

**[Open WebUI](https://github.com/open-webui/open-webui):**
```bash
# On the client machine, point Open WebUI at the remote Ollama
docker run -d -p 3000:8080 \
  -e OLLAMA_BASE_URL=http://100.79.118.92:11434 \
  ghcr.io/open-webui/open-webui:main
```

**[Continue.dev](https://continue.dev/) (VS Code / JetBrains):**

In `~/.continue/config.yaml`:
```yaml
models:
  - title: "Remote Qwen3.5 9B"
    provider: ollama
    model: qwen3.5:9b
    apiBase: http://100.79.118.92:11434
```

**[Claude Code](https://claude.ai/claude-code) with remote Ollama:**

Claude Code's Remote Control feature can connect to a GPU server running Ollama over Tailscale. Set `OLLAMA_HOST` on the server and use the Tailscale IP from your client. See the [Claude Code + Ollama guide](https://lgallardo.com/2026/02/28/claude-code-remote-control-ollama/) for details.

**Any OpenAI-compatible client:**

Ollama exposes an OpenAI-compatible API at `/v1/`. Point any client that supports custom base URLs to:
```
http://100.79.118.92:11434/v1
```

### Security Notes

- Tailscale encrypts all traffic end-to-end via WireGuard — no TLS/HTTPS needed
- Only devices on your tailnet can reach port 11434
- For extra control, use [Tailscale ACLs](https://tailscale.com/kb/1018/acls) to restrict which devices can access the Ollama port
- Do **not** set `OLLAMA_HOST=0.0.0.0` without Tailscale or a firewall — it exposes the API to your entire LAN

---

## Remote Desktop Access with RustDesk + Tailscale

RustDesk over Tailscale gives you full graphical desktop access to remote machines without relying on external relay servers.

### Connecting via RustDesk UI

1. Open RustDesk on your local machine
2. Enter the **Tailscale IP** of the remote machine (e.g. `100.79.118.92`)
3. Enter the permanent password
4. Click **Connect**

The connection goes directly through the Tailscale tunnel — no relay, no RustDesk account needed.

### Connecting via Command Line

RustDesk supports CLI-initiated connections, useful for scripting or quick access:

**Windows:**
```powershell
& "C:\Program Files\RustDesk\RustDesk.exe" --connect 100.79.118.92
```

**Linux / macOS:**
```bash
rustdesk --connect 100.79.118.92
```

With password (note: password is visible in process list):
```bash
rustdesk --connect 100.79.118.92 --password YOUR_PASSWORD
```

### Headless Host Configuration

For machines without a monitor or where you can't access the RustDesk UI (e.g. a server in a closet), configure everything via config files and command line.

**Set permanent password (CLI):**
```bash
# Linux
sudo rustdesk --password mypassword

# Windows (PowerShell as Admin)
& "C:\Program Files\RustDesk\RustDesk.exe" --password mypassword
```

**Get RustDesk ID (CLI):**
```bash
# Linux
sudo rustdesk --get-id

# Windows
& "C:\Program Files\RustDesk\RustDesk.exe" --get-id
```

**Enable direct IP access (config file):**

Edit `RustDesk2.toml` (see [Setup Step 1](#step-1-enable-direct-ip-access-on-each-host) for paths) and add under `[options]`:
```toml
direct-server = 'Y'
direct-access-port = '21118'
```

**Allow remote configuration changes (useful for managed fleets):**
```bash
rustdesk --option allow-remote-config-modification Y
```

### Combined Workflow: RustDesk + Ollama

A common pattern is using RustDesk for desktop access and Ollama API for inference, both over Tailscale:

1. **RustDesk** for administration — install models, check GPU usage, restart services
2. **Ollama API** for inference — point your dev tools at `http://<tailscale-ip>:11434`

This way your laptop does the coding while the remote GPU does the heavy lifting, and you can always RustDesk in if something needs manual attention.

### Remote Terminal via RustDesk (Recommended for Windows)

RustDesk includes a built-in **Terminal (beta)** feature that gives you a remote PowerShell session without any SSH setup, extra accounts, or key exchange.

**To use it:**
1. In the RustDesk main window, find the remote machine in your peer list
2. Click the **three-dot menu** (...)
3. Select **"Terminal (beta)"** or **"Terminal (Run as administrator) (beta)"**

This opens a PowerShell session on the remote machine, authenticated with RustDesk's permanent password. Over Tailscale direct IP, the connection is fully encrypted.

**Why this is better than SSH on Windows:**
- No extra user accounts needed
- Works with Microsoft account logins (no password re-enabling)
- No SSH key exchange to manage
- Same authentication you already use for remote desktop
- Admin option available for elevated tasks

**Known limitations:**
- The terminal runs in the **SYSTEM profile context** (`C:\Windows\System32\config\systemprofile`), not your user profile
- Per-user installed tools (e.g. `winget`) may not be in the PATH
- For Ollama, nvidia-smi, Python, and most admin tasks this is not an issue since they're installed system-wide

**Example usage over Tailscale:**
Once you've migrated your peers to Tailscale IPs (see [Migration Script](#step-3-run-the-migration-script)), the terminal session goes directly through the Tailscale tunnel:

```
# From the RustDesk terminal on 100.79.118.92 (T5500):
ollama list
nvidia-smi
python E:\Development\OllamaBenchmarks\scripts\benchmark_quality.py --models qwen3.5:9b
```

### SSH Terminal Access via Tailscale (Alternative)

For environments where RustDesk isn't available, or where you need user-context shells, SSH over Tailscale is an alternative.

#### Linux / macOS hosts

Tailscale has built-in SSH that requires zero key management:

```bash
# Enable on the remote machine (one-time)
tailscale set --ssh

# Connect from any tailnet device
ssh user@100.79.118.92
```

See [Tailscale SSH docs](https://tailscale.com/kb/1193/tailscale-ssh) for details.

#### Windows hosts

**Tailscale's built-in SSH server does not support Windows.** You need OpenSSH Server instead, which Tailscale secures at the network layer.

**The Microsoft account gotcha:** Windows SSH key-based authentication (`authorized_keys`) only works with **local Windows accounts**, not Microsoft accounts. If all your machines use Microsoft account logins, you have two options:

| Approach | Pros | Cons |
|----------|------|------|
| **Password auth over Tailscale** | Works immediately with Microsoft accounts | Must type password each time |
| **Create a local account for SSH** | Key-based auth, no password prompts | Extra account to manage |

Password auth over Tailscale is still secure — Tailscale encrypts everything with WireGuard, so the password never crosses the public internet.

#### Automated Setup Script

The included `setup_tailscale_ssh.ps1` script handles the full Windows SSH setup:

```powershell
# Run as Administrator

# 1. Install and configure OpenSSH Server on this machine
.\scripts\setup_tailscale_ssh.ps1 -SetupServer

# 2. Exchange keys with a remote Tailscale host
.\scripts\setup_tailscale_ssh.ps1 -ExchangeKeys -RemoteHost 100.79.118.92

# 3. Both at once
.\scripts\setup_tailscale_ssh.ps1 -SetupServer -ExchangeKeys -RemoteHost 100.80.83.33
```

The script will:

1. **Install** OpenSSH Server (if not already installed)
2. **Configure** `sshd_config` — enables public key auth, keeps password auth for Microsoft accounts
3. **Scope the firewall** to Tailscale's IP range only (`100.64.0.0/10`) — SSH is not exposed to your LAN
4. **Start** the sshd service and set it to auto-start
5. **Generate** an ED25519 key pair (if you don't have one)
6. **Deploy** your public key to the remote machine's `authorized_keys` (handles the Windows admin path quirk)
7. **Test** the connection

#### Key Exchange Walkthrough (manual)

If you prefer to do it manually:

**On the client machine (the one you SSH from):**
```powershell
# Generate a key pair (skip if you already have one)
ssh-keygen -t ed25519

# Copy your public key to the remote machine
# You'll be prompted for the remote password once
type $env:USERPROFILE\.ssh\id_ed25519.pub | ssh james@100.79.118.92 "powershell -Command `"Add-Content -Path 'C:/ProgramData/ssh/administrators_authorized_keys' -Value (Read-Host -Prompt 'key') -Force`""
```

**On the remote Windows machine (admin PowerShell):**
```powershell
# Fix permissions on the admin authorized_keys file
icacls C:\ProgramData\ssh\administrators_authorized_keys /inheritance:r /grant "SYSTEM:(F)" /grant "BUILTIN\Administrators:(F)"
```

**Important:** For admin users on Windows, keys go in `C:\ProgramData\ssh\administrators_authorized_keys` (not `~/.ssh/authorized_keys`). The file must have restricted permissions or sshd silently ignores it. The setup script handles this automatically.

#### After Setup

```bash
# Connect from any Tailscale device — no password needed
ssh james@100.79.118.92

# Run commands remotely
ssh james@100.79.118.92 "ollama list"
ssh james@100.79.118.92 "nvidia-smi"
```

See [Microsoft's OpenSSH key management guide](https://learn.microsoft.com/en-us/windows-server/administration/openssh/openssh_keymanagement) for more details.
