# VibeThinker-3B C# Fine-Tune — Phase 1 (Proof-of-Life) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the Stream A data pipeline (The Stack v1 C# → completion-style chat JSONL), adapt the LoRA trainer for VibeThinker-3B, and run one full 5k-example proof-of-life iteration (train → GGUF → Ollama → benchmark) measured against the L2-chat 25/158 baseline.

**Architecture:** A dataset builder with small pure functions (license/size/modernity filters, function extraction, chat conversion, dedup, stratified hold-out split) feeds a JSONL training set. A trainer adapted from the existing `train_qwen35_lora.py` runs bf16 LoRA on the RTX 3060, merges, and exports Q4_K_M GGUF. The existing benchmark harness scores the result under a new model slug.

**Tech Stack:** Python 3.12, HuggingFace `datasets` 4.3.0, `transformers`/`peft` 0.18.1/`trl` 0.24.0, `torch` 2.6.0+cu124, llama.cpp (`C:\adl\Programs\llama-cpp`), Ollama 0.30.10, pytest.

## Global Constraints

- Base model: `WeiboAI/VibeThinker-3B` (Qwen2.5-Coder-3B arch; LoRA target modules `q_proj,k_proj,v_proj,o_proj,gate_proj,up_proj,down_proj`).
- System prompt (verbatim, reused everywhere): `You are an expert C#/.NET developer. When asked to write code, return ONLY valid C# code in a single file. Do not include markdown fences, explanations, or commentary — just the raw C# source code.`
- Training format: chat JSONL, one object per line, key `messages` = list of `{role, content}` with roles `system`, `user`, `assistant`.
- Dataset source (phase 1): `bigcode/the-stack-dedup`, C# subset, permissive licenses only.
- Max sequence length: 4096 tokens.
- Hold-out: 10% stratified, written to a separate file, never used in training.
- Eval baselines to beat: L2 chat 25/158 (0.158), L3 0/50. Proof-of-life gate: L2 chat ≥ 40/158 on the 5k slice.
- Slug convention: strip `:latest`, replace `[:/\\]` with `_`. Proof-of-life model name: `vibethinker-csharp-p1-5k`.
- think:true is BROKEN on this model family (empty content). Never set `CODING_BENCH_THINK=true`. The benchmark extractors strip `<think>` tags automatically.
- All new pipeline code lives under `scripts/lora/`. Tests live beside the code as `scripts/lora/test_*.py`.

---

## File Structure

- Create: `scripts/lora/build_stack_csharp_dataset.py` — Stream A pipeline + pure helper functions.
- Create: `scripts/lora/test_build_stack_csharp_dataset.py` — pytest unit tests for the helpers.
- Create: `scripts/lora/train_vibethinker_lora.py` — trainer adapted from `train_qwen35_lora.py`.
- Create: `scripts/lora/check_lora_env.py` — environment/dependency verifier.
- Create: `docs/superpowers/plans/2026-06-21-vibethinker-csharp-phase1.md` — this plan (already created).
- Modify: `scripts/lora/requirements.txt` — pin `pytest` for the test suite.
- Output (gitignored large artifacts; JSONL committed): `scripts/lora/data/stack_csharp_train.jsonl`, `scripts/lora/data/stack_csharp_holdout.jsonl`.

---

### Task 1: Environment verifier

**Files:**
- Create: `scripts/lora/check_lora_env.py`
- Modify: `scripts/lora/requirements.txt`

**Interfaces:**
- Produces: `check_lora_env.py` exits 0 when all deps + CUDA + HF token present, exits 1 otherwise, printing a per-item report.

- [ ] **Step 1: Add pytest to requirements**

Append to `scripts/lora/requirements.txt`:

```
# Test suite
pytest>=8.0.0
```

- [ ] **Step 2: Write the verifier**

Create `scripts/lora/check_lora_env.py`:

```python
#!/usr/bin/env python3
"""Verify the LoRA fine-tuning environment is ready.

Checks: torch + CUDA, datasets, peft, trl, transformers, accelerate, and the
HF_TOKEN environment variable (needed to stream bigcode/the-stack-dedup).

Exit 0 if all green, 1 if any check fails.
"""
import importlib
import os
import sys

REQUIRED = ["torch", "datasets", "peft", "trl", "transformers", "accelerate"]


def main() -> int:
    ok = True
    for mod in REQUIRED:
        try:
            m = importlib.import_module(mod)
            print(f"  OK   {mod} {getattr(m, '__version__', '?')}")
        except Exception as e:  # noqa: BLE001 - report any import failure
            print(f"  FAIL {mod}: {e}")
            ok = False

    try:
        import torch
        if torch.cuda.is_available():
            print(f"  OK   CUDA available: {torch.cuda.get_device_name(0)}")
        else:
            print("  FAIL CUDA not available")
            ok = False
    except Exception as e:  # noqa: BLE001
        print(f"  FAIL CUDA check: {e}")
        ok = False

    if os.environ.get("HF_TOKEN") or os.environ.get("HUGGING_FACE_HUB_TOKEN"):
        print("  OK   HF token present")
    else:
        print("  WARN HF_TOKEN not set — public Stack access is rate-limited")

    print("READY" if ok else "NOT READY")
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main())
```

