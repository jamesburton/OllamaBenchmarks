#!/usr/bin/env python3
"""Layer 2 chat-mode variant — reuses benchmark_coding_layer2's FIM assembly
but swaps the model call from /api/generate (raw:true) to /api/chat (template applied).

For chat-tuned models that don't handle raw FIM completion well (gemma4, gpt-oss,
glm-4.7-flash-reap, LFM2, etc.). Writes results to coding-{slug}-chat.json so the
raw-mode baseline at coding-{slug}.json is preserved.
"""
import argparse
import datetime
import json
import os
import re
import sys
from typing import Any
import urllib.error
import urllib.request

sys.path.insert(0, os.path.dirname(__file__))

# Import the proven raw-mode helpers — we only need to swap the model-call.
import benchmark_coding_layer2 as L2
from coding_tasks.code_extractor import extract_csharp
from coding_tasks.task_runner import (
    sampling_options,
    setup_template_cache,
    model_slug as base_model_slug,
)


def model_slug(model: str) -> str:
    return base_model_slug(model)


def _inject_pass(program_cs: str) -> str:
    """Insert Console.WriteLine("PASS") before the last `}` of Main inside Problem."""
    lines = program_cs.rstrip().splitlines()
    for i in range(len(lines) - 1, -1, -1):
        s = lines[i].strip()
        if s.startswith("Debug.Assert"):
            lines.insert(i + 1, '    System.Console.WriteLine("PASS");')
            return "\n".join(lines) + "\n"
    # Fallback: print before final closing brace
    for i in range(len(lines) - 1, -1, -1):
        if lines[i].strip() == "}":
            lines.insert(i, '    System.Console.WriteLine("PASS");')
            return "\n".join(lines) + "\n"
    return program_cs + '\nSystem.Console.WriteLine("PASS");\n'


def _extract_method_body_from_full_class(generated: str, method_signature_line: str) -> str | None:
    """Extract the body content between the method's opening `{` and matching `}`.

    Used when a chat-tuned model returns a complete `class Problem { method {...} }`
    instead of just the FIM body. We locate the method signature in `generated`,
    find its opening `{`, then walk forward counting braces until we hit the
    matching `}`. Returns the body content (without the surrounding braces).
    Returns None if the method or matching brace cannot be found.
    """
    # Find the method line in the generated code (match by the signature head, e.g. "public static bool HasCloseElements")
    head = method_signature_line.strip().rstrip("{").strip()
    if not head:
        return None
    idx = generated.find(head)
    if idx < 0:
        return None
    # Find the `{` that opens the method body, after idx
    brace_open = generated.find("{", idx)
    if brace_open < 0:
        return None
    # Walk forward counting braces
    depth = 1
    pos = brace_open + 1
    while pos < len(generated) and depth > 0:
        ch = generated[pos]
        if ch == "{":
            depth += 1
        elif ch == "}":
            depth -= 1
            if depth == 0:
                # body is generated[brace_open+1 : pos]
                return generated[brace_open + 1: pos]
        pos += 1
    return None


def _method_signature_from_prompt(prompt: str) -> str | None:
    """Pull the method signature line from the dataset prompt.

    The prompt ends with `<modifiers> <return-type> <Name>(...) {`. We return
    the line containing that signature so we can locate it in the model output.
    """
    for line in prompt.rstrip().splitlines()[::-1]:
        s = line.strip()
        if (
            s.endswith("{")
            and ("public " in s or "private " in s or "static " in s)
            and "(" in s
        ):
            return line
    return None


