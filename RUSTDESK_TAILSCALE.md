# Remote Access Guide: RustDesk, Ollama & Tailscale

Set up secure remote desktop, terminal, and Ollama API access across your machines using Tailscale's encrypted WireGuard mesh network.

## Contents

- [Prerequisites](#prerequisites)
- [RustDesk Migration to Tailscale](#rustdesk-migration-to-tailscale)
- [Remote Ollama API via Tailscale](#remote-ollama-api-via-tailscale)
- [Remote Desktop & Terminal Access](#remote-desktop--terminal-access)
- [SSH Access (Alternative)](#ssh-terminal-access-via-tailscale-alternative)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Tailscale

Install and sign in to [Tailscale](https://tailscale.com/download) on every machine you want to connect. All devices must be on the same tailnet (same account).

- [Windows install](https://tailscale.com/kb/1022/install-windows) | [Linux install](https://tailscale.com/kb/1031/install-linux) | [macOS install](https://tailscale.com/kb/1016/install-mac)

Verify with `tailscale status` — all machines should appear with `100.x.x.x` IPs.

### RustDesk

Install [RustDesk](https://rustdesk.com/) on each machine. No account or self-hosted server is required for direct IP connections over Tailscale.

---

## RustDesk Migration to Tailscale

### Why Migrate?

RustDesk's default relay-based connections can be slow, unreliable (CGNAT, relay outages), and route traffic through third-party servers. Tailscale direct-IP connections are faster, always available, and fully encrypted end-to-end.

### Step 1: Enable Direct IP Access on Each Host

Every machine you want to connect **to** needs direct IP access enabled.

**Via RustDesk UI:** Settings > Security > Enable "Direct IP Access" (port 21118)

**Via config file (headless):** Edit `RustDesk2.toml` and add under `[options]`:

```toml
direct-server = 'Y'
direct-access-port = '21118'
```

| OS | Config path |
|----|------------|
| Windows | `%APPDATA%\RustDesk\config\RustDesk2.toml` |
| Linux | `~/.config/rustdesk/RustDesk2.toml` |
| macOS | `~/Library/Application Support/RustDesk/config/RustDesk2.toml` |

Restart RustDesk after editing (requires admin on Windows):

```powershell
Stop-Process -Name RustDesk -Force
Start-Process "C:\Program Files\RustDesk\RustDesk.exe" -ArgumentList "--tray"
```

### Step 2: Set a Permanent Password

Each host needs a permanent password for unattended access:
Settings > Security > Permanent Password

### Step 3: Run the Migration Script

The included script auto-detects Tailscale IPs for your RustDesk peers by matching hostnames:

```bash
python scripts/rustdesk_tailscale_migrate.py --dry-run   # preview only
python scripts/rustdesk_tailscale_migrate.py --auto       # auto-match all
python scripts/rustdesk_tailscale_migrate.py              # interactive
python scripts/rustdesk_tailscale_migrate.py --backup-only
```

The script will:
1. **Back up** the entire RustDesk config (timestamped)
2. **Query Tailscale** for all devices on your tailnet
3. **Match** RustDesk peers to Tailscale devices by hostname
4. **Create** new peer entries keyed by Tailscale IP
5. **Preserve** original numeric peers (nothing is deleted)

Run on each machine where you use RustDesk as a **client**. The `direct-server` config (Step 1) is needed on machines you connect **to**.

### Step 4: Connect

Enter the **Tailscale IP** (e.g. `100.79.118.92`) in RustDesk's connection field. Original numeric IDs still work as a fallback.

### Reverting

Backups are at `%APPDATA%\RustDesk\config-backup-YYYYMMDD-HHMMSS\` (Windows) or `~/.config/rustdesk-backup-YYYYMMDD-HHMMSS/` (Linux). Copy the backed-up `peers/` directory back to revert.

---

## Remote Ollama API via Tailscale

Any device on your tailnet can use a remote machine's GPU for inference.

### Server Setup (one-time)

Set `OLLAMA_HOST=0.0.0.0:11434` as a system environment variable and restart Ollama.

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

From any Tailscale device, replace `localhost` with the server's Tailscale IP:

**curl:**
```bash
curl http://100.79.118.92:11434/api/chat -d '{
  "model": "qwen3.5:9b",
  "messages": [{"role": "user", "content": "Hello"}],
  "stream": false
}'
```

**Python:**
```python
from ollama import Client
client = Client(host='http://100.79.118.92:11434')
response = client.chat(model='qwen3.5:9b', messages=[
    {'role': 'user', 'content': 'Explain dependency injection'}
])
```

**Environment variable (all Ollama CLI commands use the remote server):**
```bash
export OLLAMA_HOST=http://100.79.118.92:11434
ollama list            # lists models on the REMOTE machine
ollama run qwen3.5:9b  # inference on the REMOTE GPU
```

### Dev Tool Integration

Point any tool that supports Ollama or OpenAI-compatible APIs at the Tailscale IP:

| Tool | Configuration |
|------|--------------|
| **[Open WebUI](https://github.com/open-webui/open-webui)** | `OLLAMA_BASE_URL=http://100.79.118.92:11434` |
| **[Continue.dev](https://continue.dev/)** | Set `apiBase: http://100.79.118.92:11434` in config |
| **[Claude Code](https://claude.ai/claude-code)** | Set `OLLAMA_HOST` env var — see [guide](https://lgallardo.com/2026/02/28/claude-code-remote-control-ollama/) |
| **OpenAI-compatible clients** | Base URL: `http://100.79.118.92:11434/v1` |

### Security

- Tailscale encrypts all traffic end-to-end — no TLS/HTTPS needed
- Only tailnet devices can reach port 11434
- Use [Tailscale ACLs](https://tailscale.com/kb/1018/acls) for fine-grained access control
- Do **not** set `OLLAMA_HOST=0.0.0.0` without Tailscale or a firewall — it exposes the API to your entire LAN

---

## Remote Desktop & Terminal Access

### Desktop (GUI)

Connect via RustDesk UI or command line:

```powershell
# Windows
& "C:\Program Files\RustDesk\RustDesk.exe" --connect 100.79.118.92

# Linux / macOS
rustdesk --connect 100.79.118.92
```

### Terminal (Recommended for Windows)

RustDesk's built-in **Terminal (beta)** is the easiest way to get a remote shell on Windows — no SSH, no extra accounts, no key exchange.

**To use:**
1. Find the remote machine in your RustDesk peer list
2. Click the **three-dot menu** (...)
3. Select **"Terminal (beta)"** or **"Terminal (Run as administrator) (beta)"**

This opens a PowerShell session authenticated with RustDesk's permanent password, encrypted over the Tailscale tunnel.

**Why this beats SSH on Windows:**
- Works with Microsoft account logins — no password re-enabling or local accounts needed
- No SSH keys to generate, deploy, or manage
- Same credentials you already use for remote desktop
- Admin elevation available directly

**Limitations:**
- Runs in the SYSTEM profile context, not your user profile
- Per-user tools (e.g. `winget`) may not be in PATH
- System-wide tools (Ollama, Python, nvidia-smi, git) work fine

**Example — managing Ollama remotely:**
```
ollama list
nvidia-smi
python E:\Development\OllamaBenchmarks\scripts\benchmark_quality.py --models qwen3.5:9b
```

### Headless Host Configuration

For machines without a monitor, configure RustDesk entirely via CLI and config files:

```bash
# Set permanent password
rustdesk --password mypassword            # Linux
& "C:\Program Files\RustDesk\RustDesk.exe" --password mypassword  # Windows

# Get RustDesk ID
rustdesk --get-id

# Enable direct IP access (edit RustDesk2.toml — see Step 1 above)

# Allow remote config changes
rustdesk --option allow-remote-config-modification Y
```

---

## SSH Terminal Access via Tailscale (Alternative)

> **For most Windows setups, [RustDesk Terminal](#terminal-recommended-for-windows) is simpler.** SSH is useful on Linux/macOS hosts or when RustDesk isn't available.

### Linux / macOS

Tailscale has built-in SSH with zero key management:

```bash
tailscale set --ssh                    # enable on host (one-time)
ssh user@100.79.118.92                 # connect from any tailnet device
```

See [Tailscale SSH docs](https://tailscale.com/kb/1193/tailscale-ssh).

### Windows

Tailscale's built-in SSH server **does not support Windows**. You need Windows OpenSSH Server instead.

**Microsoft account limitation:** SSH key-based auth (`authorized_keys`) only works with local Windows accounts, not Microsoft accounts. Options:

| Approach | Pros | Cons |
|----------|------|------|
| **Password auth over Tailscale** | Works with Microsoft accounts | Type password each time |
| **Local account for SSH** | Key-based, passwordless | Extra account to manage |
| **RustDesk Terminal instead** | No setup, works with Microsoft accounts | SYSTEM context, beta |

#### Automated Setup Script

```powershell
# Run as Administrator
.\scripts\setup_tailscale_ssh.ps1 -SetupServer                            # install & configure
.\scripts\setup_tailscale_ssh.ps1 -ExchangeKeys -RemoteHost 100.79.118.92 # deploy keys
.\scripts\setup_tailscale_ssh.ps1 -SetupServer -ExchangeKeys -RemoteHost 100.80.83.33  # both
```

The script installs OpenSSH Server, scopes the firewall to Tailscale's IP range only (`100.64.0.0/10`), generates ED25519 keys, deploys them to the remote host (handling the Windows admin `authorized_keys` path), and tests the connection.

**Note:** For admin users on Windows, keys go in `C:\ProgramData\ssh\administrators_authorized_keys` (not `~/.ssh/authorized_keys`). The file needs restricted ACLs or sshd silently ignores it. The script handles this automatically.

See [Microsoft's OpenSSH key management guide](https://learn.microsoft.com/en-us/windows-server/administration/openssh/openssh_keymanagement) for details.

---

## Troubleshooting

### RustDesk: "Connection refused" on Tailscale IP
- Ensure `direct-server = 'Y'` in `RustDesk2.toml` on the **remote** machine
- Check port: `netstat -an | findstr 21118` (Windows) or `ss -tlnp | grep 21118` (Linux)
- Restart RustDesk with admin privileges

### Tailscale device not showing up
- Run `tailscale status` on both machines
- Ensure both are signed in to the same tailnet
- Device may show "offline" — reauthenticate if needed

### RustDesk hostnames don't match Tailscale
- Use interactive mode (without `--auto`) to manually enter IPs
- Or rename: `tailscale set --hostname=myname`

### Ollama: "connection refused" from remote client
- Verify `OLLAMA_HOST=0.0.0.0:11434` is set: `ollama serve` should show `Listening on 0.0.0.0:11434`
- Ensure Ollama was restarted after setting the env var
- Test locally first: `curl http://localhost:11434/api/tags`

### RustDesk won't restart on Windows
```powershell
# PowerShell as Admin
Stop-Process -Name RustDesk -Force
Start-Process "C:\Program Files\RustDesk\RustDesk.exe" -ArgumentList "--tray"
```