- [ ] **Step 3: Run the verifier**

Run: `python scripts/lora/check_lora_env.py`
Expected: lines beginning `OK` for torch/datasets/peft/trl/transformers/accelerate and CUDA, final line `READY`, exit 0. (Imports of transformers can take ~30–60 s; allow it to finish.)

- [ ] **Step 4: Commit**

```bash
git add scripts/lora/check_lora_env.py scripts/lora/requirements.txt
git commit -m "feat: LoRA environment verifier for VibeThinker C# fine-tune"
```

---

### Task 2: C# filter functions

**Files:**
- Create: `scripts/lora/build_stack_csharp_dataset.py`
- Create: `scripts/lora/test_build_stack_csharp_dataset.py`

**Interfaces:**
- Produces:
  - `PERMISSIVE_LICENSES: set[str]` — allowed SPDX-ish license strings.
  - `passes_license(record: dict) -> bool` — record has a `"license"` or `"max_stars_repo_licenses"` field (list or str); True if any entry is in `PERMISSIVE_LICENSES`.
  - `passes_size(text: str, lo: int = 200, hi: int = 8192) -> bool` — True if `lo <= len(text.encode("utf-8")) <= hi`.
  - `is_modern_csharp(text: str) -> bool` — True if the file shows modern C# (`namespace`, `record`, or `async`) and shows no hard pre-C#8 marker (no `using System;`-only top with no `namespace`; specifically returns False if it contains none of the modern markers).

- [ ] **Step 1: Write the failing test**

Create `scripts/lora/test_build_stack_csharp_dataset.py`:

```python
import build_stack_csharp_dataset as b


def test_passes_license_accepts_mit():
    assert b.passes_license({"license": "MIT"}) is True


def test_passes_license_accepts_list_field():
    assert b.passes_license({"max_stars_repo_licenses": ["Apache-2.0"]}) is True


def test_passes_license_rejects_gpl():
    assert b.passes_license({"license": "GPL-3.0"}) is False


def test_passes_license_rejects_missing():
    assert b.passes_license({}) is False


def test_passes_size_within_bounds():
    assert b.passes_size("x" * 500) is True


def test_passes_size_too_small():
    assert b.passes_size("tiny") is False


def test_passes_size_too_large():
    assert b.passes_size("x" * 9000) is False


def test_is_modern_csharp_true_for_record():
    assert b.is_modern_csharp("public record Person(string Name);") is True


def test_is_modern_csharp_true_for_async_namespace():
    code = "namespace App;\npublic class C { async Task M() {} }"
    assert b.is_modern_csharp(code) is True


def test_is_modern_csharp_false_for_legacy():
    assert b.is_modern_csharp("class Foo { void Bar() {} }") is False
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd scripts/lora && python -m pytest test_build_stack_csharp_dataset.py -v`
Expected: FAIL — `ModuleNotFoundError: No module named 'build_stack_csharp_dataset'`.

- [ ] **Step 3: Write minimal implementation**

Create `scripts/lora/build_stack_csharp_dataset.py`:

```python
#!/usr/bin/env python3
"""Build a completion-style C# training set from The Stack v1 (dedup).

Streams bigcode/the-stack-dedup (C# subset), filters to permissive-licensed
modern C# in a usable size range, extracts function bodies, and writes
chat-format JSONL plus a stratified hold-out split.

Pure helper functions are unit-tested in test_build_stack_csharp_dataset.py.
"""
from __future__ import annotations

PERMISSIVE_LICENSES = {
    "MIT", "Apache-2.0", "BSD-3-Clause", "BSD-2-Clause", "ISC", "MIT-0",
    "Unlicense", "0BSD", "Apache-2.0+", "BSD",
}

MODERN_MARKERS = ("namespace", "record", "async")


def passes_license(record: dict) -> bool:
    val = record.get("license") or record.get("max_stars_repo_licenses")
    if val is None:
        return False
    licenses = val if isinstance(val, (list, tuple)) else [val]
    return any(str(lic).strip() in PERMISSIVE_LICENSES for lic in licenses)


def passes_size(text: str, lo: int = 200, hi: int = 8192) -> bool:
    n = len(text.encode("utf-8"))
    return lo <= n <= hi


def is_modern_csharp(text: str) -> bool:
    return any(marker in text for marker in MODERN_MARKERS)
```

- [ ] **Step 4: Run test to verify it passes**

Run: `cd scripts/lora && python -m pytest test_build_stack_csharp_dataset.py -v`
Expected: PASS (10 passed).

- [ ] **Step 5: Commit**

```bash
git add scripts/lora/build_stack_csharp_dataset.py scripts/lora/test_build_stack_csharp_dataset.py
git commit -m "feat: C# filter functions for Stack dataset builder (TDD)"
```

---

### Task 3: Function extraction + chat conversion

**Files:**
- Modify: `scripts/lora/build_stack_csharp_dataset.py`
- Modify: `scripts/lora/test_build_stack_csharp_dataset.py`

