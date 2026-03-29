#!/usr/bin/env python3
"""Generate JSONL training data for IQuest-Coder-V1-7B LoRA fine-tuning.

Reads task YAMLs, resolves {references} placeholders, pairs them with
high-quality C# outputs from top-performing models to create chat-format
training examples.

Usage:
    python scripts/lora/generate_training_data.py
"""

import json
import os
import sys
import yaml
from pathlib import Path

# ── Paths ──────────────────────────────────────────────────────────────
REPO_ROOT = Path(__file__).resolve().parent.parent.parent
TASKS_DIR = REPO_ROOT / "scripts" / "coding_tasks" / "tasks"
REFERENCES_DIR = REPO_ROOT / "scripts" / "coding_tasks" / "references"
GENERATED_DIR = REPO_ROOT / "results" / "coding-generated"
OUTPUT_FILE = Path(__file__).resolve().parent / "training_data.jsonl"

# Models to source completions from, ordered by quality (best first).
# qwen3-coder-next is the primary reference; others augment the dataset.
SOURCE_MODELS = [
    "qwen3-coder-next",
    "RogerBen_qwen3.5-35b-opus-distill",
    "gpt-oss_120b",
    "mistral-small",
]

# System prompt that teaches clean C# output style
SYSTEM_PROMPT = (
    "You are an expert C#/.NET developer. When asked to write code, "
    "return ONLY valid C# code in a single file. Do not include markdown "
    "fences, explanations, or commentary — just the raw C# source code."
)


def load_task_yaml(yaml_path: Path) -> dict:
    """Parse a task YAML file."""
    with open(yaml_path, "r", encoding="utf-8") as fh:
        return yaml.safe_load(fh)


def resolve_references(task: dict) -> str:
    """Build the full prompt with {references} replaced by reference content."""
    prompt = task.get("prompt", "")
    ref_texts = []
    for ref_filename in task.get("references", []):
        ref_path = REFERENCES_DIR / ref_filename
        if ref_path.exists():
            ref_texts.append(ref_path.read_text(encoding="utf-8"))
        else:
            print(f"  WARNING: Reference file not found: {ref_path}", file=sys.stderr)
    combined = "\n\n".join(ref_texts)
    return prompt.replace("{references}", combined)


def read_generated_code(model_dir: str, task_name: str) -> str | None:
    """Read a .cs file from a model's generated output directory.

    Returns None if the file doesn't exist or is empty.
    """
    cs_path = GENERATED_DIR / model_dir / f"{task_name}.cs"
    if not cs_path.exists():
        return None
    text = cs_path.read_text(encoding="utf-8").strip()
    return text if text else None


def strip_markdown_fences(code: str) -> str:
    """Remove ```csharp / ``` wrappers if present."""
    lines = code.strip().splitlines()
    if lines and lines[0].strip().startswith("```"):
        lines = lines[1:]
    if lines and lines[-1].strip() == "```":
        lines = lines[:-1]
    return "\n".join(lines).strip()


def make_chat_example(prompt: str, code: str) -> dict:
    """Create a chat-format training example."""
    return {
        "messages": [
            {"role": "system", "content": SYSTEM_PROMPT},
            {"role": "user", "content": prompt.strip()},
            {"role": "assistant", "content": code.strip()},
        ]
    }


def main():
    # Discover task YAML files (skip _smoke_test)
    yaml_files = sorted(
        p for p in TASKS_DIR.glob("[0-9]*.yaml")
        if not p.stem.startswith("_")
    )
    print(f"Found {len(yaml_files)} task definitions")

    # Discover which model directories actually exist
    available_models = []
    for model in SOURCE_MODELS:
        model_path = GENERATED_DIR / model
        if model_path.is_dir():
            available_models.append(model)
            cs_count = len(list(model_path.glob("*.cs")))
            print(f"  Source model: {model} ({cs_count} .cs files)")
        else:
            print(f"  Source model: {model} — NOT FOUND, skipping")

    if not available_models:
        print("ERROR: No source model directories found!", file=sys.stderr)
        sys.exit(1)

    # Generate training pairs
    seen = set()  # (task_name, model) dedup
    examples = []
    skipped = 0

    for yaml_path in yaml_files:
        task = load_task_yaml(yaml_path)
        task_name = task.get("name", yaml_path.stem)
        prompt = resolve_references(task)

        for model in available_models:
            key = (task_name, model)
            if key in seen:
                continue
            seen.add(key)

            code = read_generated_code(model, task_name)
            if code is None:
                skipped += 1
                continue

            code = strip_markdown_fences(code)

            # Basic quality filter: skip very short outputs (likely failures)
            if len(code) < 50:
                skipped += 1
                continue

            examples.append(make_chat_example(prompt, code))

    # Write JSONL
    OUTPUT_FILE.parent.mkdir(parents=True, exist_ok=True)
    with open(OUTPUT_FILE, "w", encoding="utf-8") as fh:
        for ex in examples:
            fh.write(json.dumps(ex, ensure_ascii=False) + "\n")

    print(f"\nGenerated {len(examples)} training examples")
    print(f"Skipped {skipped} missing/empty outputs")
    print(f"Output: {OUTPUT_FILE}")

    # Summary by model
    from collections import Counter
    model_counts = Counter()
    for (task_name, model) in seen:
        code = read_generated_code(model, task_name)
        if code and len(strip_markdown_fences(code)) >= 50:
            model_counts[model] += 1
    print("\nExamples per source model:")
    for model, count in model_counts.most_common():
        print(f"  {model}: {count}")


if __name__ == "__main__":
    main()
