#!/usr/bin/env python3
"""Debug the chat-mode L2 for a single problem — print raw response, extracted, and assembled program."""
import json
import os
import sys

sys.path.insert(0, os.path.dirname(__file__))
import benchmark_coding_layer2 as L2
import benchmark_coding_layer2_chat as L2C
from coding_tasks.code_extractor import extract_csharp

model = sys.argv[1] if len(sys.argv) > 1 else "gemma4"
task_name = sys.argv[2] if len(sys.argv) > 2 else "HumanEval_0_has_close_elements"

ds = L2.load_dataset("scripts/coding_tasks/datasets/data/humaneval-cs-reworded.json")
p = next(x for x in ds if x["name"] == task_name)

print("=" * 80)
print("PROMPT (prefix):")
print(p["prompt"][:400] + "..." if len(p["prompt"]) > 400 else p["prompt"])
print("=" * 80)
print("TESTS (first 200 chars):")
print(p["tests"][:200] + "..." if len(p["tests"]) > 200 else p["tests"])
print("=" * 80)

raw = L2C._chat_complete(model, p["prompt"])
print("RAW MODEL RESPONSE (first 1500 chars):")
print(raw[:1500])
print("=" * 80)

extracted = extract_csharp(raw) or raw.strip()
print(f"EXTRACTED ({len(extracted)} chars):")
print(extracted[:1500])
print("=" * 80)

if "class Problem" in extracted and "public static void Main" not in extracted:
    stripped = extracted.rstrip()
    if stripped.endswith("}"):
        stripped = stripped[:-1].rstrip()
    program = stripped + "\n" + p["tests"]
    program = L2C._inject_pass(program)
    print("BRANCH: standalone-class with stripped trailing brace")
else:
    program = L2._build_program_cs(p["prompt"], extracted, p["tests"])
    print("BRANCH: fallback to _build_program_cs")
print(f"ASSEMBLED PROGRAM ({len(program)} chars):")
print(program[:2500])
print("=" * 80)

# Write program to a temp file and try to build
import tempfile, shutil, subprocess
work_dir = tempfile.mkdtemp(prefix="debug_l2_")
shutil.rmtree(work_dir)
shutil.copytree("scripts/coding_tasks/templates/.cache/layer2_project", work_dir)
with open(os.path.join(work_dir, "Program.cs"), "w", encoding="utf-8") as fh:
    fh.write(program)
print(f"Built at: {work_dir}")
r = subprocess.run(["dotnet", "run", "--no-restore"], cwd=work_dir, capture_output=True, text=True, timeout=60)
print(f"exit={r.returncode}")
print(f"stdout: {r.stdout[:500]}")
print(f"stderr: {r.stderr[:1500]}")
