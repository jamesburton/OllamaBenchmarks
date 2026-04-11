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
    """Compute score summary from completed tasks.

    Only counts tasks where phase != 'generated' (i.e., fully verified).
    """
    if not completed:
        return {}

    # Only count fully verified tasks (not still in 'generated' phase)
    verified = {k: v for k, v in completed.items() if v.get("phase") != "generated"}
    pending_verify = len(completed) - len(verified)

    total = len(verified)
    passed = sum(1 for r in verified.values() if r.get("passed"))

    by_cat = {}
    for r in verified.values():
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
        "pending_verify": pending_verify,
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
    """Run all tasks for one model + category. Supports per-task checkpoint.

    For C# (dotnet) tasks, uses two-phase approach:
      Phase 1: Generate all responses from model (keeps model loaded/hot)
      Phase 2: Compile and test all generated code (no model needed)
    This avoids model loading/unloading between dotnet builds.
    """
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

    total = len(task_paths)

    if category == "csharp":
        _run_csharp_two_phase(model, slug, task_paths, output_dir, checkpoint, checkpoint_path, total, context_prompt)
    else:
        _run_non_dotnet(model, slug, task_paths, output_dir, checkpoint, checkpoint_path, total, context_prompt)

    # Final summary
    completed = checkpoint.get("completed_tasks", {})
    summary = compute_summary(completed)
    print(f"  Score: {summary.get('passed', 0)}/{summary.get('total', 0)} ({summary.get('score', 0):.1%})")
    return checkpoint


def _run_non_dotnet(model, slug, task_paths, output_dir, checkpoint, checkpoint_path, total, context_prompt):
    """Run non-C# tasks (English, Maths, Logic) — single-phase."""
    completed = checkpoint.get("completed_tasks", {})
    done = len(completed)

    for yaml_path in task_paths:
        question = load_question(yaml_path)
        task_name = question.get("name", yaml_path.stem)
        if task_name in completed:
            continue

        print(f"  [{done+1}/{total}] {task_name}...", end="", flush=True)
        r = run_non_dotnet_task(question, model, context_prompt=context_prompt)
        result = dataclasses.asdict(r)

        completed[task_name] = result
        done += 1
        print(f" {'PASS' if result.get('passed') else 'FAIL'}")

        checkpoint["completed_tasks"] = completed
        checkpoint["model"] = model
        checkpoint["category"] = "non_dotnet"
        checkpoint["benchmark"] = "layer4"
        checkpoint["summary"] = compute_summary(completed)
        save_checkpoint(checkpoint_path, checkpoint)


