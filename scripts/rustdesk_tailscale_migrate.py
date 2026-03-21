"""Migrate RustDesk peers from numeric IDs to Tailscale direct-IP connections.

Backs up the current RustDesk config, auto-detects Tailscale IPs for known
peers by matching hostnames, and creates new peer entries using Tailscale IPs.

Usage:
    python rustdesk_tailscale_migrate.py                    # interactive
    python rustdesk_tailscale_migrate.py --auto             # auto-match all
    python rustdesk_tailscale_migrate.py --dry-run          # preview only
    python rustdesk_tailscale_migrate.py --backup-only      # just backup
"""

from __future__ import annotations

import argparse
import datetime as dt
import os
import platform
import re
import shutil
import subprocess
from pathlib import Path


def get_rustdesk_config_dir() -> Path:
    """Find the RustDesk config directory."""
    if platform.system() == "Windows":
        base = Path(os.environ.get("APPDATA", ""))
        candidates = [base / "RustDesk" / "config"]
    else:
        candidates = [
            Path.home() / ".config" / "rustdesk",
            Path.home() / ".rustdesk" / "config",
        ]
    for c in candidates:
        if c.exists():
            return c
    raise FileNotFoundError("RustDesk config directory not found")


def get_tailscale_devices() -> dict[str, str]:
    """Return {hostname: tailscale_ip} from 'tailscale status'."""
    try:
        result = subprocess.run(
            ["tailscale", "status"],
            capture_output=True, text=True, timeout=10,
        )
    except Exception:
        return {}

    devices = {}
    for line in result.stdout.strip().splitlines():
        parts = line.split()
        if len(parts) >= 2 and parts[0].startswith("100."):
            ip = parts[0]
            hostname = parts[1].lower()
            devices[hostname] = ip
    return devices


def parse_peer_hostname(peer_file: Path) -> str | None:
    """Extract hostname from a RustDesk peer TOML file."""
    in_info = False
    for line in peer_file.read_text(encoding="utf-8").splitlines():
        if line.strip() == "[info]":
            in_info = True
            continue
        if line.strip().startswith("[") and in_info:
            break
        if in_info and line.startswith("hostname"):
            match = re.search(r"hostname\s*=\s*'([^']*)'", line)
            if match:
                return match.group(1).lower()
    return None


def backup_config(config_dir: Path) -> Path:
    """Create a timestamped backup of the entire RustDesk config."""
    ts = dt.datetime.now().strftime("%Y%m%d-%H%M%S")
    backup_dir = config_dir.parent / f"config-backup-{ts}"
    shutil.copytree(config_dir, backup_dir)
    return backup_dir


def create_tailscale_peer(config_dir: Path, tailscale_ip: str,
                          source_peer: Path | None) -> Path:
    """Create a new peer file keyed by Tailscale IP.

    Copies settings from the source peer if provided, otherwise creates
    a minimal entry.
    """
    peer_dir = config_dir / "peers"
    new_file = peer_dir / f"{tailscale_ip}.toml"

    if source_peer and source_peer.exists():
        content = source_peer.read_text(encoding="utf-8")
        # Reset direct_failures counter
        content = re.sub(r"direct_failures = \d+", "direct_failures = 0", content)
        new_file.write_text(content, encoding="utf-8")
    else:
        new_file.write_text(
            "direct_failures = 0\n\n"
            "[info]\n"
            f"hostname = '{tailscale_ip}'\n"
            "platform = 'Windows'\n",
            encoding="utf-8",
        )
    return new_file


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Migrate RustDesk peers to Tailscale direct-IP connections"
    )
    parser.add_argument("--auto", action="store_true",
                        help="Auto-match all peers by hostname without prompting")
    parser.add_argument("--dry-run", action="store_true",
                        help="Show what would be done without making changes")
    parser.add_argument("--backup-only", action="store_true",
                        help="Only create a backup, don't migrate")
    args = parser.parse_args()

    # Find config
    config_dir = get_rustdesk_config_dir()
    peer_dir = config_dir / "peers"
    print(f"RustDesk config: {config_dir}")

    # Backup
    if not args.dry_run:
        backup = backup_config(config_dir)
        print(f"Backup created: {backup}")
    else:
        print("[dry-run] Would create backup")

    if args.backup_only:
        print("Backup complete.")
        return

    # Get Tailscale devices
    ts_devices = get_tailscale_devices()
    if not ts_devices:
        print("ERROR: No Tailscale devices found. Is Tailscale running?")
        return

    print(f"\nTailscale devices found: {len(ts_devices)}")
    for hostname, ip in sorted(ts_devices.items()):
        print(f"  {hostname:20s} -> {ip}")

    # Get current peers
    peer_files = sorted(peer_dir.glob("*.toml"))
    numeric_peers = [(f, f.stem, parse_peer_hostname(f))
                     for f in peer_files if f.stem.isdigit()]

    # Check for existing Tailscale IP peers
    existing_ts = [f.stem for f in peer_files if f.stem.startswith("100.")]

    print(f"\nRustDesk numeric peers: {len(numeric_peers)}")
    print(f"Existing Tailscale peers: {existing_ts or 'none'}")

    # Match and migrate
    migrations = []
    for peer_file, peer_id, hostname in numeric_peers:
        ts_ip = ts_devices.get(hostname) if hostname else None
        if ts_ip:
            migrations.append((peer_file, peer_id, hostname, ts_ip, "auto"))
        elif hostname:
            print(f"\n  {peer_id} ({hostname}) - no Tailscale match found")
            if not args.auto:
                user_ip = input(f"    Enter Tailscale IP for '{hostname}' (or skip): ").strip()
                if user_ip and user_ip.startswith("100."):
                    migrations.append((peer_file, peer_id, hostname, user_ip, "manual"))
        else:
            print(f"\n  {peer_id} - no hostname found in config")

    # Summary
    print(f"\n{'=' * 60}")
    print("Migration plan:")
    print(f"{'=' * 60}")
    for peer_file, peer_id, hostname, ts_ip, match_type in migrations:
        existing = "EXISTS" if (peer_dir / f"{ts_ip}.toml").exists() else "NEW"
        print(f"  {peer_id:12s} ({hostname:15s}) -> {ts_ip:16s} [{match_type}] {existing}")

    unmatched = [(pid, h) for _, pid, h, *_ in numeric_peers
                 if not any(pid == m[1] for m in migrations)]
    if unmatched:
        print(f"\nUnmatched peers (no Tailscale, kept as-is):")
        for pid, h in unmatched:
            print(f"  {pid:12s} ({h or 'unknown'})")

    if not migrations:
        print("\nNothing to migrate.")
        return

    # Execute
    if args.dry_run:
        print("\n[dry-run] No changes made.")
        return

    if not args.auto:
        confirm = input(f"\nProceed with {len(migrations)} migration(s)? [y/N] ").strip()
        if confirm.lower() != "y":
            print("Aborted.")
            return

    for peer_file, peer_id, hostname, ts_ip, _ in migrations:
        new_file = create_tailscale_peer(config_dir, ts_ip, peer_file)
        print(f"  Created {new_file.name} (from {peer_id}/{hostname})")

    print(f"\nDone! {len(migrations)} Tailscale peer(s) created.")
    print("Original numeric peers are preserved (not deleted).")
    print("\nTo connect: open RustDesk and enter the Tailscale IP (e.g. 100.x.x.x)")
    print("NOTE: Restart RustDesk for changes to take effect.")


if __name__ == "__main__":
    main()
