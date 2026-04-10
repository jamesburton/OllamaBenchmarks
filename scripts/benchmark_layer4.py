#!/usr/bin/env python3
"""L4 Extended Benchmark — 2250 questions across C#, English, Maths, Logic.

Usage:
    python scripts/benchmark_layer4.py --models qwen3-coder-next "glm-5:cloud" --category all
    python scripts/benchmark_layer4.py --models qwen3-coder-next --category maths
    python scripts/benchmark_layer4.py --models "glm-5:cloud" --context-run --baseline-dir results/layer4/baseline
"""

import argparse
import dataclasses
import datetime
import json
import os
import re
import sys
import time
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(REPO_ROOT / "scripts"))

import yaml
from coding_tasks.task_runner import (
    call_ollama, call_llama_server, model_slug, load_task,
    run_task, setup_template_cache, extract_csharp,
)
from coding_tasks.code_extractor import extract_csharp
from benchmark_layer4.question_runner import run_non_dotnet_task, L4TaskResult
from benchmark_layer4.validators import validate

CATEGORIES = ["csharp", "english", "maths", "logic"]
TASK_DIR = REPO_ROOT / "scripts" / "layer4_tasks"
TEMPLATE_BASE = REPO_ROOT / "scripts" / "coding_tasks" / "templates"


def discover_tasks(task_dir: Path, category: str) -> list[Path]:
    """Recursively find all YAML task files for a category."""
    cat_dir = task_dir / category
    if not cat_dir.is_dir():
        return []
    tasks = sorted(
        p for p in cat_dir.rglob("*.yaml")
        if not p.stem.startswith("_")
    )
    return tasks


def load_question(yaml_path: Path) -> dict:
    """Load a question YAML file."""
    with open(yaml_path, "r", encoding="utf-8") as f:
        return yaml.safe_load(f)


def load_checkpoint(checkpoint_path: Path) -> dict:
    """Load existing checkpoint or create empty one."""
    if checkpoint_path.exists():
        with open(checkpoint_path, "r", encoding="utf-8") as f:
            return json.load(f)
    return {"completed_tasks": {}}


def save_checkpoint(checkpoint_path: Path, data: dict):
    """Atomically save checkpoint (write to .tmp then rename)."""
    tmp = checkpoint_path.with_suffix(".tmp")
    with open(tmp, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2, default=str)
    tmp.replace(checkpoint_path)


def compute_summary(completed: dict) -> dict:
    """Compute score summary from completed tasks."""
    if not completed:
        return {}

    total = len(completed)
    passed = sum(1 for r in completed.values() if r.get("passed"))

    by_cat = {}
    for r in completed.values():
        cat = r.get("category", "unknown")
        if cat not in by_cat:
            by_cat[cat] = {"total": 0, "passed": 0}
        by_cat[cat]["total"] += 1
        if r.get("passed"):
            by_cat[cat]["passed"] += 1

    for cat_data in by_cat.values():
        cat_data["score"] = round(cat_data["passed"] / cat_data["total"], 4) if cat_data["total"] > 0 else 0

    return {
        "total": total,
        "passed": passed,
        "score": round(passed / total, 4) if total > 0 else 0,
        "by_category": by_cat,
    }