**Interfaces:**
- Consumes: nothing from prior tasks (uses stdlib `re`).
- Produces:
  - `SYSTEM_PROMPT: str` — the verbatim Global-Constraints system prompt.
  - `extract_functions(text: str) -> list[tuple[str, str]]` — returns `(signature_line, body_with_braces)` pairs for top-level-ish methods. A function is a line matching a C# method signature immediately followed by a balanced `{ ... }` block. Returns `[]` if none found.
  - `to_chat_example(signature: str, body: str) -> dict` — returns `{"messages": [system, user, assistant]}` where user asks to implement the signature and assistant is `signature + "\n" + body`.

- [ ] **Step 1: Write the failing test**

Append to `scripts/lora/test_build_stack_csharp_dataset.py`:

```python
def test_extract_functions_finds_one():
    code = (
        "public int Add(int a, int b)\n"
        "{\n"
        "    return a + b;\n"
        "}\n"
    )
    fns = b.extract_functions(code)
    assert len(fns) == 1
    sig, body = fns[0]
    assert "Add(int a, int b)" in sig
    assert "return a + b;" in body
    assert body.strip().startswith("{")
    assert body.strip().endswith("}")


def test_extract_functions_handles_nested_braces():
    code = (
        "public void M()\n"
        "{\n"
        "    if (true) { Console.WriteLine(1); }\n"
        "}\n"
    )
    fns = b.extract_functions(code)
    assert len(fns) == 1
    assert "Console.WriteLine(1);" in fns[0][1]


def test_extract_functions_empty_when_none():
    assert b.extract_functions("int x = 5;") == []


def test_to_chat_example_shape():
    ex = b.to_chat_example("public int Add(int a, int b)", "{ return a + b; }")
    roles = [m["role"] for m in ex["messages"]]
    assert roles == ["system", "user", "assistant"]
    assert ex["messages"][0]["content"] == b.SYSTEM_PROMPT
    assert "Add(int a, int b)" in ex["messages"][1]["content"]
    assert ex["messages"][2]["content"].startswith("public int Add")
    assert "return a + b;" in ex["messages"][2]["content"]
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd scripts/lora && python -m pytest test_build_stack_csharp_dataset.py -v`
Expected: FAIL — `AttributeError: module ... has no attribute 'extract_functions'`.

- [ ] **Step 3: Write minimal implementation**

Add to `scripts/lora/build_stack_csharp_dataset.py` (add `import re` at top):

```python
import re

SYSTEM_PROMPT = (
    "You are an expert C#/.NET developer. When asked to write code, "
    "return ONLY valid C# code in a single file. Do not include markdown "
    "fences, explanations, or commentary — just the raw C# source code."
)

# A C# method signature: optional modifiers, a return type, a name, a
# parenthesised parameter list, then an opening brace on the same or next line.
_SIG_RE = re.compile(
    r"^[ \t]*"
    r"(?:(?:public|private|protected|internal|static|async|virtual|override|sealed|"
    r"partial|new|extern|unsafe)\s+)*"
    r"[\w<>\[\],\.\?]+\s+"          # return type
    r"[A-Za-z_]\w*\s*"              # method name
    r"\([^;{]*\)\s*"               # parameter list (no ; — excludes declarations)
    r"$",
    re.MULTILINE,
)


def _match_block(text: str, brace_start: int) -> int:
    """Return the index just past the matching close brace, or -1."""
    depth = 0
    for i in range(brace_start, len(text)):
        if text[i] == "{":
            depth += 1
        elif text[i] == "}":
            depth -= 1
            if depth == 0:
                return i + 1
    return -1


def extract_functions(text: str) -> list[tuple[str, str]]:
    results: list[tuple[str, str]] = []
    for m in _SIG_RE.finditer(text):
        sig = m.group(0).strip()
        # Find the next '{' after the signature.
        rest = text[m.end():]
        brace_rel = rest.find("{")
        if brace_rel == -1:
            continue
        brace_abs = m.end() + brace_rel
        end = _match_block(text, brace_abs)
        if end == -1:
            continue
        body = text[brace_abs:end]
        results.append((sig, body))
    return results


def to_chat_example(signature: str, body: str) -> dict:
    user = (
        "Implement the following C# method. Return only the complete method.\n\n"
        f"{signature}"
    )
    assistant = f"{signature}\n{body}"
    return {
        "messages": [
            {"role": "system", "content": SYSTEM_PROMPT},
            {"role": "user", "content": user},
            {"role": "assistant", "content": assistant},
        ]
    }
```

- [ ] **Step 4: Run test to verify it passes**

Run: `cd scripts/lora && python -m pytest test_build_stack_csharp_dataset.py -v`
Expected: PASS (14 passed).

- [ ] **Step 5: Commit**

```bash
git add scripts/lora/build_stack_csharp_dataset.py scripts/lora/test_build_stack_csharp_dataset.py
git commit -m "feat: C# function extraction + chat conversion (TDD)"
```

---

### Task 4: Dedup + stratified hold-out split

**Files:**
- Modify: `scripts/lora/build_stack_csharp_dataset.py`
- Modify: `scripts/lora/test_build_stack_csharp_dataset.py`

