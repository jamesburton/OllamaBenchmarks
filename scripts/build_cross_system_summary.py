from __future__ import annotations

import argparse
import json
import pathlib
from typing import Any


def load_manifest(path: pathlib.Path) -> dict[str, Any]:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def gpu_label(gpus: list[dict[str, Any]]) -> str:
    names = [gpu.get("Name") for gpu in gpus if gpu.get("Name")]
    return ", ".join(names) if names else "Unknown"


def format_model(model: dict[str, Any] | None) -> str:
    if not model:
        return "-"
    score = ""
    if model.get("score_max"):
        score = f" ({model['score']}/{model['score_max']})"
    return f"{model['model']} ({model['toks_per_s']} tok/s){score}"


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--systems-root", default="results/systems")
    parser.add_argument("--output", default="results/cross-system-summary.md")
    args = parser.parse_args()

    systems_root = pathlib.Path(args.systems_root)
    manifests = sorted(systems_root.glob("*/manifest.json"))
    rows = [load_manifest(path) for path in manifests]

    lines = [
        "# Cross-System Benchmark Summary",
        "",
        f"Systems captured: {len(rows)}",
        "",
        "| System | OS | CPU / RAM | GPU | Primary coding model | Best value / balanced | Fastest raw model | Backend note |",
        "|---|---|---|---|---|---|---|---|",
    ]

    for row in rows:
        host = row["host"]
        summary = row.get("summary", {})
        os_label = f"{host['os']['system']} {host['os']['release']}"
        cpu_ram = f"{host.get('processor') or 'Unknown CPU'} / {host.get('total_memory_gb') or '?'} GB"
        backend_note = summary.get("backend_summary") or "-"
        primary = format_model(summary.get("primary_coding_model") or summary.get("best_balanced_model"))
        balanced = format_model(summary.get("best_balanced_model"))
        fastest = format_model(summary.get("fastest_model"))
        lines.append(
            "| {system} | {os_label} | {cpu_ram} | {gpu} | {primary} | {balanced} | {fastest} | {backend} |".format(
                system=row["system_id"],
                os_label=os_label,
                cpu_ram=cpu_ram,
                gpu=gpu_label(host.get("gpus") or []),
                primary=primary,
                balanced=balanced,
                fastest=fastest,
                backend=backend_note,
            )
        )

    lines.extend(
        [
            "",
            "## Update Process",
            "",
            "1. Run the benchmark scripts to refresh the `*-current.json` artifacts for the target system.",
            "2. Run `python .\\scripts\\archive_system_benchmarks.py --label <system-date-or-tag>` to snapshot the host metadata and current artifacts under `results/systems/`.",
            "3. Run `python .\\scripts\\build_cross_system_summary.py` to rebuild this comparison table after each new system capture.",
            "",
        ]
    )

    output_path = pathlib.Path(args.output)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(output_path.resolve())


if __name__ == "__main__":
    main()
