#!/usr/bin/env python3
"""Inspect L2 failure modes for a given model."""
import json
import sys
from collections import Counter
from pathlib import Path

slug = sys.argv[1] if len(sys.argv) > 1 else "gemma4"
p = Path(r"C:\Development\OllamaBenchmarks\results") / f"coding-{slug}.json"
d = json.loads(p.read_text(encoding="utf-8"))
l2 = d.get("layer2_results", [])

print(f"Model: {d.get('model')}")
print(f"Total L2: {len(l2)}")
print(f"Passed:  {sum(1 for r in l2 if r.get('passed'))}")

errs = Counter()
samples = {}
for r in l2:
    if r.get("passed"):
        continue
    err = (r.get("error") or "").strip()
    if not err:
        key = "(empty error)"
    else:
        # categorize: first 60 chars after splitting on newline
        first = err.splitlines()[0][:80]
        key = first
    errs[key] += 1
    if key not in samples:
        samples[key] = (r.get("name"), err[:600])

print(f"\nTop failure modes:")
for k, n in errs.most_common(10):
    print(f"  [{n:>3}] {k}")

print(f"\nFirst example per top mode:")
for k, n in errs.most_common(5):
    name, err = samples[k]
    print(f"\n  === [{n}x] {k}")
    print(f"  task: {name}")
    print(f"  err: {err[:500]!r}")
