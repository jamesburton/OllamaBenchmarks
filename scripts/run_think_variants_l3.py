#!/usr/bin/env python3
"""Run think:true L3 variants for thinking-capable installed Strix models.

For each model in MODELS, runs benchmark_coding_layer3.py with CODING_BENCH_THINK=true
and writes coding-{slug}-think.json. Skips if the -think file already exists.

Sequential — one model at a time (single iGPU on Strix).
"""
import datetime
import json
import os
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(r"C:\Development\OllamaBenchmarks")
RESULTS = ROOT / "results"

# Priority list: thinking-capable models that have a no-think L3 baseline but no -think variant.
# Roughly ordered fast-first within each tier so we get early results.
MODELS = [
    # Small/fast first (fast iteration)
    "gemma4:e2b",
    "granite4:7b-a1b-h",
    "granite4:32b-a9b-h",
    "qwen3.6",
    "qwen3.6:27b",
    "qwen3-coder-next",
    "qwen3:14b",
    "glm-4.7-flash",
    "nemotron-3-nano:30b-a3b-q8_0",
    # Heavy at the end (large MoE / slow)
    "gpt-oss:20b",
    "nemotron-3-super",
]


def slug(model: str) -> str:
    if model.endswith(":latest"):
        model = model[: -len(":latest")]
    s = re.sub(r"[:/\\]", "_", model)
    return re.sub(r"[^\w.\-]", "_", s)


def main():
    env = os.environ.copy()
    env["CODING_BENCH_THINK"] = "true"

    completed = []
    skipped = []
    failed = []

    for i, model in enumerate(MODELS, 1):
        s = slug(model)
        out_file = RESULTS / f"coding-{s}-think.json"
        if out_file.exists():
            print(f"[{i}/{len(MODELS)}] {model} -> SKIP (exists: {out_file.name})", flush=True)
            skipped.append(model)
            continue

        print(f"[{i}/{len(MODELS)}] {model} -> running think:true L3 ...", flush=True)
        t0 = datetime.datetime.now(datetime.timezone.utc)

        cmd = [
            sys.executable,
            "scripts/benchmark_coding_layer3.py",
            "--models", model,
            "--output", str(RESULTS / f"coding-layer3-{s}-think.json"),
        ]

        try:
            result = subprocess.run(
                cmd,
                cwd=ROOT,
                env=env,
                capture_output=True,
                text=True,
                timeout=4 * 60 * 60,  # 4 hours hard cap per model
            )
            elapsed = datetime.datetime.now(datetime.timezone.utc) - t0
            stdout = result.stdout or ""
            passes = stdout.count("PASS")
            fails = stdout.count("FAIL")
            score = "?"
            if out_file.exists():
                try:
                    d = json.loads(out_file.read_text(encoding="utf-8"))
                    score = f"{d.get('layer3_weighted_score', '?'):.4f}"
                except Exception:
                    pass
            print(
                f"    -> {passes} PASS / {passes+fails} total (L3={score}) in {elapsed}",
                flush=True,
            )
            if result.returncode != 0:
                tail_err = (result.stderr or "")[-300:]
                print(f"    [warn] exit={result.returncode}: {tail_err}", flush=True)
            completed.append((model, passes, passes + fails, score))
        except subprocess.TimeoutExpired:
            print(f"    -> TIMEOUT after 4h", flush=True)
            failed.append((model, "timeout"))
        except Exception as exc:
            print(f"    -> ERROR: {exc}", flush=True)
            failed.append((model, str(exc)))

    print()
    print("=" * 70)
    print("=== SUMMARY (think:true L3 variants) ===")
    for m, p, t, s in completed:
        print(f"  {m:<50} {p:>3}/{t:<3}  L3={s}")
    if skipped:
        print(f"\nSkipped (already had -think): {len(skipped)}")
    if failed:
        print(f"\nFailed:")
        for m, r in failed:
            print(f"  {m}: {r}")


if __name__ == "__main__":
    main()