**Interfaces:**
- Consumes: `to_chat_example` output shape (`{"messages": [...]}`) from Task 3.
- Produces:
  - `content_key(example: dict) -> str` — a normalized hash key (sha1 of the assistant content with whitespace collapsed) for near-duplicate detection.
  - `dedup(examples: list[dict]) -> list[dict]` — keeps first occurrence per `content_key`, preserves order.
  - `split_holdout(examples: list[dict], fraction: float = 0.10, seed: int = 42) -> tuple[list[dict], list[dict]]` — returns `(train, holdout)`; holdout is `round(len*fraction)` items chosen by a seeded shuffle; deterministic.

- [ ] **Step 1: Write the failing test**

Append to `scripts/lora/test_build_stack_csharp_dataset.py`:

```python
def _ex(content):
    return {"messages": [
        {"role": "system", "content": b.SYSTEM_PROMPT},
        {"role": "user", "content": "u"},
        {"role": "assistant", "content": content},
    ]}


def test_content_key_ignores_whitespace():
    assert b.content_key(_ex("a  b\n c")) == b.content_key(_ex("a b c"))


def test_dedup_removes_duplicates():
    items = [_ex("same"), _ex("same"), _ex("different")]
    out = b.dedup(items)
    assert len(out) == 2


def test_split_holdout_sizes():
    items = [_ex(str(i)) for i in range(100)]
    train, holdout = b.split_holdout(items, fraction=0.10, seed=42)
    assert len(holdout) == 10
    assert len(train) == 90
    # disjoint
    train_keys = {b.content_key(x) for x in train}
    hold_keys = {b.content_key(x) for x in holdout}
    assert train_keys.isdisjoint(hold_keys)


def test_split_holdout_deterministic():
    items = [_ex(str(i)) for i in range(100)]
    a1, b1 = b.split_holdout(items, seed=42)
    a2, b2 = b.split_holdout(items, seed=42)
    assert [b.content_key(x) for x in b1] == [b.content_key(x) for x in b2]
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd scripts/lora && python -m pytest test_build_stack_csharp_dataset.py -v`
Expected: FAIL — `AttributeError: ... 'content_key'`.

- [ ] **Step 3: Write minimal implementation**

Add to `scripts/lora/build_stack_csharp_dataset.py` (add `import hashlib`, `import random` at top):

```python
import hashlib
import random


def content_key(example: dict) -> str:
    assistant = example["messages"][-1]["content"]
    normalized = " ".join(assistant.split())
    return hashlib.sha1(normalized.encode("utf-8")).hexdigest()


def dedup(examples: list[dict]) -> list[dict]:
    seen: set[str] = set()
    out: list[dict] = []
    for ex in examples:
        k = content_key(ex)
        if k not in seen:
            seen.add(k)
            out.append(ex)
    return out


def split_holdout(
    examples: list[dict], fraction: float = 0.10, seed: int = 42
) -> tuple[list[dict], list[dict]]:
    idx = list(range(len(examples)))
    random.Random(seed).shuffle(idx)
    n_hold = round(len(examples) * fraction)
    hold_idx = set(idx[:n_hold])
    train = [ex for i, ex in enumerate(examples) if i not in hold_idx]
    holdout = [examples[i] for i in idx[:n_hold]]
    return train, holdout
```

- [ ] **Step 4: Run test to verify it passes**

Run: `cd scripts/lora && python -m pytest test_build_stack_csharp_dataset.py -v`
Expected: PASS (18 passed).

- [ ] **Step 5: Commit**

```bash
git add scripts/lora/build_stack_csharp_dataset.py scripts/lora/test_build_stack_csharp_dataset.py
git commit -m "feat: dedup + stratified hold-out split (TDD)"
```

---

### Task 5: Wire the streaming pipeline (CLI + smoke test)

**Files:**
- Modify: `scripts/lora/build_stack_csharp_dataset.py`

**Interfaces:**
- Consumes: all helpers from Tasks 2–4.
- Produces: a `main()` CLI accepting `--target N`, `--out-dir DIR`, `--limit N` (max raw files to scan, for smoke runs), `--seed`. Writes `stack_csharp_train.jsonl` and `stack_csharp_holdout.jsonl` to `--out-dir`. Prints counts. Streams `bigcode/the-stack-dedup` with `data_dir="data/c-sharp"`, `streaming=True`.

- [ ] **Step 1: Add the streaming pipeline and CLI**

Add to `scripts/lora/build_stack_csharp_dataset.py` (add `import argparse`, `import json`, `import sys` and `from pathlib import Path` at top):

