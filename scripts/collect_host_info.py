from __future__ import annotations

import argparse
import datetime as dt
import json
import os
import platform
import shutil
import subprocess
from typing import Any


def run_command(command: list[str]) -> str | None:
    try:
        completed = subprocess.run(
            command,
            capture_output=True,
            text=True,
            timeout=30,
            check=False,
        )
    except Exception:
        return None
    if completed.returncode != 0:
        return None
    return completed.stdout.strip() or None


def run_powershell_json(script: str) -> Any:
    powershell = shutil.which("powershell") or shutil.which("pwsh")
    if not powershell:
        return None
    output = run_command([powershell, "-NoProfile", "-Command", script])
    if not output:
        return None
    try:
        return json.loads(output)
    except json.JSONDecodeError:
        return None


def slugify(text: str) -> str:
    return "".join(ch.lower() if ch.isalnum() else "-" for ch in text).strip("-")


def get_windows_hardware() -> tuple[dict[str, Any], list[dict[str, Any]]]:
    system = run_powershell_json(
        "Get-CimInstance Win32_ComputerSystem | "
        "Select-Object Manufacturer,Model,TotalPhysicalMemory | ConvertTo-Json -Compress"
    )
    gpus = run_powershell_json(
        "Get-CimInstance Win32_VideoController | "
        "Select-Object Name,AdapterRAM,DriverVersion,VideoProcessor,Status | ConvertTo-Json -Compress"
    )
    if isinstance(gpus, dict):
        gpus = [gpus]
    return system or {}, gpus or []


def get_ollama_version() -> str | None:
    output = run_command(["ollama", "--version"])
    if not output:
        return None
    return output.splitlines()[0]


def build_host_info() -> dict[str, Any]:
    uname = platform.uname()
    hostname = platform.node() or os.environ.get("COMPUTERNAME") or "unknown-host"
    system_details: dict[str, Any] = {}
    gpus: list[dict[str, Any]] = []
    total_memory_gb = None
    manufacturer = None
    model = None

    if platform.system() == "Windows":
        system_details, gpus = get_windows_hardware()
        manufacturer = system_details.get("Manufacturer")
        model = system_details.get("Model")
        total_physical_memory = system_details.get("TotalPhysicalMemory")
        if total_physical_memory:
            total_memory_gb = round(int(total_physical_memory) / (1024 ** 3), 2)

    host_slug = slugify(hostname) or "unknown-host"
    return {
        "captured_at": dt.datetime.now(dt.timezone.utc).isoformat(),
        "hostname": hostname,
        "host_slug": host_slug,
        "os": {
            "system": uname.system,
            "release": uname.release,
            "version": uname.version,
            "machine": uname.machine,
        },
        "manufacturer": manufacturer,
        "model": model,
        "processor": uname.processor or platform.processor() or None,
        "logical_cpu_count": os.cpu_count(),
        "total_memory_gb": total_memory_gb,
        "python_version": platform.python_version(),
        "ollama_version": get_ollama_version(),
        "gpus": gpus,
    }


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--compact", action="store_true")
    args = parser.parse_args()

    payload = build_host_info()
    if args.compact:
        print(json.dumps(payload, separators=(",", ":")))
    else:
        print(json.dumps(payload, indent=2))


if __name__ == "__main__":
    main()
