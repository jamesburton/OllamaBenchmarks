#!/usr/bin/env python3
"""Run baseline vs test_aware comparison across all models.

For each model, runs coding L3 twice:
1. Without system prompt (baseline)
2. With test_aware system prompt

Reports the comparison table.

Usage:
    python scripts/run_test_aware_comparison.py
    python scripts/run_test_aware_comparison.py --models qwen3.5:4b qwen3:8b
"""

import argparse
import json
import os
import re
import subprocess
import sys
import time
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
RESULTS_DIR = REPO_ROOT / "results"
CONFIG_DIR = REPO_ROOT / "scripts" / "prompt_configs"

TEST_AWARE_PROMPT = (
    "You are an expert C#/.NET 10 developer writing code that will be compiled "
    "and tested with xUnit v3, AwesomeAssertions, and NSubstitute. "
    "Return ONLY valid C# code in a single file. No markdown fences. "
    "Ensure all public types and methods exactly match the names specified in the prompt. "
    "Use positional record syntax when the prompt specifies records."
)

DEFAULT_MODELS = [
    "qwen3.5:4b",
    "qwen3:8b",
    "granite4:7b-a1b-h",
    "cogito:14b",
    "qwen3:14b",
    "glm-4.7-flash",
    "ministral-3:14b",
    "gemma3:12b",
    "glm-5:cloud",
    "deepseek-v3.2:cloud",
    "minimax-m2.7:cloud",
    "cogito-2.1:671b-cloud",
]


def model_slug(model: str) -> str:
    return re.sub(r"[^\w\.-]", "_", model.replace(":", "_").replace("/", "_"))


def write_config(model: str, system_prompt: str | None):
    """Write or remove prompt config."""
    CONFIG_DIR.mkdir(parents=True, exist_ok=True)
    config_path = CONFIG_DIR / f"{model_slug(model)}.json"
    if system_prompt:
        with open(config_path, "w", encoding="utf-8") as fh:
            json.dump({
                "model": model,
                "system_prompt": system_prompt,
                "mutation": "test_aware",
            }, fh, indent=2)
    elif config_path.exists():
        config_path.unlink()


def run_l3(model: str, timeout: int = 7200) -> dict:
    """Run coding L3, return {passed, total}."""
    slug = model_slug(model)
    checkpoint = RESULTS_DIR / f"coding-{slug}.json"
    if checkpoint.exists():
        checkpoint.unlink()

    cmd = [
        sys.executable, str(REPO_ROOT / "scripts" / "benchmark_coding_layer3.py"),
        "--models", model,
        "--checkpoint-dir", str(RESULTS_DIR),
    ]
    try:
        subprocess.run(cmd, capture_output=True, text=True, timeout=timeout)
    except subprocess.TimeoutExpired:
        pass

    if checkpoint.exists():
        data = json.loads(checkpoint.read_text())
        results = data.get("layer3_results", [])
        passed = sum(1 for r in results if r.get("passed"))
        return {"passed": passed, "total": len(results)}
    return {"passed": 0, "total": 50, "error": "no results"}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--models", nargs="+", default=DEFAULT_MODELS)
    args = parser.parse_args()

    results = []
    print(f"=== Baseline vs test_aware Comparison ===")
    print(f"Models: {len(args.models)}")
    print()

    for i, model in enumerate(args.models, 1):
        print(f"[{i}/{len(args.models)}] {model}")

        # Run 1: baseline (no system prompt)
        write_config(model, None)
        print(f"  [baseline] Running...", end="", flush=True)
        baseline = run_l3(model)
        print(f" {baseline['passed']}/{baseline['total']}")

        # Run 2: test_aware
        write_config(model, TEST_AWARE_PROMPT)
        print(f"  [test_aware] Running...", end="", flush=True)
        test_aware = run_l3(model)
        print(f" {test_aware['passed']}/{test_aware['total']}")

        delta = test_aware["passed"] - baseline["passed"]
        pct = (delta / baseline["passed"] * 100) if baseline["passed"] > 0 else float("inf") if delta > 0 else 0
        print(f"  Delta: {'+' if delta >= 0 else ''}{delta} ({'+' if pct >= 0 else ''}{pct:.0f}%)")

        results.append({
            "model": model,
            "baseline": baseline["passed"],
            "test_aware": test_aware["passed"],
            "total": baseline["total"],
            "delta": delta,
        })

        # Keep test_aware config if it improved, remove if it didn't
        if delta <= 0:
            write_config(model, None)

        print()

    # Summary table
    print("=" * 70)
    print(f"{'Model':<35} {'Baseline':>8} {'TestAware':>9} {'Delta':>7}")
    print("-" * 70)
    for r in sorted(results, key=lambda x: x["delta"], reverse=True):
        delta_str = f"+{r['delta']}" if r['delta'] > 0 else str(r['delta'])
        print(f"{r['model']:<35} {r['baseline']:>5}/50  {r['test_aware']:>5}/50  {delta_str:>7}")
    print("=" * 70)

    # Save results
    output_path = RESULTS_DIR / "test_aware_comparison.json"
    with open(output_path, "w") as fh:
        json.dump({"results": results, "prompt": TEST_AWARE_PROMPT}, fh, indent=2)
    print(f"\nSaved to {output_path}")


if __name__ == "__main__":
    main()
