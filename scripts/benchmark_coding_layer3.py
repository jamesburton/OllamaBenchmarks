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


def read_checkpoint(path: str) -> dict[str, Any]:
    """Read an existing per-model checkpoint, or {} if absent/unreadable.

    Layer 1/2/4 all merge their fields into the same coding-<slug>.json file
    (layer1_*, layer2_*, etc. namespaced per layer) rather than each layer
    overwriting the whole file. Layer 3 must do the same or it silently wipes
    whatever Layer 1/2/4 already wrote for this model.
    """
    if os.path.isfile(path):
        try:
            with open(path, "r", encoding="utf-8") as fh:
                return json.load(fh)
        except (OSError, json.JSONDecodeError):
            pass
    return {}


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
    parser.add_argument("--resume", action="store_true",
                         help="Skip tasks already present in an existing coding-<slug>.json checkpoint "
                              "(from a prior run that was interrupted) instead of re-running them.")
    parser.add_argument("--limit", type=int, default=0,
                         help="Limit number of tasks (0 = all). Applied after --resume filtering, "
                              "i.e. counts newly-run tasks, not previously-completed ones.")
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
        slug = model_slug(model)
        print(f"\n[model] {model} (slug={slug})")

        # -think suffix: track "thinking mode enabled" runs as a separate
        # checkpoint file from the non-thinking baseline, rather than one
        # overwriting the other.
        think_env = os.environ.get("CODING_BENCH_THINK", "").strip().lower()
        suffix = "-think" if think_env in ("1", "true", "yes", "on", "low", "medium", "high") else ""
        checkpoint_path = os.path.join(args.checkpoint_dir, f"coding-{slug}{suffix}.json")

        task_results: list[TaskResult] = []
        existing = read_checkpoint(checkpoint_path)
        model_run_started_at_str = existing.get("layer3_run_started_at")
        model_run_started_at = (
            datetime.datetime.fromisoformat(model_run_started_at_str)
            if model_run_started_at_str
            else datetime.datetime.now(datetime.timezone.utc)
        )

        # --resume is explicit opt-in (not automatic): a checkpoint existing on
        # disk doesn't by itself mean this invocation wants to skip its tasks.
        # NOTE: matching MUST use the YAML's internal "name:" field
        # (TaskResult.task), not the filename -- see the comment at the
        # skip-check below for why a filename-based match silently breaks this.
        already_done: dict[str, Any] = {}
        if args.resume:
            for r in existing.get("layer3_results", []):
                if r.get("task"):
                    already_done[r["task"]] = r
            if already_done:
                print(f"[resume] {len(already_done)} task(s) already completed in a prior run — skipping them")

        newly_run_count = 0
        for yaml_path in task_paths:
            filename_task_name = os.path.splitext(os.path.basename(yaml_path))[0]

            # Resume matching MUST use the YAML's internal "name:" field, not the
            # filename. TaskResult.task (what run_task/task_runner.py actually
            # stores in the checkpoint) comes from task_def["name"], which is
            # frequently NOT the same string as the filename (e.g. filename
            # "01_aspnet_oneof_controller" vs. name: "aspnet_oneof_controller").
            # An earlier version of this check compared against the filename and
            # silently never matched, so --resume printed "N tasks skipped" but
            # then re-ran every task anyway, overwriting the real checkpoint with
            # a fresh, shorter, duplicate-effort run. Load task_def before the
            # skip check so the comparison is done on the correct key.
            task_def = load_task(yaml_path, args.references_dir)
            actual_task_name = task_def.get("name", filename_task_name)

            if actual_task_name in already_done:
                task_results.append(TaskResult(**already_done[actual_task_name]))
                continue

            if args.limit > 0 and newly_run_count >= args.limit:
                continue
            newly_run_count += 1

            print(f"  [task] {filename_task_name} ...", end="", flush=True)

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

            # Incremental checkpoint after every task. Previously this only wrote
            # a checkpoint once, after the entire task list finished for a model —
            # a live run against dotLLM was killed mid-run by an external factor
            # and lost all progress under that scheme (see Layer 2's identical fix).
            # Write partial progress every iteration so an interrupted run is at
            # least partially recoverable/inspectable instead of silently void.
            partial_score = compute_layer3_score(task_results)
            partial_checkpoint = read_checkpoint(checkpoint_path)
            partial_checkpoint.update({
                "model": model,
                "benchmark": "coding",
                "layer3_run_started_at": model_run_started_at.isoformat(),
                "layer3_run_finished_at": None,
                "layer3_in_progress": True,
                "layer3_total_so_far": len(task_results),
                "layer3_total": len(task_paths),
                "layer3_results": [dataclasses.asdict(r) for r in task_results],
                "layer3_weighted_score": partial_score,
                "think_setting": think_env or "false",
            })
            write_json(checkpoint_path, partial_checkpoint)

        layer3_score = compute_layer3_score(task_results)
        model_run_finished_at = datetime.datetime.now(datetime.timezone.utc)

        print(
            f"  [score] layer3_weighted_score={layer3_score:.4f} "
            f"({sum(1 if r.passed else 0 for r in task_results)}/{len(task_results)} tasks passed)"
        )

        # Re-read from disk in case another concurrently-running layer (1/2/4 all
        # share this per-model file) wrote in the meantime; think_env/suffix/
        # checkpoint_path were already resolved before the task loop above.
        checkpoint_payload: dict[str, Any] = read_checkpoint(checkpoint_path)
        checkpoint_payload.update({
            "model": model,
            "benchmark": "coding",
            "layer3_run_started_at": model_run_started_at.isoformat(),
            "layer3_run_finished_at": model_run_finished_at.isoformat(),
            "layer3_in_progress": False,
            "layer3_results": [dataclasses.asdict(r) for r in task_results],
            "layer3_weighted_score": layer3_score,
            "think_setting": think_env or "false",
        })
        checkpoint_payload.pop("layer3_total_so_far", None)
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
