import argparse
import datetime as dt
import json
import os
import re
from pathlib import Path
from typing import Any


def model_slug(model: str) -> str:
    model = re.sub(r":latest$", "", model)
    return re.sub(r"[^\w\.-]", "_", model.replace(":", "_").replace("/", "_").replace("\\", "_"))


def load_json(path: Path) -> dict[str, Any]:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def write_json(path: Path, payload: dict[str, Any]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, indent=2) + "\n", encoding="utf-8")


def normalize_failed_models(payload: dict[str, Any]) -> list[dict[str, Any]]:
    failed = payload.get("failed_models")
    if isinstance(failed, list):
        return failed
    return []


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--benchmark", choices=["throughput_resource", "quality"], required=True)
    parser.add_argument("--output", required=True)
    parser.add_argument("--checkpoint-dir", default="results")
    parser.add_argument("--base")
    parser.add_argument("--models", nargs="+", required=True)
    args = parser.parse_args()

    prefix = {
        "throughput_resource": "throughput-resource",
        "quality": "quality",
    }[args.benchmark]

    base_payload: dict[str, Any] = {}
    if args.base:
        base_payload = load_json(Path(args.base))
    elif Path(args.output).exists():
        base_payload = load_json(Path(args.output))

    results: list[dict[str, Any]] = []
    failed_models: list[dict[str, Any]] = []

    checkpoint_dir = Path(args.checkpoint_dir)
    for model in args.models:
        checkpoint_path = checkpoint_dir / f"{prefix}-{model_slug(model)}.json"
        if not checkpoint_path.exists():
            continue
        payload = load_json(checkpoint_path)
        results.extend(payload.get("results") or [])
        failed_models.extend(normalize_failed_models(payload))

    completed_models = [row["model"] for row in results]
    failed_model_names = [row["model"] for row in failed_models if isinstance(row, dict) and row.get("model")]
    incomplete_models = [model for model in args.models if model not in completed_models and model not in failed_model_names]

    merged = dict(base_payload)
    merged["benchmark"] = args.benchmark
    merged["run_finished_at"] = dt.datetime.now(dt.timezone.utc).isoformat()
    merged["output_path"] = str(Path(args.output).resolve())
    merged["models"] = args.models
    merged["completed_models"] = completed_models
    merged["failed_models"] = failed_models
    merged["incomplete_models"] = incomplete_models
    merged["results"] = results

    if "run_started_at" not in merged:
        merged["run_started_at"] = dt.datetime.now(dt.timezone.utc).isoformat()

    write_json(Path(args.output), merged)


if __name__ == "__main__":
    main()
