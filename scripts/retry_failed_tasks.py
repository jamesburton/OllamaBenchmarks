#!/usr/bin/env python3
"""Retry only failed/suspect tasks for a model's L3 checkpoint.

Identifies tasks that failed due to transient errors (empty extraction,
503, 429, timeouts) and re-runs only those, preserving passing results.

Usage:
    python scripts/retry_failed_tasks.py --model glm-5:cloud
    python scripts/retry_failed_tasks.py --model glm-5.1:cloud --max-retries 2
"""

import argparse
import json
import os
import re
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
RESULTS_DIR = REPO_ROOT / "results"
sys.path.insert(0, str(REPO_ROOT / "scripts"))

from coding_tasks.task_runner import load_task, call_ollama, run_dotnet_task, setup_template_cache
from coding_tasks.code_extractor import extract_csharp


def model_slug(model: str) -> str:
    return re.sub(r"[^\w\.-]", "_", model.replace(":", "_").replace("/", "_"))


def is_suspect_failure(result: dict) -> bool:
    """Detect failures likely caused by transient issues, not model capability."""
    if result.get("passed"):
        return False
    err = result.get("harness_error", "") or ""
    gen_time = result.get("generation_time_s", 999)
    # Empty extraction = likely thinking-field or API error
    if "Empty code" in err:
        return True
    # Very fast generation = likely API error (503, 429, timeout)
    if gen_time < 3:
        return True
    return False


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--model", required=True)
    parser.add_argument("--max-retries", type=int, default=2)
    args = parser.parse_args()

    slug = model_slug(args.model)
    checkpoint = RESULTS_DIR / f"coding-{slug}.json"

    if not checkpoint.exists():
        print(f"No checkpoint found for {args.model}")
        sys.exit(1)

    data = json.loads(checkpoint.read_text())
    results = data.get("layer3_results", [])
    passed_before = sum(1 for r in results if r.get("passed"))
    total = len(results)

    suspect = [(i, r) for i, r in enumerate(results) if is_suspect_failure(r)]

    if not suspect:
        print(f"{args.model}: {passed_before}/{total} — no suspect failures to retry")
        return

    print(f"{args.model}: {passed_before}/{total}, retrying {len(suspect)} suspect failures")

    tasks_dir = str(REPO_ROOT / "scripts" / "coding_tasks" / "tasks")
    references_dir = str(REPO_ROOT / "scripts" / "coding_tasks" / "references")
    templates_dir = str(REPO_ROOT / "scripts" / "coding_tasks" / "templates")
    cache_dir = os.path.join(templates_dir, ".cache")
    generated_dir = str(RESULTS_DIR / "coding-generated" / slug)
    os.makedirs(generated_dir, exist_ok=True)

    # Setup template caches
    for template_name in ("test_project", "blazor_project"):
        template_dir = os.path.join(templates_dir, template_name)
        if os.path.isdir(template_dir):
            setup_template_cache(template_dir, cache_dir)

    fixed = 0
    for idx, old_result in suspect:
        task_name = old_result["task"]
        # Find the YAML file
        yaml_path = None
        for f in sorted(os.listdir(tasks_dir)):
            if f.endswith(".yaml") and task_name in f:
                yaml_path = os.path.join(tasks_dir, f)
                break

        if not yaml_path:
            print(f"  [{task_name}] YAML not found, skipping")
            continue

        task = load_task(yaml_path, references_dir)

        for attempt in range(1, args.max_retries + 1):
            print(f"  [{task_name}] attempt {attempt}/{args.max_retries}...", end="", flush=True)

            raw = call_ollama(
                args.model, task["prompt"],
                max_tokens=task.get("max_tokens", 4096),
                num_ctx=task.get("num_ctx", 12288),
            )

            code = extract_csharp(raw)
            if not code:
                print(" empty extraction")
                continue

            # Save generated code
            code_path = os.path.join(generated_dir, f"{task_name}.cs")
            with open(code_path, "w", encoding="utf-8") as fh:
                fh.write(code)

            # Run the test
            template = task.get("template", "test_project")
            template_cache = os.path.join(cache_dir, template)
            build_ok, all_passed, t_passed, t_total, output = run_dotnet_task(
                generated_code=code,
                test_code=task.get("test_code", ""),
                cached_template_dir=template_cache,
            )

            new_result = {
                "task": task_name,
                "category": task.get("category", ""),
                "weight": task.get("weight", 1),
                "passed": all_passed,
                "harness_error": None if build_ok else output[:200],
                "build_success": build_ok,
                "tests_passed": t_passed,
                "tests_total": t_total,
                "generated_code_path": code_path,
            }

            if all_passed:
                print(f" PASS (was suspect fail)")
                results[idx] = new_result
                fixed += 1
                break
            else:
                print(f" FAIL ({t_passed}/{t_total})")
                results[idx] = new_result

    # Save updated checkpoint
    data["layer3_results"] = results
    passed_after = sum(1 for r in results if r.get("passed"))
    data["layer3_weighted_score"] = sum(
        r.get("weight", 1) for r in results if r.get("passed")
    ) / sum(r.get("weight", 1) for r in results)

    with open(checkpoint, "w", encoding="utf-8") as fh:
        json.dump(data, fh, indent=2)

    print(f"\nResult: {passed_before}/{total} -> {passed_after}/{total} (+{fixed} fixed)")
    print(f"Checkpoint updated: {checkpoint}")


if __name__ == "__main__":
    main()
