#!/usr/bin/env python3
"""Print benchmark coverage for installed Ollama models.

For each installed model, checks whether quality, throughput-resource,
Layer 2 (HumanEval-CS, 158 tasks), and Layer 3 (.NET, 50 tasks) results exist.
"""
import json
import os
import re
import subprocess
import sys
from pathlib import Path

RESULTS = Path(r"C:\Development\OllamaBenchmarks\results")
OLLAMA = r"C:\Users\james\AppData\Local\Programs\Ollama\ollama.exe"


def slug(model: str) -> str:
    if model.endswith(":latest"):
        model = model[: -len(":latest")]
    s = re.sub(r"[:/\\]", "_", model)
    return re.sub(r"[^\w.\-]", "_", s)


def installed():
    out = subprocess.check_output([OLLAMA, "list"], text=True).splitlines()[1:]
    models = []
    for line in out:
        if not line.strip():
            continue
        models.append(line.split()[0])
    return models


def quality_path(s: str) -> Path | None:
    p = RESULTS / f"quality-{s}.json"
    return p if p.exists() else None


def throughput_path(s: str) -> Path | None:
    p = RESULTS / f"throughput-resource-{s}.json"
    return p if p.exists() else None


def coding_paths(s: str):
    """Return (l2_count, l3_count, source_path) by reading coding-{s}.json or coding-layer3-{s}.json."""
    l2_count = 0
    l3_count = 0
    src = None
    composite = RESULTS / f"coding-{s}.json"
    if composite.exists():
        try:
            d = json.loads(composite.read_text(encoding="utf-8"))
            l2_count = len(d.get("layer2_results", []))
            l3_count = len(d.get("layer3_results", []))
            src = composite.name
        except Exception as e:
            print(f"  WARN: failed to read {composite.name}: {e}", file=sys.stderr)
    if l3_count == 0:
        l3_only = RESULTS / f"coding-layer3-{s}.json"
        if l3_only.exists():
            try:
                d = json.loads(l3_only.read_text(encoding="utf-8"))
                # may be layer3-only checkpoint — list of records
                if isinstance(d, list):
                    l3_count = len(d)
                else:
                    l3_count = len(d.get("results", d.get("layer3_results", [])))
                src = (src or "") + ("," if src else "") + l3_only.name
            except Exception as e:
                print(f"  WARN: failed to read {l3_only.name}: {e}", file=sys.stderr)
    return l2_count, l3_count, src


def main():
    models = installed()
    rows = []
    for model in models:
        s = slug(model)
        q = quality_path(s)
        t = throughput_path(s)
        l2, l3, src = coding_paths(s)
        rows.append((model, s, bool(q), bool(t), l2, l3, src))

    print(f"{'Model':<70} {'Slug':<60} Q  T  L2   L3   Source")
    print("-" * 200)
    for m, s, q, t, l2, l3, src in rows:
        flagq = "Y" if q else "."
        flagt = "Y" if t else "."
        print(f"{m:<70} {s:<60} {flagq}  {flagt}  {l2:<4} {l3:<4} {src or '-'}")

    # Summary of missing
    print("\n=== Missing items by model ===")
    for m, s, q, t, l2, l3, src in rows:
        missing = []
        if not q:
            missing.append("quality")
        if not t:
            missing.append("throughput")
        if l2 < 158:
            missing.append(f"L2({l2}/158)")
        if l3 < 50:
            missing.append(f"L3({l3}/50)")
        if missing:
            print(f"  {m}: {', '.join(missing)}")


if __name__ == "__main__":
    main()
