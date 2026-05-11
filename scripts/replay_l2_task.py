#!/usr/bin/env python3
"""Re-run a single L2 problem to see the generated code for a model."""
import json
import sys
import urllib.request

model = sys.argv[1]
task_name = sys.argv[2] if len(sys.argv) > 2 else "HumanEval_0_has_close_elements"

dataset = json.load(open(r"C:\Development\OllamaBenchmarks\scripts\coding_tasks\datasets\data\humaneval-cs-reworded.json"))
problem = next((p for p in dataset if p.get("name") == task_name), None)
if not problem:
    print(f"Task {task_name} not found")
    sys.exit(1)

prompt = problem.get("prompt", "")
print(f"=== PROMPT ===\n{prompt}\n")

payload = {
    "model": model,
    "prompt": prompt,
    "stream": False,
    "think": False,
    "options": {"num_predict": 1024, "num_ctx": 4096, "temperature": 0, "top_p": 1},
}
req = urllib.request.Request(
    "http://127.0.0.1:11434/api/generate",
    data=json.dumps(payload).encode("utf-8"),
    headers={"Content-Type": "application/json"},
    method="POST",
)
with urllib.request.urlopen(req, timeout=300) as r:
    body = json.loads(r.read().decode("utf-8"))
print(f"=== RAW RESPONSE ===\n{body.get('response','')}\n")
