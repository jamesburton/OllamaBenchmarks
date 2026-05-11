#!/usr/bin/env python3
"""Layer 2 chat-mode variant — uses /api/chat (raw=false) instead of /api/generate raw=true.

For chat-tuned models that don't handle raw FIM completion well (gemma4, etc.).
Writes results to coding-{slug}-chat.json so the raw-mode baseline is preserved.

Run with the same dataset as benchmark_coding_layer2.py but the call goes through
chat completion with the model's normal template applied.
"""
import argparse
import datetime
import json
import os
import re
import subprocess
import sys
import tempfile
from typing import Any
import urllib.error
import urllib.request

sys.path.insert(0, os.path.dirname(__file__))
from coding_tasks.task_runner import (
    model_slug as base_model_slug,
    sampling_options,
    setup_template_cache,
)
from coding_tasks.code_extractor import extract_csharp


def model_slug(model: str) -> str:
    return base_model_slug(model)


def chat_complete(
    model: str,
    prompt: str,
    max_tokens: int = 4096,
    num_ctx: int = 8192,
    seed: int = 42,
    timeout: int = 600,
) -> str:
    """Call /api/chat (chat-mode, template applied) and return content.

    Notes on think handling for known-difficult models:
    - gpt-oss family: pass reasoning_effort='low' to keep thinking compact.
    - models that ignore think:false (gpt-oss, lfm2.5-thinking, glm-4.7-flash-reap):
      we still send think:False so Ollama puts reasoning in the separate field
      (not in content), reducing tag-soup; extractor strips remaining <think>.
    - num_predict bumped to 4096 so reasoning + code fits even for verbose thinkers.
    """
    options = sampling_options(model)
    payload: dict = {
        "model": model,
        "messages": [{"role": "user", "content": prompt}],
        "stream": False,
        "think": False,
        "options": {
            "num_predict": max_tokens,
            "num_ctx": num_ctx,
            "temperature": options["temperature"],
            "top_p": options["top_p"],
            "seed": seed,
        },
    }
    if model.startswith("gpt-oss"):
        payload["options"]["reasoning_effort"] = "low"
    data = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(
        "http://127.0.0.1:11434/api/chat",
        data=data,
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=timeout) as resp:
            body = json.loads(resp.read().decode("utf-8"))
        content = body.get("message", {}).get("content", "")
        if "<think>" in content:
            content = re.sub(r"<think>.*?</think>", "", content, flags=re.S).strip()
        return content
    except (urllib.error.URLError, OSError, TimeoutError) as exc:
        print(f"    [chat] Error: {type(exc).__name__}: {exc}")
        return ""


def build_program_cs(prompt: str, generated_code: str, tests: str) -> str:
    """Same as benchmark_coding_layer2._build_program_cs."""
    USINGS = (
        "using System;\nusing System.Collections.Generic;\nusing System.Linq;\n"
        "using System.Text;\nusing System.Text.RegularExpressions;\n"
        "using System.Diagnostics;\n"
    )
    generated_code = generated_code.replace("\r\n", "\n").strip()
    prompt = prompt.replace("\r\n", "\n").strip()
    tests = tests.replace("\r\n", "\n").strip()

    if "class Problem" in generated_code:
        # Code already contains class structure
        body = generated_code
    else:
        # Wrap generated code as method bodies inside Problem class
        body = (
            "class Problem {\n"
            "    public static void Main(string[] args) {\n"
            "        " + tests + "\n"
            "    }\n"
            "    " + generated_code + "\n"
            "}\n"
        )
        return USINGS + "\n" + body

    # Tests run via Main if not already present
    return USINGS + "\n" + body


