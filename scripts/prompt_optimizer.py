#!/usr/bin/env python3
"""Prompt optimization loop for OllamaBenchmarks.

Iterates on system prompts and sampling parameters per model to maximize
benchmark scores. Uses a simple hill-climbing approach:
  modify → benchmark → keep/discard → repeat

Optimizes two axes:
  1. System prompt (for coding L3 tasks)
  2. Sampling parameters (temperature, top_p, num_predict)

Usage:
    python scripts/prompt_optimizer.py --model gemma4:26b --iterations 5
    python scripts/prompt_optimizer.py --model glm-5:cloud --iterations 10 --quality-only
    python scripts/prompt_optimizer.py --model cogito:8b --coding-only --tasks 5
"""

import argparse
import copy
import datetime
import json
import os
import random
import subprocess
import sys
import time
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
RESULTS_DIR = REPO_ROOT / "results"
OPTIMIZER_DIR = REPO_ROOT / "scripts" / "prompt_configs"

# ── Baseline prompts ────────────────────────────────────────────────────

BASELINE_SYSTEM_PROMPTS = {
    "coding": (
        "You are an expert C#/.NET developer. When asked to write code, "
        "return ONLY valid C# code in a single file. Do not include markdown "
        "fences, explanations, or commentary — just the raw C# source code."
    ),
    "quality_coding": (
        "Return only Python code. No explanations, no markdown fences."
    ),
}

# Prompt mutations — each is a variant to try
SYSTEM_PROMPT_MUTATIONS = [
    # Coding L3 variants
    {
        "name": "explicit_structure",
        "target": "coding",
        "prompt": (
            "You are a senior C#/.NET 10 developer. Return ONLY valid C# source code. "
            "Rules: (1) Single file, all types at top level or in one namespace. "
            "(2) No markdown fences, no explanations, no comments unless asked. "
            "(3) Use modern C# syntax: records, pattern matching, file-scoped namespaces. "
            "(4) Include all necessary using statements."
        ),
    },
    {
        "name": "concise_expert",
        "target": "coding",
        "prompt": (
            "Expert C# developer. Output raw C# code only. No markdown, no prose. "
            "Single file. Include usings. Use records, modern patterns."
        ),
    },
    {
        "name": "structured_output",
        "target": "coding",
        "prompt": (
            "You write production C#/.NET 10 code. Output rules:\n"
            "- Return ONLY the C# source code\n"
            "- No markdown code fences (no ```)\n"
            "- No explanatory text before or after the code\n"
            "- All types in a single file\n"
            "- Include all required using directives\n"
            "- Use positional records where appropriate\n"
            "- Follow .NET naming conventions (PascalCase types, camelCase locals)"
        ),
    },
    {
        "name": "minimal",
        "target": "coding",
        "prompt": "Return only valid C# code. No markdown. No explanation.",
    },
    {
        "name": "chain_of_thought_then_code",
        "target": "coding",
        "prompt": (
            "You are an expert C#/.NET developer. Think through the problem step by step, "
            "then output ONLY the final C# code. Do not include your reasoning in the output. "
            "No markdown fences. Single file with all types."
        ),
    },
    {
        "name": "test_aware",
        "target": "coding",
        "prompt": (
            "You are an expert C#/.NET 10 developer writing code that will be compiled "
            "and tested with xUnit v3, AwesomeAssertions, and NSubstitute. "
            "Return ONLY valid C# code in a single file. No markdown fences. "
            "Ensure all public types and methods exactly match the names specified in the prompt. "
            "Use positional record syntax when the prompt specifies records."
        ),
    },
]

# Sampling parameter mutations
SAMPLING_MUTATIONS = [
    {"name": "baseline", "temperature": 0, "top_p": 1},
    {"name": "slight_temp", "temperature": 0.1, "top_p": 1},
    {"name": "low_temp", "temperature": 0.3, "top_p": 0.95},
    {"name": "medium_temp", "temperature": 0.6, "top_p": 0.9},
    {"name": "high_temp", "temperature": 0.8, "top_p": 0.85},
    {"name": "nemotron_style", "temperature": 1.0, "top_p": 1.0},
]


