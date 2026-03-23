"""Layer 3 orchestrator for the custom .NET practical coding benchmark suite.

Runs all task YAML files in --task-dir against each model, writes per-model
checkpoints, and produces a combined output JSON.
"""

import argparse
import dataclasses
import datetime
import glob
import json
import os
import sys
from typing import Any

sys.path.insert(0, os.path.dirname(__file__))
from coding_tasks.task_runner import (
    TaskResult,
    model_slug,
    load_task,
    setup_template_cache,
    run_task,
)


def write_json(path: str, payload: dict[str, Any]) -> None:
    os.makedirs(os.path.dirname(path) or ".", exist_ok=True)
    with open(path, "w", encoding="utf-8") as fh:
        fh.write(json.dumps(payload, indent=2) + "\n")


def compute_layer3_score(results: list[TaskResult]) -> float:
    numerator = sum(r.weight * (1 if r.passed else 0) for r in results)
    denominator = sum(r.weight for r in results)
    return numerator / denominator if denominator > 0 else 0.0


def discover_tasks(task_dir: str) -> list[str]:
    """Glob all YAML files in task_dir, excluding files starting with '_'."""
    pattern = os.path.join(task_dir, "*.yaml")
    paths = [
        p for p in glob.glob(pattern)
        if not os.path.basename(p).startswith("_")
    ]
    return sorted(paths)


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Run Layer 3 .NET practical coding benchmark suite against Ollama models."
    )
    parser.add_argument("--models", nargs="+", required=True)
    parser.add_argument("--output", default="results/coding-layer3-results.json")
    parser.add_argument("--checkpoint-dir", default="results")
    parser.add_argument("--task-dir", default="scripts/coding_tasks/tasks")
    parser.add_argument("--references-dir", default="scripts/coding_tasks/references")
    parser.add_argument("--template-base", default="scripts/coding_tasks/templates")
    parser.add_argument("--save-code", action="store_true", default=True)
    args = parser.parse_args()

    run_started_at = datetime.datetime.now(datetime.timezone.utc)

    # Pre-restore both template caches once at startup
    cache_base = os.path.join(args.template_base, ".cache")
    cached_templates: dict[str, str] = {}
    for template_name in ("test_project", "blazor_project"):
        template_dir = os.path.join(args.template_base, template_name)
        cache_dir = os.path.join(cache_base, template_name)
        print(f"[setup] Restoring template cache: {template_name} -> {cache_dir}")
        cached_templates[template_name] = setup_template_cache(template_dir, cache_dir)

    # Discover task YAML files
    task_paths = discover_tasks(args.task_dir)
    if not task_paths:
        print(f"[warning] No task YAML files found in {args.task_dir}", file=sys.stderr)

    print(f"[info] Found {len(task_paths)} task(s) in {args.task_dir}")
    print(f"[info] Running {len(args.models)} model(s): {', '.join(args.models)}")

    all_model_results: list[dict[str, Any]] = []

    for model in args.models:
        model_run_started_at = datetime.datetime.now(datetime.timezone.utc)
        slug = model_slug(model)
        print(f"\n[model] {model} (slug={slug})")

        task_results: list[TaskResult] = []

        for yaml_path in task_paths:
            task_name = os.path.splitext(os.path.basename(yaml_path))[0]
            print(f"  [task] {task_name} ...", end="", flush=True)

            task_def = load_task(yaml_path, args.references_dir)
            template_name = task_def.get("template", "test_project")
            cached_template_path = cached_templates.get(template_name)

            if cached_template_path is None:
                # Fallback: try to resolve on-the-fly
                template_dir = os.path.join(args.template_base, template_name)
                cache_dir = os.path.join(cache_base, template_name)
                cached_template_path = setup_template_cache(template_dir, cache_dir)
                cached_templates[template_name] = cached_template_path

            result = run_task(
                task_def,
                model,
                cached_template_path,
                args.checkpoint_dir,
                args.save_code,
            )
            task_results.append(result)

            status = "PASS" if result.passed else "FAIL"
            extra = f" ({result.harness_error})" if result.harness_error else ""
            print(f" {status}{extra}")

        layer3_score = compute_layer3_score(task_results)
        model_run_finished_at = datetime.datetime.now(datetime.timezone.utc)

        print(
            f"  [score] layer3_weighted_score={layer3_score:.4f} "
            f"({sum(1 if r.passed else 0 for r in task_results)}/{len(task_results)} tasks passed)"
        )

        checkpoint_payload: dict[str, Any] = {
            "model": model,
            "benchmark": "coding",
            "run_started_at": model_run_started_at.isoformat(),
            "run_finished_at": model_run_finished_at.isoformat(),
            "layer3_results": [dataclasses.asdict(r) for r in task_results],
            "layer3_weighted_score": layer3_score,
        }

        checkpoint_path = os.path.join(args.checkpoint_dir, f"coding-{slug}.json")
        write_json(checkpoint_path, checkpoint_payload)
        print(f"  [checkpoint] Written to {checkpoint_path}")

        all_model_results.append(checkpoint_payload)

    # Write combined output file
    combined_payload: dict[str, Any] = {
        "benchmark": "coding",
        "run_started_at": run_started_at.isoformat(),
        "run_finished_at": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "models": args.models,
        "results": all_model_results,
    }
    write_json(args.output, combined_payload)
    print(f"\n[done] Combined results written to {args.output}")


if __name__ == "__main__":
    main()