```python
import argparse
import json
import sys
from pathlib import Path


def build(target: int, limit: int | None, seed: int):
    from datasets import load_dataset

    ds = load_dataset(
        "bigcode/the-stack-dedup",
        data_dir="data/c-sharp",
        split="train",
        streaming=True,
    )

    examples: list[dict] = []
    scanned = 0
    for record in ds:
        scanned += 1
        if limit is not None and scanned > limit:
            break
        text = record.get("content") or ""
        if not passes_license(record):
            continue
        if not passes_size(text):
            continue
        if not is_modern_csharp(text):
            continue
        for sig, body in extract_functions(text):
            if not passes_size(body, lo=40, hi=6000):
                continue
            examples.append(to_chat_example(sig, body))
        if len(examples) >= target * 3:  # over-collect; dedup trims later
            break

    examples = dedup(examples)
    if len(examples) > target:
        examples = examples[:target]
    print(f"scanned={scanned} kept={len(examples)}")
    return split_holdout(examples, fraction=0.10, seed=seed)


def write_jsonl(path: Path, rows: list[dict]):
    path.parent.mkdir(parents=True, exist_ok=True)
    with open(path, "w", encoding="utf-8") as fh:
        for row in rows:
            fh.write(json.dumps(row, ensure_ascii=False) + "\n")


def main() -> int:
    ap = argparse.ArgumentParser(description="Build Stack v1 C# training set")
    ap.add_argument("--target", type=int, default=5000)
    ap.add_argument("--out-dir", type=Path, default=Path(__file__).parent / "data")
    ap.add_argument("--limit", type=int, default=None,
                    help="max raw files to scan (for smoke runs)")
    ap.add_argument("--seed", type=int, default=42)
    args = ap.parse_args()

    train, holdout = build(args.target, args.limit, args.seed)
    write_jsonl(args.out_dir / "stack_csharp_train.jsonl", train)
    write_jsonl(args.out_dir / "stack_csharp_holdout.jsonl", holdout)
    print(f"train={len(train)} holdout={len(holdout)} -> {args.out_dir}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
```

- [ ] **Step 2: Verify unit tests still pass (no regression)**

Run: `cd scripts/lora && python -m pytest test_build_stack_csharp_dataset.py -v`
Expected: PASS (18 passed) — the new imports and `main()` must not break the helpers.

- [ ] **Step 3: Smoke-run the pipeline against a small slice**

Run: `cd scripts/lora && python build_stack_csharp_dataset.py --target 30 --limit 200 --out-dir data_smoke`
Expected: prints `scanned=...`, `kept=...`, and `train=... holdout=... -> data_smoke`; creates `data_smoke/stack_csharp_train.jsonl` and `data_smoke/stack_csharp_holdout.jsonl`. (If `scanned` stays 0 with an auth error, set `HF_TOKEN` and retry — see Global Constraints fallback.)

- [ ] **Step 4: Inspect one example for schema correctness**

Run: `cd scripts/lora && python -c "import json; r=json.loads(open('data_smoke/stack_csharp_train.jsonl',encoding='utf-8').readline()); print([m['role'] for m in r['messages']]); print(r['messages'][2]['content'][:200])"`
Expected: `['system', 'user', 'assistant']` and an assistant snippet that is C# code starting with a method signature.

- [ ] **Step 5: Commit (code only — smoke data is disposable)**

```bash
rm -rf scripts/lora/data_smoke
git add scripts/lora/build_stack_csharp_dataset.py
git commit -m "feat: Stack v1 C# streaming pipeline + CLI"
```

---

### Task 6: Adapt the trainer for VibeThinker-3B

**Files:**
- Create: `scripts/lora/train_vibethinker_lora.py`

**Interfaces:**
- Consumes: `stack_csharp_train.jsonl` (chat JSONL from Task 5).
- Produces: a trainer CLI with `--training-data`, `--output-dir`, `--epochs`, `--lr`, `--lora-r`, `--lora-alpha`, `--max-seq-length`, `--max-steps` (for smoke), `--base-model` (default `WeiboAI/VibeThinker-3B`), `--phase {1,2}`. Phase 1 trains code-only; the `--phase` flag is recorded but phase-2 think-handling is a follow-up plan. Writes a LoRA adapter to `--output-dir`.

- [ ] **Step 1: Create the trainer (bf16-first, fp16-split fallback)**

Create `scripts/lora/train_vibethinker_lora.py`:

