#!/usr/bin/env python3
"""Run L2 chat-mode variant for chat-tuned models that scored weakly on raw L2.

Queued to run after the main fill_l2_gaps.py chain completes. The hypothesis:
gemma4 and similar chat-tuned models scored 4-8% on raw-mode L2 because the
runner uses raw:true and bypasses the chat template. With chat-mode, scores
should jump much higher.
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

# Chat-tuned models that scored <20% on raw L2 (or are expected to).
# Includes gemma4 family and any other chat-only models worth re-validating.
MODELS = [
    "gemma4",         # 6/158 raw
    "gemma4:e2b",     # 11/158 raw
    "gemma4:26b",     # raw pending
    "gemma4:31b",     # raw pending
    "lfm2.5-thinking:1.2b",  # 7/158 raw
    "sam860/lfm2:2.6b",      # 10/158 raw
    "LFM2-2.6b-tools",       # 10/158 raw
    "mistral-small",  # raw pending — chat-tuned, may benefit
    "phi4-mini",      # 17/158 raw — chat-tuned
    "nemotron-cascade-2",  # raw pending — small MoE chat
    "gpt-oss:20b",    # 3/158 raw — heavy reasoning + chat-tuned, very weak raw
    "gpt-oss:120b",   # raw pending — same pattern
    "qwen3:0.6b",     # tiny chat — for sanity check
    "qwen3.6",        # already 77/158 raw, but chat may differ
    "qwen3.6:27b",    # already 68/158 raw
    "qwen3-coder-next",   # already 80+/158 raw expected — sanity check
    "glm-4.7-flash",  # default thinker; raw may have issues like glm-4.5-air
    "glm-4.7-flash-reap-toolfix",  # raw pending
    "nemotron-3-super",  # raw pending — heavy reasoning
    "nemotron-3-nano",   # raw pending
    "nemotron-3-nano:30b-a3b-q8_0",  # raw pending
    "nemotron-nano-9b-v2-toolfix",   # raw pending
    "llama4:16x17b",  # raw pending — chat-tuned MoE
    "trinity-mini:Q4_K_M",  # raw pending
    "RogerBen/qwen3.5-35b-opus-distill",  # raw pending — opus-distill chat
]


def slug(model: str) -> str:
    if model.endswith(":latest"):
        model = model[: -len(":latest")]
    s = re.sub(r"[:/\\]", "_", model)
    return re.sub(r"[^\w.\-]", "_", s)


def raw_l2_score(s: str) -> str:
    p = RESULTS / f"coding-{s}.json"
    if not p.exists():
        return "?"
    try:
        d = json.loads(p.read_text(encoding="utf-8"))
        pr = d.get("layer2_pass_rate")
        if pr is None:
            return "?"
        return f"{d.get('layer2_passed','?')}/{d.get('layer2_total','?')} ({pr:.4f})"
    except Exception:
        return "?"


def main():
    completed = []
    skipped = []
    failed = []

    for i, model in enumerate(MODELS, 1):
        s = slug(model)
        out = RESULTS / f"coding-{s}-chat.json"
        if out.exists():
            try:
                d = json.loads(out.read_text(encoding="utf-8"))
                if d.get("layer2_chat_total", 0) >= 158:
                    print(f"[{i}/{len(MODELS)}] {model} -> SKIP (chat L2 already done)")
                    skipped.append(model); continue
            except Exception:
                pass

        raw = raw_l2_score(s)
        print(f"[{i}/{len(MODELS)}] {model} -> running chat-mode L2 (raw was {raw}) ...", flush=True)
        t0 = datetime.datetime.now(datetime.timezone.utc)
        try:
            res = subprocess.run(
                [
                    sys.executable,
                    "scripts/benchmark_coding_layer2_chat.py",
                    "--models", model,
                    "--dataset-path",
                    "scripts/coding_tasks/datasets/data/humaneval-cs-reworded.json",
                ],
                cwd=ROOT,
                capture_output=True,
                text=True,
                timeout=6 * 60 * 60,
            )
            elapsed = datetime.datetime.now(datetime.timezone.utc) - t0
            pf = "?"; score = "?"
            if out.exists():
                d = json.loads(out.read_text(encoding="utf-8"))
                pf = f"{d.get('layer2_chat_passed','?')}/{d.get('layer2_chat_total','?')}"
                pr = d.get("layer2_chat_pass_rate")
                score = f"{pr:.4f}" if pr is not None else "?"
            print(f"    -> {pf} chat (L2_chat={score}) in {elapsed}; raw baseline was {raw}", flush=True)
            if res.returncode != 0:
                print(f"    [warn] exit={res.returncode}: {(res.stderr or '')[-200:]}", flush=True)
            completed.append((model, pf, score, raw))
        except subprocess.TimeoutExpired:
            print(f"    -> TIMEOUT after 6h"); failed.append((model, "timeout"))
        except Exception as exc:
            print(f"    -> ERROR: {exc}"); failed.append((model, str(exc)))

    print("\n" + "=" * 70)
    print("=== SUMMARY (L2 chat vs raw) ===")
    print(f"{'Model':<40} {'chat':<22} {'raw':<22}")
    for m, pf, s, r in completed:
        print(f"  {m:<40} {pf+' ('+s+')':<22} {r:<22}")
    if skipped:
        print(f"\nSkipped: {len(skipped)}")
    if failed:
        print(f"\nFailed:")
        for m, r in failed: print(f"  {m}: {r}")


if __name__ == "__main__":
    main()
