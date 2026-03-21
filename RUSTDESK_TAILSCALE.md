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