```python
#!/usr/bin/env python3
"""LoRA fine-tune WeiboAI/VibeThinker-3B for C#/.NET (phase 1, code-only).

Adapted from train_qwen35_lora.py. VibeThinker is Qwen2.5 arch — standard
target modules. At 3B, bf16 LoRA fits the RTX 3060 12GB on-GPU; we try that
first and fall back to an fp16 CPU/GPU split if it OOMs.
"""
import argparse
import sys
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
DEFAULT_TRAINING_DATA = SCRIPT_DIR / "data" / "stack_csharp_train.jsonl"
DEFAULT_OUTPUT_DIR = SCRIPT_DIR / "output" / "vibethinker-csharp-p1-lora"

BASE_MODEL = "WeiboAI/VibeThinker-3B"
LORA_DROPOUT = 0.05
TARGET_MODULES = [
    "q_proj", "k_proj", "v_proj", "o_proj",
    "gate_proj", "up_proj", "down_proj",
]
SYSTEM_PROMPT = (
    "You are an expert C#/.NET developer. When asked to write code, "
    "return ONLY valid C# code in a single file. Do not include markdown "
    "fences, explanations, or commentary — just the raw C# source code."
)


def parse_args():
    p = argparse.ArgumentParser(description="LoRA fine-tune VibeThinker-3B for C#")
    p.add_argument("--training-data", type=Path, default=DEFAULT_TRAINING_DATA)
    p.add_argument("--output-dir", type=Path, default=DEFAULT_OUTPUT_DIR)
    p.add_argument("--base-model", default=BASE_MODEL)
    p.add_argument("--epochs", type=int, default=3)
    p.add_argument("--batch-size", type=int, default=1)
    p.add_argument("--gradient-accumulation", type=int, default=8)
    p.add_argument("--lr", type=float, default=2e-4)
    p.add_argument("--max-seq-length", type=int, default=4096)
    p.add_argument("--lora-r", type=int, default=32)
    p.add_argument("--lora-alpha", type=int, default=64)
    p.add_argument("--max-steps", type=int, default=-1,
                   help="cap training steps (smoke runs); -1 = full")
    p.add_argument("--phase", type=int, choices=[1, 2], default=1)
    return p.parse_args()


def load_examples(path: Path) -> list[dict]:
    import json
    if not path.exists():
        print(f"ERROR: training data not found at {path}")
        print("Run build_stack_csharp_dataset.py first.")
        sys.exit(1)
    rows = []
    with open(path, "r", encoding="utf-8") as fh:
        for line in fh:
            if line.strip():
                rows.append(json.loads(line))
    print(f"Loaded {len(rows)} training examples")
    return rows


def train(args, examples, bf16_gpu_only: bool):
    import torch
    from transformers import AutoModelForCausalLM, AutoTokenizer
    from peft import LoraConfig, get_peft_model
    from trl import SFTTrainer, SFTConfig
    from datasets import Dataset

    label = "bf16 GPU-only" if bf16_gpu_only else "fp16 CPU/GPU split"
    print(f"\n=== Loading {args.base_model} ({label}) ===")

    tokenizer = AutoTokenizer.from_pretrained(
        args.base_model, trust_remote_code=True, padding_side="right"
    )
    if tokenizer.pad_token is None:
        tokenizer.pad_token = tokenizer.eos_token

    load_kwargs = {"trust_remote_code": True, "attn_implementation": "eager"}
    if bf16_gpu_only:
        load_kwargs["torch_dtype"] = torch.bfloat16
        load_kwargs["device_map"] = {"": "cuda:0"}
    else:
        load_kwargs["torch_dtype"] = torch.float16
        load_kwargs["device_map"] = {"": "cuda:0"}

    model = AutoModelForCausalLM.from_pretrained(args.base_model, **load_kwargs)
    model.gradient_checkpointing_enable()

    lora_config = LoraConfig(
        r=args.lora_r, lora_alpha=args.lora_alpha, lora_dropout=LORA_DROPOUT,
        bias="none", task_type="CAUSAL_LM", target_modules=TARGET_MODULES,
    )
    model = get_peft_model(model, lora_config)
    model.print_trainable_parameters()

    dataset = Dataset.from_list(examples)

    def formatting_func(example):
        return tokenizer.apply_chat_template(
            example["messages"], tokenize=False, add_generation_prompt=False
        )

    training_args = SFTConfig(
        output_dir=str(args.output_dir),
        num_train_epochs=args.epochs,
        max_steps=args.max_steps,
        per_device_train_batch_size=args.batch_size,
        gradient_accumulation_steps=args.gradient_accumulation,
        learning_rate=args.lr,
        weight_decay=0.01,
        warmup_ratio=0.1,
        lr_scheduler_type="cosine",
        logging_steps=5,
        save_strategy="epoch",
        save_total_limit=2,
        bf16=bf16_gpu_only,
        fp16=not bf16_gpu_only,
        max_length=args.max_seq_length,
        gradient_checkpointing=True,
        gradient_checkpointing_kwargs={"use_reentrant": False},
        optim="adamw_8bit",
        max_grad_norm=0.3,
        seed=42,
        report_to="none",
    )

    trainer = SFTTrainer(
        model=model, processing_class=tokenizer, train_dataset=dataset,
        args=training_args, formatting_func=formatting_func,
    )
    print(f"\n=== Training (phase {args.phase}, {label}) ===")
    trainer.train()
    print(f"\nSaving adapter to {args.output_dir}")
    model.save_pretrained(str(args.output_dir))
    tokenizer.save_pretrained(str(args.output_dir))


def main():
    args = parse_args()
    examples = load_examples(args.training_data)
    if not examples:
        sys.exit(1)
    try:
        train(args, examples, bf16_gpu_only=True)
    except RuntimeError as e:
        if "out of memory" in str(e).lower():
            print(f"\nOOM in bf16 GPU-only: {e}\nRetrying fp16 split...")
            import torch
            torch.cuda.empty_cache()
            train(args, examples, bf16_gpu_only=False)
        else:
            raise
    print("\n=== Training complete ===")
    print(f"Adapter: {args.output_dir}")


if __name__ == "__main__":
    main()
```

- [ ] **Step 2: Build a tiny dataset for the training smoke test**

Run: `cd scripts/lora && python build_stack_csharp_dataset.py --target 40 --limit 400 --out-dir data_smoke`
Expected: creates `data_smoke/stack_csharp_train.jsonl` with up to ~36 rows.