def run_quality_benchmark(model: str, timeout: int = 600) -> dict:
    """Run quality benchmark, return results dict."""
    cmd = [
        sys.executable, str(REPO_ROOT / "scripts" / "benchmark_quality.py"),
        "--models", model,
        "--checkpoint-dir", str(RESULTS_DIR),
    ]
    result = subprocess.run(cmd, capture_output=True, text=True, timeout=timeout)
    if result.returncode != 0:
        return {"score": 0, "score_max": 11, "error": result.stderr[:200]}

    # Parse the JSON output
    try:
        data = json.loads(result.stdout)
        if data.get("results"):
            r = data["results"][0]
            return {
                "score": r.get("score", 0),
                "score_max": r.get("score_max", 11),
                "coding_pass": r.get("coding_pass", 0),
                "tool_pass": r.get("tool_pass", 0),
                "agentic_pass": r.get("agentic_pass", 0),
            }
    except (json.JSONDecodeError, IndexError, KeyError):
        pass

    # Fallback: read the checkpoint file
    slug = model.replace(":", "_").replace("/", "_")
    checkpoint = RESULTS_DIR / f"quality-{slug}.json"
    if checkpoint.exists():
        data = json.loads(checkpoint.read_text())
        if data.get("results"):
            r = data["results"][0]
            return {
                "score": r.get("score", 0),
                "score_max": r.get("score_max", 11),
                "coding_pass": r.get("coding_pass", 0),
                "tool_pass": r.get("tool_pass", 0),
                "agentic_pass": r.get("agentic_pass", 0),
            }
    return {"score": 0, "score_max": 11, "error": "no results"}


def run_coding_l3(model: str, max_tasks: int = 0, timeout: int = 3600) -> dict:
    """Run coding L3 benchmark, return results dict."""
    cmd = [
        sys.executable, str(REPO_ROOT / "scripts" / "benchmark_coding_layer3.py"),
        "--models", model,
        "--checkpoint-dir", str(RESULTS_DIR),
    ]
    result = subprocess.run(cmd, capture_output=True, text=True, timeout=timeout)

    # Parse from checkpoint file
    slug = model.replace(":", "_").replace("/", "_").replace("\\", "_")
    import re
    slug = re.sub(r"[^\w\.-]", "_", slug)
    checkpoint = RESULTS_DIR / f"coding-{slug}.json"
    if checkpoint.exists():
        data = json.loads(checkpoint.read_text())
        results = data.get("layer3_results", [])
        passed = sum(1 for r in results if r.get("passed"))
        total = len(results) if results else 50
        return {"passed": passed, "total": total, "weighted": data.get("layer3_weighted_score", 0)}

    return {"passed": 0, "total": 50, "error": "no results"}


def write_prompt_config(model: str, config: dict):
    """Write a prompt config override for a model."""
    OPTIMIZER_DIR.mkdir(parents=True, exist_ok=True)
    slug = model.replace(":", "_").replace("/", "_")
    path = OPTIMIZER_DIR / f"{slug}.json"
    with open(path, "w", encoding="utf-8") as fh:
        json.dump(config, fh, indent=2)
    return path


def load_prompt_config(model: str) -> dict | None:
    """Load existing prompt config if present."""
    slug = model.replace(":", "_").replace("/", "_")
    path = OPTIMIZER_DIR / f"{slug}.json"
    if path.exists():
        return json.loads(path.read_text())
    return None