def run_problem(problem: dict, model: str, cached_template: str) -> tuple[bool, str]:
    """Run one problem in chat mode. Returns (passed, error_str)."""
    prompt = problem.get("prompt", "")
    tests = problem.get("tests", "")

    # Ask the model to complete the C# function via chat
    chat_prompt = (
        "Complete the following C# code. Provide ONLY the function implementation. "
        "Do not include the function signature or docstring — start with the function body content. "
        "Wrap your code in a ```csharp code block.\n\n"
        f"{prompt}"
    )
    raw = chat_complete(model, chat_prompt)
    if not raw:
        return False, "empty model response"

    extracted = extract_csharp(raw) or raw.strip()

    # If extracted lacks function signature, prepend the prompt's signature
    if "public static" not in extracted and "static " not in extracted:
        body = extracted
        extracted = prompt.rstrip() + "\n" + body + "\n}"

    work_dir = tempfile.mkdtemp(prefix="layer2_chat_")
    try:
        # Copy cached template
        import shutil
        shutil.rmtree(work_dir)
        shutil.copytree(cached_template, work_dir)

        program_cs = build_program_cs(prompt, extracted, tests)
        with open(os.path.join(work_dir, "Program.cs"), "w", encoding="utf-8") as fh:
            fh.write(program_cs)

        build = subprocess.run(
            ["dotnet", "build", "--no-restore"],
            cwd=work_dir,
            capture_output=True,
            text=True,
            timeout=60,
        )
        if build.returncode != 0:
            return False, "The build failed. Fix the build errors and run again."

        run = subprocess.run(
            ["dotnet", "run", "--no-build", "--no-restore"],
            cwd=work_dir,
            capture_output=True,
            text=True,
            timeout=60,
        )
        if run.returncode != 0:
            err = (run.stdout + "\n" + run.stderr).strip()
            return False, err.splitlines()[0] if err else "Process terminated."
        return True, ""
    finally:
        import shutil
        try:
            shutil.rmtree(work_dir)
        except Exception:
            pass


def main():
    parser = argparse.ArgumentParser(description="Layer 2 chat-mode variant.")
    parser.add_argument("--models", nargs="+", required=True)
    parser.add_argument("--checkpoint-dir", default="results")
    parser.add_argument("--dataset-path", required=True)
    parser.add_argument("--template-base", default="scripts/coding_tasks/templates")
    parser.add_argument("--limit", type=int, default=0)
    args = parser.parse_args()

    print(f"[setup] Loading dataset: {args.dataset_path}")
    from benchmark_coding_layer2 import load_dataset
    problems = load_dataset(args.dataset_path)
    if args.limit > 0:
        problems = problems[: args.limit]
    total = len(problems)
    print(f"[setup] {total} problems")

    template_dir = os.path.join(args.template_base, "layer2_project")
    cache_dir = os.path.join(args.template_base, ".cache", "layer2_project")
    cached_template = setup_template_cache(template_dir, cache_dir)
    print(f"[setup] Template cache ready: {cached_template}")

    for model in args.models:
        slug = model_slug(model)
        print(f"\n[model] {model} (slug={slug}-chat)")
        started = datetime.datetime.now(datetime.timezone.utc)
        passed = 0
        records = []
        for i, prob in enumerate(problems, 1):
            name = prob.get("name", f"prob_{i}")
            print(f"  [{i}/{total}] {name} ...", end="", flush=True)
            try:
                ok, err = run_problem(prob, model, cached_template)
            except Exception as exc:
                ok = False; err = str(exc)
            if ok:
                passed += 1
                print(" PASS")
            else:
                first = err.splitlines()[0] if err else "unknown"
                print(f" FAIL  ({first[:80]})")
            records.append({"name": name, "passed": ok, "error": err})
        finished = datetime.datetime.now(datetime.timezone.utc)
        rate = passed / total if total else 0
        print(f"  [score] layer2_chat_pass_rate={rate:.4f} ({passed}/{total})")

        # Write to coding-{slug}-chat.json (don't overwrite raw baseline)
        cp_path = os.path.join(args.checkpoint_dir, f"coding-{slug}-chat.json")
        payload = {
            "model": model,
            "benchmark": "coding-layer2-chat",
            "layer2_chat_run_started_at": started.isoformat(),
            "layer2_chat_run_finished_at": finished.isoformat(),
            "layer2_chat_pass_rate": rate,
            "layer2_chat_passed": passed,
            "layer2_chat_total": total,
            "layer2_chat_results": records,
        }
        with open(cp_path, "w", encoding="utf-8") as fh:
            fh.write(json.dumps(payload, indent=2) + "\n")
        print(f"  [checkpoint] Written to {cp_path}")

    print("\n[done] Layer 2 chat-mode complete.")


if __name__ == "__main__":
    main()