- [ ] **Step 3: Smoke-run training for 2 steps**

Run: `cd scripts/lora && python train_vibethinker_lora.py --training-data data_smoke/stack_csharp_train.jsonl --output-dir output/smoke-adapter --max-steps 2 --epochs 1`
Expected: model downloads on first run (VibeThinker-3B, ~6 GB), prints `trainable params`, runs 2 optimizer steps, writes `output/smoke-adapter/adapter_model.safetensors`. No OOM on the RTX 3060 (bf16 path). If it OOMs, the fp16 fallback message appears and it retries.

- [ ] **Step 4: Verify the adapter was written**

Run: `cd scripts/lora && python -c "import os; print(sorted(os.listdir('output/smoke-adapter')))"`
Expected: list includes `adapter_config.json` and `adapter_model.safetensors`.

- [ ] **Step 5: Commit (code only)**

```bash
rm -rf scripts/lora/data_smoke scripts/lora/output/smoke-adapter
git add scripts/lora/train_vibethinker_lora.py
git commit -m "feat: VibeThinker-3B LoRA trainer (bf16-first, fp16 fallback)"
```

---

### Task 7: Run the 5k proof-of-life train + GGUF export + Ollama import

**Files:**
- Uses: `build_stack_csharp_dataset.py`, `train_vibethinker_lora.py`, existing GGUF tooling at `C:\adl\Programs\llama-cpp`.

**Interfaces:**
- Consumes: the trained adapter from `train_vibethinker_lora.py`.
- Produces: an Ollama model named `vibethinker-csharp-p1-5k` loadable via `ollama run`.

- [ ] **Step 1: Build the real 5k dataset**

Run: `cd scripts/lora && python build_stack_csharp_dataset.py --target 5000 --out-dir data`
Expected: `train≈4500 holdout≈500 -> .../data`. Writes `data/stack_csharp_train.jsonl` and `data/stack_csharp_holdout.jsonl`. (Streaming + filtering ~15–40 min depending on hit rate.)

- [ ] **Step 2: Commit the dataset**

```bash
git add scripts/lora/data/stack_csharp_train.jsonl scripts/lora/data/stack_csharp_holdout.jsonl
git commit -m "data: 5k Stack v1 C# proof-of-life training set + holdout"
```

- [ ] **Step 3: Train the 5k phase-1 adapter**

Run: `cd scripts/lora && python train_vibethinker_lora.py --training-data data/stack_csharp_train.jsonl --output-dir output/vibethinker-csharp-p1-5k-lora --epochs 3`
Expected: 3 epochs over ~4500 examples; completes without OOM; adapter written to `output/vibethinker-csharp-p1-5k-lora/`. (Est. 2–5 h on RTX 3060.)

- [ ] **Step 4: Merge adapter into base weights**

Run:
```bash
cd scripts/lora && python -c "
import torch
from transformers import AutoModelForCausalLM, AutoTokenizer
from peft import PeftModel
base='WeiboAI/VibeThinker-3B'; adp='output/vibethinker-csharp-p1-5k-lora'; out='output/vibethinker-csharp-p1-5k-merged'
tok=AutoTokenizer.from_pretrained(adp, trust_remote_code=True)
m=AutoModelForCausalLM.from_pretrained(base, torch_dtype=torch.float16, device_map='cpu', trust_remote_code=True)
m=PeftModel.from_pretrained(m, adp).merge_and_unload()
m.save_pretrained(out, safe_serialization=True); tok.save_pretrained(out); print('merged ->', out)
"
```
Expected: prints `merged -> output/vibethinker-csharp-p1-5k-merged` with `*.safetensors` written.

- [ ] **Step 5: Convert to GGUF Q4_K_M**

Run:
```bash
cd scripts/lora && python C:/adl/Programs/llama-cpp/convert_hf_to_gguf.py output/vibethinker-csharp-p1-5k-merged --outfile output/vibethinker-csharp-p1-5k-f16.gguf --outtype f16 && C:/adl/Programs/llama-cpp/llama-quantize.exe output/vibethinker-csharp-p1-5k-f16.gguf output/vibethinker-csharp-p1-5k-Q4_K_M.gguf Q4_K_M
```
Expected: produces `output/vibethinker-csharp-p1-5k-Q4_K_M.gguf` (~1.9 GB). (qwen2 arch is mainline-supported.)

- [ ] **Step 6: Create the Ollama model**

Write `scripts/lora/output/Modelfile-p1-5k`:

```
FROM ./vibethinker-csharp-p1-5k-Q4_K_M.gguf

PARAMETER temperature 0.2
PARAMETER top_p 0.9
PARAMETER num_ctx 8192
PARAMETER stop "<|im_end|>"
PARAMETER stop "<|endoftext|>"

SYSTEM "You are an expert C#/.NET developer. When asked to write code, return ONLY valid C# code in a single file. Do not include markdown fences, explanations, or commentary — just the raw C# source code."
```

Run: `cd scripts/lora/output && ollama create vibethinker-csharp-p1-5k -f Modelfile-p1-5k`
Expected: `success`. Verify: `ollama list` shows `vibethinker-csharp-p1-5k`.

