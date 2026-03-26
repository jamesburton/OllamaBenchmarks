#!/usr/bin/env python3
"""Layer 1: EvalPlus HumanEval+ benchmark wrapper for Ollama models."""

import argparse
import json
import os
import re
import subprocess
import sys
from typing import Any


def model_slug(model: str) -> str:
    model = re.sub(r":latest$", "", model)
    return re.sub(r"[^\w\.-]", "_", model.replace(":", "_").replace("/", "_").replace("\\", "_"))


def write_json(path: str, payload: dict[str, Any]) -> None:
    os.makedirs(os.path.dirname(path) or ".", exist_ok=True)
    with open(path, "w", encoding="utf-8") as handle:
        handle.write(json.dumps(payload, indent=2) + "\n")


def parse_pass_rate(output: str) -> float:
    """Extract pass@1 float from evalplus stdout.

    EvalPlus prints lines such as:
        pass@1: 0.8537
    or:
        HumanEval+ ... pass@1 = 0.854
    """
    # Try "pass@1: <float>" form first
    match = re.search(r"pass@1\s*[=:]\s*([\d.]+)", output, re.IGNORECASE)
    if match:
        try:
            return float(match.group(1))
        except ValueError:
            pass
    return 0.0


def run_evalplus(model: str, dataset: str) -> float:
    """Run evalplus for a single model and return the pass@1 rate."""
    cmd = [
        sys.executable, "-m", "evalplus.evaluate",
        "--model", model,
        "--dataset", dataset,
        "--backend", "openai",
        "--base-url", "http://localhost:11434/v1",
        "--greedy",
    ]
    # Note: evalplus uses signal.SIGALRM which is Linux-only.
    # On Windows, this will fail. Run Layer 1 in WSL or Docker instead.
    try:
        result = subprocess.run(cmd, capture_output=True, text=True, timeout=7200)
    except subprocess.TimeoutExpired:
        print(f"[Layer 1] {model}: timed out after 7200 s", flush=True)
        return 0.0
    except Exception as exc:
        print(f"[Layer 1] {model}: subprocess error — {exc}", flush=True)
        return 0.0

    combined = result.stdout + "\n" + result.stderr
    if result.returncode != 0:
        print(f"[Layer 1] {model}: evalplus exited with code {result.returncode}", flush=True)

    return parse_pass_rate(combined)


def load_checkpoint(path: str) -> dict[str, Any]:
    """Load an existing checkpoint JSON, or return an empty dict."""
    if os.path.isfile(path):
        try:
            with open(path, "r", encoding="utf-8") as fh:
                return json.load(fh)
        except (json.JSONDecodeError, OSError):
            pass
    return {}


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Layer 1: EvalPlus HumanEval+ benchmark wrapper for Ollama models."
    )
    parser.add_argument("--models", nargs="+", required=True, help="Ollama model names to benchmark")
    parser.add_argument("--checkpoint-dir", default="results", help="Directory for per-model checkpoint files")
    parser.add_argument(
        "--evalplus-dataset",
        default="humaneval",
        choices=["humaneval", "mbpp"],
        help="EvalPlus dataset to evaluate against",
    )
    args = parser.parse_args()

    # Verify evalplus is importable before iterating over models
    check = subprocess.run(
        [sys.executable, "-c", "import evalplus"],
        capture_output=True,
    )
    if check.returncode != 0:
        print(
            "ERROR: evalplus is not installed. "
            "Install it with:  pip install evalplus",
            file=sys.stderr,
        )
        sys.exit(1)

    for model in args.models:
        pass_rate = 0.0
        try:
            pass_rate = run_evalplus(model, args.evalplus_dataset)
        except Exception as exc:
            print(f"[Layer 1] {model}: unexpected error — {exc}", flush=True)

        print(f"[Layer 1] {model}: pass@1 = {pass_rate:.4f}", flush=True)

        slug = model_slug(model)
        checkpoint_path = os.path.join(args.checkpoint_dir, f"coding-{slug}.json")

        # Merge into existing checkpoint or create a new one
        data = load_checkpoint(checkpoint_path)
        if data:
            data["layer1_pass_rate"] = pass_rate
        else:
            data = {
                "model": model,
                "benchmark": "coding",
                "layer1_pass_rate": pass_rate,
            }

        write_json(checkpoint_path, data)


if __name__ == "__main__":
    main()