def main():
    parser = argparse.ArgumentParser(description="Prompt optimization loop")
    parser.add_argument("--model", required=True, help="Model to optimize")
    parser.add_argument("--iterations", type=int, default=5, help="Max iterations")
    parser.add_argument("--quality-only", action="store_true", help="Only run quality benchmark")
    parser.add_argument("--coding-only", action="store_true", help="Only run coding L3")
    parser.add_argument("--tasks", type=int, default=0, help="Limit L3 tasks (0=all)")
    parser.add_argument("--seed", type=int, default=42)
    args = parser.parse_args()

    random.seed(args.seed)
    run_quality = not args.coding_only
    run_coding = not args.quality_only

    print(f"=== Prompt Optimizer ===")
    print(f"Model: {args.model}")
    print(f"Iterations: {args.iterations}")
    print(f"Benchmarks: {'quality' if run_quality else ''} {'coding-L3' if run_coding else ''}")
    print()

    # Step 1: Baseline score
    print("[baseline] Running baseline benchmarks...")
    baseline = {"prompt": "baseline", "sampling": "baseline"}
    if run_quality:
        baseline["quality"] = run_quality_benchmark(args.model)
        print(f"  Quality: {baseline['quality'].get('score', '?')}/{baseline['quality'].get('score_max', '?')}")
    if run_coding:
        baseline["coding"] = run_coding_l3(args.model, args.tasks)
        print(f"  Coding L3: {baseline['coding'].get('passed', '?')}/{baseline['coding'].get('total', '?')}")

    def total_score(result: dict) -> float:
        s = 0
        if "quality" in result:
            s += result["quality"].get("score", 0)
        if "coding" in result:
            s += result["coding"].get("passed", 0)
        return s

    best = copy.deepcopy(baseline)
    best_score = total_score(best)
    print(f"\n[baseline] Total score: {best_score}")

    history = [{"iteration": 0, "mutation": "baseline", "score": best_score, "kept": True}]

    # Step 2: Iterate
    mutations = SYSTEM_PROMPT_MUTATIONS + SAMPLING_MUTATIONS
    random.shuffle(mutations)

    for i in range(1, args.iterations + 1):
        if i > len(mutations):
            # Combine best prompt + random sampling variation
            mutation = random.choice(SAMPLING_MUTATIONS)
        else:
            mutation = mutations[i - 1]

        name = mutation.get("name", f"mutation_{i}")
        print(f"\n[iter {i}/{args.iterations}] Trying: {name}")

        # Apply mutation
        if "prompt" in mutation:
            # System prompt mutation — need to temporarily override
            # For now, write config that benchmark scripts can read
            config = {
                "model": args.model,
                "system_prompt": mutation["prompt"],
                "mutation": name,
                "timestamp": datetime.datetime.now(datetime.timezone.utc).isoformat(),
            }
            config_path = write_prompt_config(args.model, config)
            print(f"  Config: {config_path}")

        if "temperature" in mutation:
            config = load_prompt_config(args.model) or {}
            config["sampling"] = {
                "temperature": mutation["temperature"],
                "top_p": mutation["top_p"],
            }
            config["mutation"] = name
            write_prompt_config(args.model, config)

        # Run benchmarks
        result = {"prompt": name}
        if run_quality:
            result["quality"] = run_quality_benchmark(args.model)
            print(f"  Quality: {result['quality'].get('score', '?')}/{result['quality'].get('score_max', '?')}")
        if run_coding:
            result["coding"] = run_coding_l3(args.model, args.tasks)
            print(f"  Coding L3: {result['coding'].get('passed', '?')}/{result['coding'].get('total', '?')}")

        score = total_score(result)
        improved = score > best_score
        kept = improved or (score == best_score and "prompt" in mutation and len(mutation.get("prompt", "")) < len(best.get("prompt", "")))

        if kept:
            print(f"  Score: {score} (was {best_score}) → KEEP")
            best = copy.deepcopy(result)
            best_score = score
        else:
            print(f"  Score: {score} (was {best_score}) → DISCARD")
            # Restore previous config
            if best.get("_config"):
                write_prompt_config(args.model, best["_config"])

        history.append({"iteration": i, "mutation": name, "score": score, "kept": kept})

    # Step 3: Report
    print(f"\n{'='*60}")
    print(f"=== Optimization Complete ===")
    print(f"Model: {args.model}")
    print(f"Baseline score: {total_score(baseline)}")
    print(f"Best score: {best_score}")
    improvement = best_score - total_score(baseline)
    print(f"Improvement: +{improvement}" if improvement > 0 else f"No improvement ({improvement})")
    print(f"\nBest config: {best.get('prompt', 'baseline')}")
    if "quality" in best:
        q = best["quality"]
        print(f"  Quality: {q.get('score', '?')}/{q.get('score_max', '?')} "
              f"(coding {q.get('coding_pass', '?')}, tools {q.get('tool_pass', '?')}, "
              f"agentic {q.get('agentic_pass', '?')})")
    if "coding" in best:
        c = best["coding"]
        print(f"  Coding L3: {c.get('passed', '?')}/{c.get('total', '?')} "
              f"(weighted {c.get('weighted', '?')})")

    # Save history
    history_path = OPTIMIZER_DIR / f"{args.model.replace(':', '_').replace('/', '_')}_history.json"
    OPTIMIZER_DIR.mkdir(parents=True, exist_ok=True)
    with open(history_path, "w") as fh:
        json.dump({
            "model": args.model,
            "iterations": args.iterations,
            "baseline_score": total_score(baseline),
            "best_score": best_score,
            "history": history,
        }, fh, indent=2)
    print(f"\nHistory saved to {history_path}")

    print("\nIteration log:")
    for h in history:
        mark = "✓" if h["kept"] else "✗"
        print(f"  [{mark}] {h['iteration']:2d}. {h['mutation']:30s} → {h['score']}")


if __name__ == "__main__":
    main()
