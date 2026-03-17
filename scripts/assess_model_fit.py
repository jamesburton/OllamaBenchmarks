from __future__ import annotations

import argparse
import json
import pathlib
import re
from typing import Any

from collect_host_info import build_host_info


SIZE_RE = re.compile(r"\b([0-9]+(?:\.[0-9]+)?)\s*GB\b", re.IGNORECASE)


def load_json(path: pathlib.Path) -> dict[str, Any]:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def parse_observed_resident_gb(result: dict[str, Any]) -> float | None:
    ollama_ps = result.get("ollama_ps")
    if isinstance(ollama_ps, str):
        match = SIZE_RE.search(ollama_ps)
        if match:
            return float(match.group(1))

    ram_peak = result.get("ram_peak_gb")
    gpu_peak = result.get("gpu_mem_peak_gb")
    values = [value for value in (ram_peak, gpu_peak) if isinstance(value, (int, float)) and value > 0]
    if values:
        return round(sum(values), 2)
    return None


def gpu_tier(host: dict[str, Any]) -> int:
    names = " ".join((gpu.get("Name") or "") for gpu in host.get("gpus") or []).lower()
    if "radeon" in names and "8060s" in names:
        return 4
    if "nvidia" in names and "rtx 3060" in names:
        return 3
    if "arc" in names:
        return 2
    if "iris" in names:
        return 1
    return 0


def cpu_tier(host: dict[str, Any]) -> int:
    logical = host.get("logical_cpu_count") or 0
    if logical >= 24:
        return 3
    if logical >= 12:
        return 2
    if logical >= 8:
        return 1
    return 0


def load_system_evidence(systems_root: pathlib.Path) -> tuple[dict[str, list[dict[str, Any]]], set[str]]:
    evidence: dict[str, list[dict[str, Any]]] = {}
    incomplete: set[str] = set()

    for system_dir in sorted(path for path in systems_root.iterdir() if path.is_dir()):
        manifest_path = system_dir / "manifest.json"
        if not manifest_path.exists():
            continue
        manifest = load_json(manifest_path)
        host = manifest.get("host") or {}

        throughput_path = system_dir / manifest["artifacts"]["throughput"]
        quality_path = system_dir / manifest["artifacts"]["quality"]
        throughput = load_json(throughput_path)
        quality = load_json(quality_path)

        quality_by_model = {
            row["model"]: row
            for row in quality.get("results") or []
            if isinstance(row, dict) and row.get("model")
        }

        for model in throughput.get("incomplete_models") or []:
            incomplete.add(model)

        for row in throughput.get("results") or []:
            model = row.get("model")
            if not model:
                continue
            quality_row = quality_by_model.get(model, {})
            evidence.setdefault(model, []).append(
                {
                    "system_id": manifest.get("system_id"),
                    "host_slug": host.get("host_slug"),
                    "host_memory_gb": host.get("total_memory_gb"),
                    "host_cpu_tier": cpu_tier(host),
                    "host_gpu_tier": gpu_tier(host),
                    "toks_per_s": row.get("toks_per_s"),
                    "score": quality_row.get("score"),
                    "score_max": quality_row.get("score_max"),
                    "observed_resident_gb": parse_observed_resident_gb(row),
                }
            )

    return evidence, incomplete