def _chat_complete(
    model: str,
    prompt: str,
    max_tokens: int = 4096,
    num_ctx: int = 8192,
    seed: int = 42,
    timeout: int | None = None,
    stop_tokens: list[str] | None = None,
) -> str:
    """Drop-in replacement for L2._call_ollama_complete that uses /api/chat.

    The dataset prompt is a fill-in-the-middle prefix (usings + class + method
    signature). For chat-tuned models, ship the prefix with an explicit
    instruction to complete the function body and return the full Program.cs in
    a code fence; extract C# code afterwards.
    """
    if timeout is None:
        # Was hardcoded to 600s. Under OLLAMA_NUM_PARALLEL>1, concurrent decode
        # streams roughly halve (or worse) each other's effective tok/s (see
        # MODEL_QUIRKS.md perf-lever entry), so multi-hundred-token completions
        # routinely blew past 600s (observed 500-response-at-exactly-600s during
        # cross-machine 2-worker testing, 2026-07-10). Override via L2_CHAT_TIMEOUT_S.
        timeout = int(os.environ.get("L2_CHAT_TIMEOUT_S", "600"))
    options = sampling_options(model)
    chat_prompt = (
        "You are completing a C# program. The code below is the start of a file. "
        "Continue and finish the function body (and any closing braces it needs). "
        "Return the COMPLETE Program.cs including the prefix I gave you, wrapped in a "
        "```csharp code block. Do not add tests or a Main method — those are appended separately.\n\n"
        "```csharp\n" + prompt.rstrip() + "\n```"
    )
    payload: dict[str, Any] = {
        "model": model,
        "messages": [{"role": "user", "content": chat_prompt}],
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

    req = urllib.request.Request(
        f'{os.environ.get("OLLAMA_HOST", "http://127.0.0.1:11434").rstrip("/")}/api/chat',
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=timeout) as resp:
            body = json.loads(resp.read().decode("utf-8"))
        content = body.get("message", {}).get("content", "") or ""
        if "<think>" in content:
            content = re.sub(r"<think>.*?</think>", "", content, flags=re.S).strip()
        return content
    except (urllib.error.URLError, OSError, TimeoutError) as exc:
        print(f"    [chat] Error: {type(exc).__name__}: {exc}")
        return ""


def run_problem_chat(problem: dict, model: str, cached_template: str) -> tuple[bool, str]:
    """Mirror of L2.run_problem but using chat-mode call + smarter extraction.

    Strategy:
    1. Ask the model to complete the program in chat mode (with code-fence instruction).
    2. Extract C# from the response.
    3. If the extracted code already has `class ` or `using ` (full file) → pass
       to L2._build_program_cs which handles the has_class branch (adds tests via
       _wrap_tests_in_main as a separate class).
    4. If extracted code is just the function body → pass to _build_program_cs
       which handles the FIM branch (prompt + body + tests).
    """
    import shutil
    import subprocess
    import tempfile

    prompt: str = problem.get("prompt", "")
    tests: str = problem.get("tests", "")

    raw_response = _chat_complete(model, prompt)
    if not raw_response:
        return False, "Ollama returned empty response"

    generated = extract_csharp(raw_response) or raw_response.strip()

    # Chat-tuned models typically return a complete C# file with the full
    # `class Problem { method {...} }` re-emitted from the prompt prefix.
    # The dataset's `tests` block is FIM scaffolding designed to slot inside
    # the class: it starts with `    }` (close method) then declares Main, and
    # ends with `}` (close class). To make the existing FIM assembly work, we
    # need just the method body from the model's output.
    body = None
    if "public static void Main" not in generated:
        sig = _method_signature_from_prompt(prompt)
        if sig:
            # Handles both a full `class Problem { method {...} }` wrapper AND a
            # bare re-emitted method (signature + body, no class) — the latter is
            # what instruction-tuned C# models often return. _extract_method_body
            # locates the signature anywhere in the output and pulls its body, so
            # the FIM assembly gets just the statements (not a duplicate signature).
            body = _extract_method_body_from_full_class(generated, sig)
    if body is not None:
        # Use the standard FIM assembly with the extracted body
        program_cs = L2._build_program_cs(prompt, body, tests)
    else:
        # Fallback: pass whatever we extracted to the existing assembler.
        program_cs = L2._build_program_cs(prompt, generated, tests)

    work_dir = tempfile.mkdtemp(prefix="layer2_chat_")
    try:
        shutil.rmtree(work_dir)
        shutil.copytree(cached_template, work_dir)

        with open(os.path.join(work_dir, "Program.cs"), "w", encoding="utf-8") as fh:
            fh.write(program_cs)

        # Each task builds in a fresh temp dir (no incremental reuse), so the
        # per-task budget is a COLD build + run. The 30 s default is adequate on
        # an idle host; on a loaded/disk-starved box cold builds can hit 60-90 s
        # and silently turn passes into timeouts. Override via L2_RUN_TIMEOUT_S.
        run_timeout = int(os.environ.get("L2_RUN_TIMEOUT_S", "30"))
        try:
            result = subprocess.run(
                ["dotnet", "run", "--no-restore"],
                cwd=work_dir,
                capture_output=True,
                text=True,
                timeout=run_timeout,
                stdin=subprocess.DEVNULL,
            )
        except subprocess.TimeoutExpired:
            return False, f"dotnet run timed out ({run_timeout} s)"

        stdout = result.stdout or ""
        stderr = result.stderr or ""

        if result.returncode == 0 and "PASS" in stdout:
            return True, ""

        error_lines = (stderr or stdout).strip().splitlines()
        brief = "\n".join(error_lines[:10])
        return False, brief
    finally:
        shutil.rmtree(work_dir, ignore_errors=True)


def main():
    parser = argparse.ArgumentParser(description="Layer 2 chat-mode variant.")
    parser.add_argument("--models", nargs="+", required=True)
    parser.add_argument("--checkpoint-dir", default="results")
    parser.add_argument("--dataset-path", required=True)
    parser.add_argument("--template-base", default="scripts/coding_tasks/templates")
    parser.add_argument("--limit", type=int, default=0)
    args = parser.parse_args()

    print(f"[setup] Loading dataset: {args.dataset_path}")
    problems = L2.load_dataset(args.dataset_path)
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
                ok, err = run_problem_chat(prob, model, cached_template)
            except Exception as exc:
                ok = False
                err = str(exc)
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
