#!/usr/bin/env python3
"""Inspect chat-mode L2 failures."""
import json
import sys
from pathlib import Path

slug = sys.argv[1]
p = Path(r"C:\Development\OllamaBenchmarks\results") / f"coding-{slug}-chat.json"
d = json.loads(p.read_text(encoding="utf-8"))
r = d.get("layer2_chat_results", [])
print(f"keys: {list(d.keys())}")
print(f"records: {len(r)}")
print(f"passed: {sum(1 for x in r if x.get('passed'))}")
print()
print("first 5:")
for x in r[:5]:
    err = (x.get("error") or "").strip().splitlines()
    err_short = err[0][:120] if err else ""
    print(f"  {x['name']}: passed={x['passed']}, err={err_short!r}")