def classify_model(
    model: str,
    current_host: dict[str, Any],
    model_evidence: list[dict[str, Any]],
    incomplete_models: set[str],
    backend_notes: dict[str, str],
) -> dict[str, Any]:
    current_memory = float(current_host.get("total_memory_gb") or 0.0)
    current_cpu_tier = cpu_tier(current_host)
    current_gpu_tier = gpu_tier(current_host)
    practical_memory = round(current_memory * 0.75, 2)

    best_score = max(
        (entry.get("score", -1) for entry in model_evidence if isinstance(entry.get("score"), (int, float))),
        default=None,
    )
    smallest_observed = min(
        (entry["observed_resident_gb"] for entry in model_evidence if isinstance(entry.get("observed_resident_gb"), (int, float))),
        default=None,
    )
    weakest_success = None
    for entry in model_evidence:
        key = (
            float(entry.get("host_memory_gb") or 0.0),
            entry.get("host_gpu_tier") or 0,
            entry.get("host_cpu_tier") or 0,
        )
        if weakest_success is None or key < weakest_success[0]:
            weakest_success = (key, entry)

    reasons: list[str] = []
    if smallest_observed is not None:
        reasons.append(f"smallest observed resident set was {smallest_observed:.2f} GB")
    if weakest_success:
        witness = weakest_success[1]
        reasons.append(
            "completed on {system} ({memory:.2f} GB RAM, GPU tier {gpu}, CPU tier {cpu})".format(
                system=witness["system_id"],
                memory=float(witness.get("host_memory_gb") or 0.0),
                gpu=witness.get("host_gpu_tier") or 0,
                cpu=witness.get("host_cpu_tier") or 0,
            )
        )
    if isinstance(best_score, (int, float)):
        reasons.append(f"best quick-quality score was {int(best_score)}/5")
    if model in backend_notes:
        reasons.append(backend_notes[model])

    if model in incomplete_models:
        status = "unlikely"
        reasons.insert(0, "benchmark did not complete on at least one stronger archived machine")
    elif smallest_observed is not None and smallest_observed <= practical_memory:
        if weakest_success and weakest_success[1].get("host_memory_gb", 0) <= current_memory:
            status = "likely"
        else:
            status = "borderline"
    elif smallest_observed is not None and smallest_observed <= current_memory:
        status = "borderline"
    else:
        status = "unlikely"

    if status != "unlikely" and weakest_success:
        witness = weakest_success[1]
        if witness.get("host_gpu_tier", 0) > current_gpu_tier + 1 and (smallest_observed or 0) > 12:
            status = "borderline"
            reasons.append("current machine has materially weaker graphics than the weakest archived success")
        if witness.get("host_cpu_tier", 0) > current_cpu_tier + 1 and (witness.get("toks_per_s") or 0) < 10:
            status = "borderline"
            reasons.append("current machine also has a weaker CPU tier, so very slow decode is possible")

    return {
        "model": model,
        "status": status,
        "best_score": best_score,
        "smallest_observed_resident_gb": smallest_observed,
        "reasons": reasons,
        "evidence": model_evidence,
    }


def build_report(repo_root: pathlib.Path) -> dict[str, Any]:
    benchmark_models = load_json(repo_root / "benchmark-models.json")
    current_host = build_host_info()
    evidence, incomplete_models = load_system_evidence(repo_root / "results" / "systems")

    model_names = sorted(
        set(benchmark_models.get("effective_models") or [])
        | set(benchmark_models.get("local_installed") or [])
        | set(evidence.keys())
        | set(benchmark_models.get("backend_notes", {}).keys())
    )

    assessments = [
        classify_model(
            model,
            current_host,
            evidence.get(model, []),
            incomplete_models,
            benchmark_models.get("backend_notes") or {},
        )
        for model in model_names
        if evidence.get(model) or model in incomplete_models or model in (benchmark_models.get("backend_notes") or {})
    ]

    grouped: dict[str, list[str]] = {"likely": [], "borderline": [], "unlikely": []}
    for entry in assessments:
        grouped[entry["status"]].append(entry["model"])

    return {
        "host": current_host,
        "summary": {
            "likely": grouped["likely"],
            "borderline": grouped["borderline"],
            "unlikely": grouped["unlikely"],
        },
        "assessments": assessments,
    }


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", default=pathlib.Path(__file__).resolve().parents[1], type=pathlib.Path)
    parser.add_argument("--output", type=pathlib.Path)
    args = parser.parse_args()

    report = build_report(args.repo_root.resolve())
    payload = json.dumps(report, indent=2)

    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(payload + "\n", encoding="utf-8")
        print(args.output.resolve())
        return

    print(payload)


if __name__ == "__main__":
    main()
