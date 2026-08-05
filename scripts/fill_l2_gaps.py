#!/usr/bin/env python3
"""Fill Layer 2 (158-task HumanEval-CS) gaps for installed Strix Ollama models.

Ordered roughly small-to-large so early results land quickly. Skips models
that already have layer2_results >= 158 in their composite checkpoint.
Skips models known to fail (sentinel via SKIP set).
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

# Ordered small-to-large for fastest early results.
# Skips: duplicate blobs (qwen3.5:latest == qwen3.5:9b), known-broken loads.
MODELS = [
    # Tier A — small/fast (<10 GB)
    "lfm2.5-thinking:1.2b",
    "sam860/lfm2:2.6b",
    "LFM2-2.6b-tools",
    "nemotron-3-nano:4b",
    "gemma3:4b",
    "hf.co/mradermacher/shenwen-coderV2-Instruct-GGUF:q8_0",
    "granite4:7b-a1b-h",
    "gemma4:e2b",
    "gemma4",  # latest = e4b
    "hf.co/bartowski/Tesslate_OmniCoder-9B-GGUF:Q4_K_M",
    "omnicoder:9b-q4_k_m",
    "qwen3.5",  # latest = 9b
    "ministral-3:14b",
    "zac/phi4-tools",
    "qwen3:14b",
    "phi4-mini",
    "gpt-oss:20b",
    "mistral-small",
    "glm-4.7-flash-reap-toolfix",
    "nemotron-nano-9b-v2-toolfix",
    "trinity-mini:Q4_K_M",
    "gemma3:27b",
    "gemma4:26b",
    "granite4:32b-a9b-h",
    "glm-4.7-flash",
    "gemma4:31b",
    "RogerBen/qwen3.5-35b-opus-distill",
    # Tier B — medium-large (24-35 GB)
    "nemotron-3-nano",
    "nemotron-3-nano:30b-a3b-q8_0",
    "nemotron-cascade-2",
    # Tier C — heavy (60+ GB, will be slow)
    "gpt-oss:120b",
    "llama4:16x17b",
    "nemotron-3-super",
]

# Models known to fail or duplicate — skip without running
SKIP = {
    "hf.co/Jackrong/Qwopus3.5-9B-v3-GGUF:q8_0",  # fails to load (qwen3.5 arch blob error)
    "qwen3:0.6b",  # too small
    "gemma4:e4b",  # same blob as gemma4:latest
    "qwen3.5:9b",  # same blob as qwen3.5:latest
    "hf.co/Tesslate/OmniCoder-9B-GGUF:Q4_K_M",  # probable dup of bartowski variant
    "hf.co/bartowski/nvidia_NVIDIA-Nemotron-Nano-9B-v2-GGUF:Q4_K_M",  # dup of nemotron-nano-9b-v2-toolfix
    "lfm2.5-thinking",  # same blob as :1.2b
}


def slug(model: str) -> str:
    if model.endswith(":latest"):
        model = model[: -len(":latest")]
    s = re.sub(r"[:/\\]", "_", model)
    return re.sub(r"[^\w.\-]", "_", s)


def main():
    completed = []
    skipped = []
    failed = []

    for i, model in enumerate(MODELS, 1):
        if model in SKIP:
            print(f"[{i}/{len(MODELS)}] {model} -> SKIP (in skip-list)", flush=True)
            skipped.append((model, "skip-list"))
            continue

        s = slug(model)
        cp = RESULTS / f"coding-{s}.json"
        if cp.exists():
            try:
                d = json.loads(cp.read_text(encoding="utf-8"))
                l2 = d.get("layer2_results", [])
                if len(l2) >= 158:
                    print(
                        f"[{i}/{len(MODELS)}] {model} -> SKIP (L2 already {len(l2)}/158)",
                        flush=True,
                    )
                    skipped.append((model, f"L2 {len(l2)}/158"))
                    continue
            except Exception:
                pass

        print(f"[{i}/{len(MODELS)}] {model} -> running L2 ...", flush=True)
        t0 = datetime.datetime.now(datetime.timezone.utc)

        cmd = [
            sys.executable,
            "scripts/benchmark_coding_layer2.py",
            "--models", model,
            "--dataset-path",
            "scripts/coding_tasks/datasets/data/humaneval-cs-reworded.json",
        ]

        try:
            result = subprocess.run(
                cmd,
                cwd=ROOT,
                capture_output=True,
                text=True,
                timeout=6 * 60 * 60,  # 6 hours hard cap per model
            )
            elapsed = datetime.datetime.now(datetime.timezone.utc) - t0
            # Tally from output file
            score = "?"
            pf = "?"
            if cp.exists():
                try:
                    d = json.loads(cp.read_text(encoding="utf-8"))
                    if "layer2_pass_rate" in d:
                        score = f"{d['layer2_pass_rate']:.4f}"
                        pf = f"{d.get('layer2_passed','?')}/{d.get('layer2_total','?')}"
                except Exception:
                    pass
            print(
                f"    -> {pf} (L2={score}) in {elapsed}",
                flush=True,
            )
            if result.returncode != 0:
                tail_err = (result.stderr or "")[-300:]
                print(f"    [warn] exit={result.returncode}: {tail_err}", flush=True)
            completed.append((model, pf, score))
        except subprocess.TimeoutExpired:
            print(f"    -> TIMEOUT after 6h", flush=True)
            failed.append((model, "timeout"))
        except Exception as exc:
            print(f"    -> ERROR: {exc}", flush=True)
            failed.append((model, str(exc)))

    print()
    print("=" * 70)
    print("=== SUMMARY (L2 fill) ===")
    for m, pf, s in completed:
        print(f"  {m:<55} {pf:>10}  L2={s}")
    if skipped:
        print(f"\nSkipped: {len(skipped)}")
        for m, r in skipped:
            print(f"  {m}: {r}")
    if failed:
        print(f"\nFailed:")
        for m, r in failed:
            print(f"  {m}: {r}")


if __name__ == "__main__":
    main()
