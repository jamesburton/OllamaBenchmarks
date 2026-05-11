#!/usr/bin/env python3
"""Print L3 no-think vs think:true comparison table."""
import json
from pathlib import Path

R = Path(r"C:\Development\OllamaBenchmarks\results")

MODELS = [
    "gemma4_e2b", "qwen3.6", "qwen3.6_27b", "qwen3_14b", "glm-4.7-flash",
    "nemotron-3-nano_30b-a3b-q8_0", "gpt-oss_20b", "nemotron-3-nano",
    "nemotron-3-super", "granite4_7b-a1b-h", "granite4_32b-a9b-h",
    "qwen3-coder-next",
]


def read(p: Path):
    if not p.exists():
        return None
    d = json.loads(p.read_text(encoding="utf-8"))
    if d.get("skipped"):
        return ("SKIPPED", None)
    rs = d.get("layer3_results", [])
    if not rs:
        return None
    n_pass = sum(1 for r in rs if r.get("passed"))
    n_tot = len(rs)
    score = d.get("layer3_weighted_score")
    return (f"{n_pass}/{n_tot}", score)


def fmt(v):
    if v is None:
        return "-"
    if v[0] == "SKIPPED":
        return "SKIPPED"
    pf, score = v
    if score is None:
        return f"{pf} (partial)"
    return f"{pf} ({score:.4f})"


print(f"{'slug':<40} {'no-think':<22} {'think':<22}")
print("-" * 84)
for m in MODELS:
    nt = read(R / f"coding-{m}.json")
    th = read(R / f"coding-{m}-think.json")
    print(f"{m:<40} {fmt(nt):<22} {fmt(th):<22}")
