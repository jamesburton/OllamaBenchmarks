#!/usr/bin/env python3
"""Follow-up after the chat-mode L2 chain finishes:

1. Throughput-resource on Strix for gemma4 family (currently missing).
2. Layer 3 (.NET) for models that scored 30%+ on L2 but lack L3 data.

Sequential (one model at a time — single iGPU).
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
OLLAMA = r"C:\Users\james\AppData\Local\Programs\Ollama\ollama.exe"


def slug(model: str) -> str:
    if model.endswith(":latest"):
        model = model[: -len(":latest")]
    s = re.sub(r"[:/\\]", "_", model)
    return re.sub(r"[^\w.\-]", "_", s)


# 1. Throughput-resource — missing Strix tps for gemma4 family
THROUGHPUT_MODELS = [
    "gemma4",        # latest = e4b
    "gemma4:e2b",
    "gemma4:26b",
    "gemma4:31b",
    "mistral-small",        # also missing Strix tps
    "LFM2-2.6b-tools",      # also missing
    "nemotron-3-nano",      # latest 24GB missing
]

# 2. Layer 3 — high-L2 models that lack L3 data
L3_MODELS = [
    "RogerBen/qwen3.5-35b-opus-distill",   # L2 raw 53.8%
    "hf.co/bartowski/Tesslate_OmniCoder-9B-GGUF:Q4_K_M",  # L2 raw 44.9%
    "nemotron-nano-9b-v2-toolfix",         # L2 raw 43.0%
    "zac/phi4-tools",                       # L2 raw 39.9%
    "mistral-small",                        # L2 raw 38.0%
    "nemotron-cascade-2",                   # L2 raw 36.7%
    "nemotron-3-nano:30b-a3b-q8_0",         # L2 raw 33.5%
    "trinity-mini:Q4_K_M",                  # L2 raw 31.0%
    "nemotron-3-nano",                      # L2 raw 31.0%
    "nemotron-3-nano:4b",                   # L2 raw 22.2%
    "LFM2-2.6b-tools",                      # L2 raw 6.3%, may improve with chat
    "phi4-mini",                            # L2 raw 10.8%
    "sam860/LFM2:2.6b",                     # L2 raw 6.3%
    "lfm2.5-thinking:1.2b",                 # L2 raw 4.4%
]


def has_strix_throughput(model: str) -> bool:
    p = RESULTS / f"throughput-resource-{slug(model)}.json"
    if not p.exists():
        return False
    try:
        d = json.loads(p.read_text(encoding="utf-8"))
        host = d.get("host") or (d.get("host_details") or {}).get("hostname", "")
        return (host or "").upper() == "STRIX"
    except Exception:
        return False


def has_l3(model: str) -> bool:
    p = RESULTS / f"coding-{slug(model)}.json"
    if not p.exists():
        return False
    try:
        d = json.loads(p.read_text(encoding="utf-8"))
        return len(d.get("layer3_results", [])) >= 50
    except Exception:
        return False


def run_throughput(model: str) -> bool:
    s = slug(model)
    out = RESULTS / f"throughput-resource-{s}.json"
    print(f"  throughput: {model} -> {out.name}", flush=True)
    t0 = datetime.datetime.now(datetime.timezone.utc)
    res = subprocess.run(
        [
            "powershell", "-ExecutionPolicy", "Bypass", "-File",
            "scripts/benchmark_throughput_resource.ps1",
            "-Models", model,
            "-OutputPath", str(out),
        ],
        cwd=ROOT,
        capture_output=True,
        text=True,
        timeout=20 * 60,
    )
    elapsed = datetime.datetime.now(datetime.timezone.utc) - t0
    print(f"    exit={res.returncode} in {elapsed}", flush=True)
    return res.returncode == 0


def run_l3(model: str) -> bool:
    s = slug(model)
    print(f"  L3: {model}", flush=True)
    t0 = datetime.datetime.now(datetime.timezone.utc)
    res = subprocess.run(
        [
            sys.executable,
            "scripts/benchmark_coding_layer3.py",
            "--models", model,
            "--output", str(RESULTS / f"coding-layer3-{s}.json"),
        ],
        cwd=ROOT,
        capture_output=True,
        text=True,
        timeout=6 * 60 * 60,
    )
    elapsed = datetime.datetime.now(datetime.timezone.utc) - t0
    # Read score
    cp = RESULTS / f"coding-{s}.json"
    score = "?"
    if cp.exists():
        try:
            d = json.loads(cp.read_text(encoding="utf-8"))
            s_score = d.get("layer3_weighted_score")
            n_pass = sum(1 for r in d.get("layer3_results", []) if r.get("passed"))
            n_tot = len(d.get("layer3_results", []))
            score = f"{n_pass}/{n_tot} ({s_score:.4f})" if s_score is not None else f"{n_pass}/{n_tot}"
        except Exception:
            pass
    print(f"    {score} in {elapsed}", flush=True)
    if res.returncode != 0:
        print(f"    [warn] exit={res.returncode}: {(res.stderr or '')[-200:]}", flush=True)
    return res.returncode == 0


def main():
    print(f"=== Phase 1: throughput-resource fills ({len(THROUGHPUT_MODELS)} candidates) ===")
    for i, m in enumerate(THROUGHPUT_MODELS, 1):
        if has_strix_throughput(m):
            print(f"[{i}/{len(THROUGHPUT_MODELS)}] {m} -> SKIP (Strix tps already present)")
            continue
        print(f"[{i}/{len(THROUGHPUT_MODELS)}] {m}")
        run_throughput(m)

    print(f"\n=== Phase 2: Layer 3 fills ({len(L3_MODELS)} candidates) ===")
    for i, m in enumerate(L3_MODELS, 1):
        if has_l3(m):
            print(f"[{i}/{len(L3_MODELS)}] {m} -> SKIP (L3 already present)")
            continue
        print(f"[{i}/{len(L3_MODELS)}] {m}")
        run_l3(m)

    print("\n=== Follow-up benchmarks done ===")


if __name__ == "__main__":
    main()
