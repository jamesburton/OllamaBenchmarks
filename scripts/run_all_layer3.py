#!/usr/bin/env python3
"""Run Layer 3 coding benchmark for all installed Ollama models sequentially.

Checkpoints each model individually so progress is preserved if interrupted.
Skips models that already have a Layer 3 checkpoint from this run.
"""
import json
import os
import subprocess
import sys
import datetime

MODELS = [
    # Tier 1: Top quality scorers (5/5 quality), most likely to do well
    "qwen3.5",              # 9b, champion tier
    "qwen3.5:4b",           # best value
    "cogito:14b",           # best PPL
    "qwen3:8b",             # strong quality
    "qwen3:14b",            # larger qwen3
    # Tier 2: Coding-focused models
    "qwen3-coder-next",     # 51GB frontier (already done but re-run for clean data)
    "qwen3-coder:30b",
    # Tier 3: Other strong models
    "RogerBen/qwen3.5-35b-opus-distill",
    "phi4-mini",
    "ministral-3:14b",
    "granite4:7b-a1b-h",
    "granite4:32b-a9b-h",
    "devstral-small-2:24b-instruct-2512-q8_0",
    "glm-4.7-flash",
    "nemotron-3-nano",
    # Tier 4: Larger / slower models
    "lfm2:24b",
    "qwen3.5:35b-a3b",
    "qwen3.5:122b-a10b",
    "nemotron-3-nano:30b-a3b-q8_0",
    "llama4:16x17b",
    "gpt-oss:120b",
    "nemotron-3-super",
    # Tier 5: Specialty / experimental
    "zac/phi4-tools",
    "mistral-small",
    "ingu627/exaone4.0:32b",
    "gpt-oss:20b",
    "lfm2.5-thinking:1.2b",
    "omnicoder:9b-q4_k_m",
    "nemotron-cascade-2",
    "glm-4.7-flash:bf16",
    "trinity-mini:Q4_K_M",
]


def model_slug(model: str) -> str:
    import re
    model = re.sub(r":latest$", "", model)
    return re.sub(r"[^\w\.-]", "_", model.replace(":", "_").replace("/", "_").replace("\\", "_"))


def main():
    results_dir = "results"
    run_start = datetime.datetime.now(datetime.timezone.utc)
    completed = []
    failed = []

    print(f"=== Layer 3 full benchmark run: {len(MODELS)} models ===")
    print(f"Started at {run_start.isoformat()}")
    print()

    for i, model in enumerate(MODELS, 1):
        slug = model_slug(model)
        checkpoint = os.path.join(results_dir, f"coding-{slug}.json")

        # Check if already has fresh Layer 3 results from this session
        # (We always re-run to get clean data with the fixed tasks)

        print(f"[{i}/{len(MODELS)}] {model} ...", flush=True)
        cmd = [
            sys.executable,
            "scripts/benchmark_coding.py",
            "--models", model,
            "--layers", "3",
            "--checkpoint-dir", results_dir,
            "--output", checkpoint,
        ]
        try:
            result = subprocess.run(cmd, capture_output=True, text=True, timeout=7200)
            # Parse the score from the layer3 checkpoint
            if os.path.isfile(checkpoint):
                data = json.load(open(checkpoint))
                # Find the model's L3 score
                l3 = "?"
                if "layer3_weighted_score" in data:
                    l3 = f"{data['layer3_weighted_score']:.4f}"
                elif "results" in data:
                    for r in data.get("results", []):
                        if isinstance(r, dict) and r.get("model") == model:
                            l3 = f"{r.get('layer3_weighted_score', '?')}"
                            break
                # Count pass/fail from stdout
                stdout = result.stdout or ""
                passes = stdout.count("PASS")
                fails = stdout.count("FAIL")
                print(f"  -> {passes}/{passes+fails} passed (L3={l3})", flush=True)
                completed.append((model, passes, passes+fails, l3))
            else:
                print(f"  -> No checkpoint written", flush=True)
                failed.append((model, "no checkpoint"))

            if result.returncode != 0:
                stderr_preview = (result.stderr or "")[:200]
                if stderr_preview:
                    print(f"  [warn] exit code {result.returncode}: {stderr_preview}", flush=True)

        except subprocess.TimeoutExpired:
            print(f"  -> TIMEOUT (2h)", flush=True)
            failed.append((model, "timeout"))
        except Exception as exc:
            print(f"  -> ERROR: {exc}", flush=True)
            failed.append((model, str(exc)))

        print(flush=True)

    # Summary
    print("=" * 60)
    print(f"=== RESULTS ({len(completed)} completed, {len(failed)} failed) ===")
    print("=" * 60)
    print(f"{'Model':<45} {'Pass':>5} {'Total':>5} {'L3 Score':>10}")
    print("-" * 70)
    for model, passes, total, l3 in sorted(completed, key=lambda x: -x[1]):
        print(f"{model:<45} {passes:>5} {total:>5} {l3:>10}")

    if failed:
        print(f"\nFailed models:")
        for model, reason in failed:
            print(f"  {model}: {reason}")

    run_end = datetime.datetime.now(datetime.timezone.utc)
    print(f"\nTotal time: {run_end - run_start}")


if __name__ == "__main__":
    main()