- [ ] **Step 7: Smoke-test the model**

Run: `ollama run vibethinker-csharp-p1-5k "Write a C# method that reverses a string."`
Expected: returns C# code (a method using `Array.Reverse` or LINQ), not prose-only. Minor imperfections are fine — this only confirms the model loads and emits C#.

- [ ] **Step 8: Commit the Modelfile**

```bash
git add scripts/lora/output/Modelfile-p1-5k
git commit -m "feat: Ollama Modelfile for vibethinker-csharp-p1-5k proof-of-life"
```

---

### Task 8: Benchmark the proof-of-life model and record the gate result

**Files:**
- Uses: existing `scripts/benchmark_coding_layer2_chat.py`, `scripts/benchmark_coding_layer3.py`.
- Modify: `benchmark-models.json` (add `backend_notes` entry).

**Interfaces:**
- Consumes: the `vibethinker-csharp-p1-5k` Ollama model.
- Produces: `results/coding-vibethinker-csharp-p1-5k-chat.json`, `results/coding-vibethinker-csharp-p1-5k.json` (L3), and a recorded verdict against the gate.

- [ ] **Step 1: Run L2 chat-mode benchmark**

Run: `cd c:/Development/OllamaBenchmarks && python scripts/benchmark_coding_layer2_chat.py --models vibethinker-csharp-p1-5k --dataset-path scripts/coding_tasks/datasets/data/humaneval-cs-reworded.json`
Expected: prints `layer2_chat_pass_rate=...`; writes `results/coding-vibethinker-csharp-p1-5k-chat.json`. (~20–30 min.)

- [ ] **Step 2: Run L3 benchmark (no-think; never set CODING_BENCH_THINK)**

Run: `cd c:/Development/OllamaBenchmarks && python scripts/benchmark_coding_layer3.py --models vibethinker-csharp-p1-5k --output results/coding-layer3-vibethinker-csharp-p1-5k.json`
Expected: prints `layer3_weighted_score=...`; writes `results/coding-vibethinker-csharp-p1-5k.json`. (~30–45 min.)

- [ ] **Step 3: Compare to baseline and evaluate the gate**

Run:
```bash
cd c:/Development/OllamaBenchmarks && python -c "
import json
chat=json.load(open('results/coding-vibethinker-csharp-p1-5k-chat.json'))
def passes(d):
    r=d.get('layer2_results') or d.get('results') or []
    return sum(1 for t in r if t.get('passed')), len(r)
p,t=passes(chat)
print(f'L2 chat: {p}/{t}  (baseline 25/158, gate >=40)')
print('GATE PASS' if p>=40 else 'GATE MISS — inspect holdout / filtering before scaling')
"
```
Expected: prints the L2-chat score and a `GATE PASS`/`GATE MISS` verdict. This is the decision point from the spec: PASS → proceed to scale (follow-up plan); MISS → revisit filtering.

- [ ] **Step 4: Record the result in benchmark-models.json**

Add a `backend_notes` entry keyed `vibethinker-csharp-p1-5k` summarising: base = VibeThinker-3B + 5k Stack v1 C# LoRA (phase 1), the L2-chat and L3 scores, throughput if measured, the gate verdict, and a pointer to this plan. Add `vibethinker-csharp-p1-5k` to `benchmark_suite` and `local_installed`. Update `updated_at`.

- [ ] **Step 5: Commit results**

```bash
cd c:/Development/OllamaBenchmarks && git add results/coding-vibethinker-csharp-p1-5k-chat.json results/coding-vibethinker-csharp-p1-5k.json results/coding-layer3-vibethinker-csharp-p1-5k.json benchmark-models.json
git commit -m "feat: vibethinker-csharp-p1-5k proof-of-life benchmarks vs baseline"
```

---

## Follow-up (out of scope for this plan)

Gated on Task 8's verdict:
- **Gate PASS** → scale Stream A to ~50k (T5500 build + train), then Phase 2 (Streams B + C, think+code traces, `--resume-from` the phase-1 adapter). New plan.
- **Gate MISS** → diagnose with the hold-out set: inspect filter quality, function-extraction noise, and chat-template correctness before scaling. Adjust Task 2–5 filters and re-run.

## Self-Review Notes

- **Spec coverage:** Stream A (Tasks 2–5, 7), trainer adaptation (Task 6), iteration loop + gate (Task 8), naming/slug (Global Constraints, Task 7–8), hold-out (Task 4), think-flag quirk (Global Constraints, Task 8 Step 2). Streams B/C and Phase 2 are explicitly deferred to the follow-up plan per the spec's decision-point structure.
- **Baselines anchored:** L2 chat 25/158, L3 0/50, gate ≥40/158 — consistent across header, constraints, and Task 8.
- **Type consistency:** `to_chat_example` → `{"messages":[...]}` consumed identically by `content_key`, `dedup`, `split_holdout`, and the trainer's `formatting_func`. Helper names (`passes_license`, `passes_size`, `is_modern_csharp`, `extract_functions`, `content_key`, `dedup`, `split_holdout`) match between definitions and tests.