def run_category(
    model: str,
    category: str,
    task_dir: Path,
    output_dir: Path,
    context_prompt: str | None = None,
    baseline_checkpoint: dict | None = None,
):
    """Run all tasks for one model + category. Supports per-task checkpoint."""
    slug = model_slug(model)
    suffix = "_ctx" if context_prompt else ""
    checkpoint_path = output_dir / f"{category}-{slug}{suffix}.json"

    # Load existing checkpoint for resumption
    checkpoint = load_checkpoint(checkpoint_path)
    completed = checkpoint.get("completed_tasks", {})

    # Discover tasks
    task_paths = discover_tasks(task_dir, category)
    if not task_paths:
        print(f"  No tasks found for {category}")
        return

    # If context-run, only run tasks that failed in baseline
    if context_prompt and baseline_checkpoint:
        baseline_completed = baseline_checkpoint.get("completed_tasks", {})
        failed_names = {k for k, v in baseline_completed.items() if not v.get("passed")}
        task_paths = [p for p in task_paths if p.stem in failed_names or load_question(p).get("name", p.stem) in failed_names]
        print(f"  Context re-run: {len(task_paths)} failed tasks (of {len(baseline_completed)} total)")

    # Setup dotnet template cache if C# tasks
    if category == "csharp":
        cache_dir = str(TEMPLATE_BASE / ".cache")
        for template in ("test_project", "blazor_project"):
            template_dir = str(TEMPLATE_BASE / template)
            if os.path.isdir(template_dir):
                setup_template_cache(template_dir, os.path.join(cache_dir, template))

    total = len(task_paths)
    done = len(completed)
    passed_count = sum(1 for r in completed.values() if r.get("passed"))

    for i, yaml_path in enumerate(task_paths, 1):
        question = load_question(yaml_path)
        task_name = question.get("name", yaml_path.stem)

        # Skip if already completed
        if task_name in completed:
            continue

        print(f"  [{done+1}/{total}] {task_name}...", end="", flush=True)

        if question.get("validator_type") == "dotnet":
            # C# task — use existing L3 harness
            references_dir = str(REPO_ROOT / "scripts" / "coding_tasks" / "references")
            task_def = load_task(str(yaml_path), references_dir)
            template = task_def.get("template", "test_project")
            cached_template = os.path.join(cache_dir, template)

            t0 = time.monotonic()
            llama_url = os.environ.get("LLAMA_SERVER_URL")
            if llama_url:
                from coding_tasks.task_runner import call_llama_server as _call_ls
                raw = _call_ls(model, task_def["prompt"], max_tokens=task_def.get("max_tokens", 4096), timeout=600, base_url=llama_url)
            else:
                raw = call_ollama(model, task_def["prompt"], max_tokens=task_def.get("max_tokens", 4096), timeout=600)
            gen_time = time.monotonic() - t0

            code = extract_csharp(raw)
            if not code:
                result = {"task": task_name, "category": question.get("category", "csharp"),
                          "subcategory": question.get("subcategory", ""), "difficulty": question.get("difficulty", "medium"),
                          "weight": question.get("weight", 1), "passed": False, "validator_type": "dotnet",
                          "harness_error": "Empty code after extraction", "model_response": "",
                          "generation_time_s": round(gen_time, 2)}
            else:
                from coding_tasks.task_runner import run_dotnet_task
                build_ok, all_passed, t_passed, t_total, output = run_dotnet_task(
                    generated_code=code, test_code=task_def.get("test_code", ""),
                    cached_template_dir=cached_template,
                )
                # Save generated code
                code_dir = output_dir.parent / "generated_code" / slug
                code_dir.mkdir(parents=True, exist_ok=True)
                (code_dir / f"{task_name}.cs").write_text(code, encoding="utf-8")

                result = {"task": task_name, "category": question.get("category", "csharp"),
                          "subcategory": question.get("subcategory", ""), "difficulty": question.get("difficulty", "medium"),
                          "weight": question.get("weight", 1), "passed": all_passed, "validator_type": "dotnet",
                          "harness_error": None if build_ok else output[:200],
                          "build_success": build_ok, "tests_passed": t_passed, "tests_total": t_total,
                          "generation_time_s": round(gen_time, 2)}
        else:
            # Non-dotnet task
            r = run_non_dotnet_task(question, model, context_prompt=context_prompt)
            result = dataclasses.asdict(r)

        completed[task_name] = result
        done += 1
        if result.get("passed"):
            passed_count += 1
        status = "PASS" if result.get("passed") else "FAIL"
        print(f" {status}")

        # Save checkpoint after each task
        checkpoint["completed_tasks"] = completed
        checkpoint["model"] = model
        checkpoint["category"] = category
        checkpoint["benchmark"] = "layer4"
        checkpoint["summary"] = compute_summary(completed)
        save_checkpoint(checkpoint_path, checkpoint)

    # Final summary
    summary = compute_summary(completed)
    print(f"  Score: {summary.get('passed', 0)}/{summary.get('total', 0)} ({summary.get('score', 0):.1%})")
    return checkpoint


def main():
    parser = argparse.ArgumentParser(description="L4 Extended Benchmark")
    parser.add_argument("--models", nargs="+", required=True)
    parser.add_argument("--category", default="all", choices=CATEGORIES + ["all"])
    parser.add_argument("--task-dir", type=Path, default=TASK_DIR)
    parser.add_argument("--output-dir", type=Path, default=REPO_ROOT / "results" / "layer4" / "baseline")
    parser.add_argument("--template-base", type=Path, default=TEMPLATE_BASE)
    parser.add_argument("--context-run", action="store_true", help="Re-run failed tasks with context injection")
    parser.add_argument("--context-dir", type=Path, default=REPO_ROOT / "results" / "layer4" / "context")
    parser.add_argument("--baseline-dir", type=Path, default=REPO_ROOT / "results" / "layer4" / "baseline")
    args = parser.parse_args()

    categories = CATEGORIES if args.category == "all" else [args.category]
    args.output_dir.mkdir(parents=True, exist_ok=True)

    print(f"=== L4 Extended Benchmark ===")
    print(f"Models: {args.models}")
    print(f"Categories: {categories}")
    print(f"{'Context re-run' if args.context_run else 'Baseline'}")
    print()

    for model in args.models:
        slug = model_slug(model)
        print(f"[model] {model}")

        for category in categories:
            print(f"  [{category}]")

            context_prompt = None
            baseline_checkpoint = None

            if args.context_run:
                # Load context prompt
                ctx_file = args.context_dir / f"{category}_{slug}_context.json"
                if ctx_file.exists():
                    ctx_data = json.loads(ctx_file.read_text())
                    context_prompt = ctx_data.get("context_prompt")
                    if not context_prompt:
                        print(f"    No context prompt found, skipping")
                        continue

                # Load baseline checkpoint
                baseline_file = args.baseline_dir / f"{category}-{slug}.json"
                if baseline_file.exists():
                    baseline_checkpoint = json.loads(baseline_file.read_text())

                output_dir = REPO_ROOT / "results" / "layer4" / "context_run"
            else:
                output_dir = args.output_dir

            output_dir.mkdir(parents=True, exist_ok=True)
            run_category(model, category, args.task_dir, output_dir,
                         context_prompt=context_prompt, baseline_checkpoint=baseline_checkpoint)

        print()

    print("=== Done ===")


if __name__ == "__main__":
    main()