def _run_csharp_two_phase(model, slug, task_paths, output_dir, checkpoint, checkpoint_path, total, context_prompt):
    """Two-phase C# execution: generate all responses, then compile/test all.

    Phase 1 (GENERATE): Call model for every task, save raw code to disk.
                        Model stays loaded — no dotnet builds interrupt it.
    Phase 2 (VERIFY):   Compile and test each saved code file against test_code.
                        No model calls — just dotnet build+test.

    Checkpoint tracks state: 'generated' (phase 1 done) or full result (phase 2 done).
    """
    completed = checkpoint.get("completed_tasks", {})
    code_dir = output_dir.parent / "generated_code" / slug
    code_dir.mkdir(parents=True, exist_ok=True)
    references_dir = str(REPO_ROOT / "scripts" / "coding_tasks" / "references")

    # Setup dotnet template cache
    cache_dir = str(TEMPLATE_BASE / ".cache")
    for template in ("test_project", "blazor_project"):
        template_dir = str(TEMPLATE_BASE / template)
        if os.path.isdir(template_dir):
            setup_template_cache(template_dir, os.path.join(cache_dir, template))

    # ── Phase 1: GENERATE all responses ──────────────────────────────────
    pending_generation = []
    for yaml_path in task_paths:
        question = load_question(yaml_path)
        task_name = question.get("name", yaml_path.stem)
        # Skip if already fully completed (phase 2 done)
        if task_name in completed and completed[task_name].get("phase") != "generated":
            continue
        # Skip if already generated (phase 1 done, awaiting phase 2)
        if task_name in completed and completed[task_name].get("phase") == "generated":
            continue
        pending_generation.append((yaml_path, question, task_name))

    if pending_generation:
        gen_done = sum(1 for r in completed.values() if r.get("phase") == "generated")
        full_done = sum(1 for r in completed.values() if r.get("phase") != "generated")
        print(f"  Phase 1 (GENERATE): {len(pending_generation)} to generate "
              f"({gen_done} awaiting verify, {full_done} fully done)")

        for yaml_path, question, task_name in pending_generation:
            idx = gen_done + full_done + 1
            print(f"  [gen {idx}/{total}] {task_name}...", end="", flush=True)

            task_def = load_task(str(yaml_path), references_dir)
            t0 = time.monotonic()
            llama_url = os.environ.get("LLAMA_SERVER_URL")
            if llama_url:
                raw = call_llama_server(model, task_def["prompt"],
                                        max_tokens=task_def.get("max_tokens", 4096), timeout=600,
                                        base_url=llama_url)
            else:
                raw = call_ollama(model, task_def["prompt"],
                                  max_tokens=task_def.get("max_tokens", 4096), timeout=600)
            gen_time = time.monotonic() - t0

            code = extract_csharp(raw)
            code_path = code_dir / f"{task_name}.cs"
            if code:
                code_path.write_text(code, encoding="utf-8")

            # Save generation result (phase 1 only — not yet verified)
            completed[task_name] = {
                "task": task_name,
                "category": question.get("category", "csharp"),
                "subcategory": question.get("subcategory", ""),
                "difficulty": question.get("difficulty", "medium"),
                "weight": question.get("weight", 1),
                "validator_type": "dotnet",
                "phase": "generated",
                "has_code": bool(code),
                "code_path": str(code_path),
                "generation_time_s": round(gen_time, 2),
            }
            gen_done += 1
            print(f" {'code' if code else 'EMPTY'} ({gen_time:.1f}s)")

            checkpoint["completed_tasks"] = completed
            checkpoint["model"] = model
            checkpoint["category"] = "csharp"
            checkpoint["benchmark"] = "layer4"
            save_checkpoint(checkpoint_path, checkpoint)

        print(f"  Phase 1 complete: {gen_done} generated")

    # ── Phase 2: VERIFY all generated code ───────────────────────────────
    pending_verify = [
        (k, v) for k, v in completed.items()
        if v.get("phase") == "generated"
    ]

    if pending_verify:
        print(f"  Phase 2 (VERIFY): {len(pending_verify)} to compile+test")
        from coding_tasks.task_runner import run_dotnet_task

        verified = 0
        passed_count = 0
        for task_name, gen_result in pending_verify:
            verified += 1
            print(f"  [verify {verified}/{len(pending_verify)}] {task_name}...", end="", flush=True)

            if not gen_result.get("has_code"):
                gen_result.update({
                    "phase": "complete",
                    "passed": False,
                    "harness_error": "Empty code after extraction",
                    "build_success": False,
                    "tests_passed": 0,
                    "tests_total": 0,
                })
                print(" FAIL (no code)")
            else:
                # Find the YAML to get test_code
                yaml_match = None
                for yaml_path in task_paths:
                    q = load_question(yaml_path)
                    if q.get("name", yaml_path.stem) == task_name:
                        yaml_match = yaml_path
                        break

                if not yaml_match:
                    gen_result.update({"phase": "complete", "passed": False,
                                       "harness_error": "YAML not found for verify"})
                    print(" FAIL (yaml missing)")
                else:
                    task_def = load_task(str(yaml_match), references_dir)
                    template = task_def.get("template", "test_project")
                    cached_template = os.path.join(cache_dir, template)

                    code = Path(gen_result["code_path"]).read_text(encoding="utf-8")
                    build_ok, all_passed, t_passed, t_total, output = run_dotnet_task(
                        generated_code=code,
                        test_code=task_def.get("test_code", ""),
                        cached_template_dir=cached_template,
                    )
                    gen_result.update({
                        "phase": "complete",
                        "passed": all_passed,
                        "harness_error": None if build_ok else output[:200],
                        "build_success": build_ok,
                        "tests_passed": t_passed,
                        "tests_total": t_total,
                    })
                    if all_passed:
                        passed_count += 1
                    print(f" {'PASS' if all_passed else 'FAIL'} ({t_passed}/{t_total})")

            completed[task_name] = gen_result
            checkpoint["completed_tasks"] = completed
            checkpoint["summary"] = compute_summary(completed)
            save_checkpoint(checkpoint_path, checkpoint)

        print(f"  Phase 2 complete: {passed_count}/{verified} passed")


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
