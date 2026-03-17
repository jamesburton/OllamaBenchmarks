"""Rebuild quality-current.json and throughput-resource-current.json from per-model result files."""

import json
import pathlib
from datetime import datetime, timezone

from collect_host_info import build_host_info

RESULTS_DIR = pathlib.Path(__file__).resolve().parent.parent / "results"


def load_json(path: pathlib.Path) -> dict:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def write_json(path: pathlib.Path, data: dict) -> None:
    with path.open("w", encoding="utf-8") as handle:
        json.dump(data, handle, indent=2)
        handle.write("\n")


def rebuild_quality() -> None:
    per_model_files = sorted(RESULTS_DIR.glob("quality-*.json"))
    skip = {"quality-current.json"}
    # Also skip openai and requested files — they have different structure
    all_results = []
    seen_models = set()
    earliest_start = None
    latest_finish = None
    host_details = None

    for path in per_model_files:
        if path.name in skip:
            continue
        if path.name.startswith("quality-openai-") or path.name.startswith("quality-requested-"):
            continue
        data = load_json(path)
        results = data.get("results", [])
        if not results:
            continue
        for row in results:
            model = row.get("model")
            if model and model not in seen_models:
                seen_models.add(model)
                all_results.append(row)
        start = data.get("run_started_at")
        finish = data.get("run_finished_at")
        if start and (earliest_start is None or start < earliest_start):
            earliest_start = start
        if finish and (latest_finish is None or finish > latest_finish):
            latest_finish = finish
        if data.get("host_details"):
            host_details = data["host_details"]

    if not host_details:
        host_details = build_host_info()

    models_list = [row["model"] for row in all_results]
    payload = {
        "benchmark": "quality",
        "run_started_at": earliest_start or datetime.now(timezone.utc).isoformat(),
        "run_finished_at": latest_finish or datetime.now(timezone.utc).isoformat(),
        "output_path": str(RESULTS_DIR / "quality-current.json"),
        "ollama_host": "http://127.0.0.1:11434",
        "host_details": host_details,
        "models": models_list,
        "completed_models": models_list,
        "failed_models": [],
        "incomplete_models": [],
        "results": all_results,
    }
    out = RESULTS_DIR / "quality-current.json"
    write_json(out, payload)
    print(f"Rebuilt {out.name}: {len(all_results)} models")


def rebuild_throughput() -> None:
    per_model_files = sorted(RESULTS_DIR.glob("throughput-resource-*.json"))
    skip = {"throughput-resource-current.json"}
    all_results = []
    seen_models = set()
    earliest_start = None
    latest_finish = None
    host_details = None

    for path in per_model_files:
        if path.name in skip:
            continue
        if path.name.startswith("throughput-resource-requested-"):
            continue
        data = load_json(path)
        results = data.get("results", [])
        if not results:
            continue
        for row in results:
            model = row.get("model")
            if model and model not in seen_models:
                seen_models.add(model)
                all_results.append(row)
        start = data.get("run_started_at")
        finish = data.get("run_finished_at")
        if start and (earliest_start is None or start < earliest_start):
            earliest_start = start
        if finish and (latest_finish is None or finish > latest_finish):
            latest_finish = finish
        if data.get("host_details"):
            host_details = data["host_details"]

    if not host_details:
        host_details = build_host_info()

    models_list = [row["model"] for row in all_results]
    payload = {
        "benchmark": "throughput_resource",
        "run_started_at": earliest_start or datetime.now(timezone.utc).isoformat(),
        "run_finished_at": latest_finish or datetime.now(timezone.utc).isoformat(),
        "output_path": str(RESULTS_DIR / "throughput-resource-current.json"),
        "host": host_details.get("hostname", "UNKNOWN"),
        "ollama_host": "http://127.0.0.1:11434",
        "host_details": host_details,
        "prompt": "Write a concise explanation of dependency injection with one short Python example.",
        "num_predict": 192,
        "seed": 42,
        "models": models_list,
        "completed_models": models_list,
        "failed_models": [],
        "incomplete_models": [],
        "results": all_results,
    }
    out = RESULTS_DIR / "throughput-resource-current.json"
    write_json(out, payload)
    print(f"Rebuilt {out.name}: {len(all_results)} models")


if __name__ == "__main__":
    rebuild_quality()
    rebuild_throughput()
