"""Layer 2 harness for the MultiPL-E C# coding benchmark.

Evaluates models on HumanEval problems translated to C# via the MultiPL-E
dataset.  For each problem the model completes a function signature, the
generated code is combined with the dataset's test assertions, compiled with
dotnet, run, and scored pass@1.
"""

import argparse
import datetime
import json
import os
import shutil
import subprocess
import sys
import tempfile
from typing import Any

sys.path.insert(0, os.path.dirname(__file__))
from coding_tasks.task_runner import (
    model_slug,
    call_ollama,
    setup_template_cache,
)
from coding_tasks.code_extractor import extract_csharp
from coding_tasks.task_runner import sampling_options


# ---------------------------------------------------------------------------
# Completion-mode generation (fill-in-the-middle for MultiPL-E)
# ---------------------------------------------------------------------------

def _call_ollama_complete(
    model: str,
    prompt: str,
    max_tokens: int = 2048,
    num_ctx: int = 4096,
    seed: int = 42,
    timeout: int = 120,
    stop_tokens: list[str] | None = None,
) -> str:
    """Use /api/generate (completion mode) for fill-in-the-middle generation.

    Unlike call_ollama (chat mode), this sends the prompt as a raw prefix and
    the model continues from where the prompt left off, without repeating it.
    """
    import re
    import urllib.request

    options = sampling_options(model)
    payload: dict = {
        "model": model,
        "prompt": prompt,
        "stream": False,
        "raw": True,
        "options": {
            "num_predict": max_tokens,
            "num_ctx": num_ctx,
            "temperature": options["temperature"],
            "top_p": options["top_p"],
            "seed": seed,
        },
    }
    if stop_tokens:
        payload["options"]["stop"] = stop_tokens

    data = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(
        "http://127.0.0.1:11434/api/generate",
        data=data,
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=timeout) as resp:
            body = json.loads(resp.read().decode("utf-8"))
        content = body.get("response", "")
        # Strip thinking tags if present
        if "<think>" in content:
            content = re.sub(r"<think>.*?</think>", "", content, flags=re.S).strip()
        return content
    except (urllib.error.URLError, OSError, TimeoutError) as exc:
        print(f"    [complete] Error: {type(exc).__name__}: {exc}")
        return ""


# ---------------------------------------------------------------------------
# Dataset loading
# ---------------------------------------------------------------------------

def load_dataset(path: str) -> list[dict]:
    """Load a MultiPL-E C# dataset from a JSON or JSONL file.

    Supports:
    - JSON file containing a list of problem objects
    - JSON file containing a dict with a top-level list value
    - JSONL file (one JSON object per line)

    Each problem object is expected to have at least:
        "name"   - problem identifier
        "prompt" - C# function signature / docstring to complete
        "tests"  - C# test assertions
    """
    if not os.path.isfile(path):
        print(f"[error] Dataset file not found: {path}", file=sys.stderr)
        sys.exit(1)

    with open(path, "r", encoding="utf-8") as fh:
        content = fh.read().strip()

    # Try JSONL first (multiple JSON objects, one per line)
    if "\n" in content:
        first_line = content.splitlines()[0].strip()
        if first_line.startswith("{"):
            problems = []
            for line in content.splitlines():
                line = line.strip()
                if line:
                    problems.append(json.loads(line))
            return problems

    # Single JSON document
    data = json.loads(content)
    if isinstance(data, list):
        return data
    if isinstance(data, dict):
        # Find the first list value
        for v in data.values():
            if isinstance(v, list):
                return v
        # Treat the dict itself as a single problem
        return [data]

    print(f"[error] Unexpected dataset format in {path}", file=sys.stderr)
    sys.exit(1)


# ---------------------------------------------------------------------------
# Per-problem runner
# ---------------------------------------------------------------------------

def run_problem(
    problem: dict,
    model: str,
    cached_template_dir: str,
) -> tuple[bool, str]:
    """Run a single MultiPL-E problem.

    Returns (passed, error_message).  error_message is empty on success.
    """
    prompt: str = problem.get("prompt", "")
    tests: str = problem.get("tests", "")
    stop_tokens: list[str] = problem.get("stop_tokens") or ["\n    }\n"]

    # 1. Generate code using completion mode (fill-in-the-middle)
    raw_response = _call_ollama_complete(
        model,
        prompt,
        max_tokens=2048,
        num_ctx=4096,
        seed=42,
        timeout=120,
        stop_tokens=stop_tokens,
    )

    if not raw_response:
        return False, "Ollama returned empty response"

    # For completion mode, the response IS the function body — no extraction needed.
    # Only strip markdown fences if the model wrapped it.
    generated = raw_response
    if "```" in generated:
        generated = extract_csharp(generated) or raw_response.strip()

    # 2. Build Program.cs: prompt + completion + tests (fill-in-the-middle)
    program_cs = _build_program_cs(prompt, generated, tests)

    # 3. Copy template to a temp dir
    work_dir = tempfile.mkdtemp(prefix="layer2_bench_")
    try:
        shutil.rmtree(work_dir)
        shutil.copytree(cached_template_dir, work_dir)

        program_path = os.path.join(work_dir, "Program.cs")
        with open(program_path, "w", encoding="utf-8") as fh:
            fh.write(program_cs)

        # 4. Run dotnet run --no-restore
        try:
            result = subprocess.run(
                ["dotnet", "run", "--no-restore"],
                cwd=work_dir,
                capture_output=True,
                text=True,
                timeout=30,
            )
        except subprocess.TimeoutExpired:
            return False, "dotnet run timed out (30 s)"

        stdout = result.stdout or ""
        stderr = result.stderr or ""

        # 5. Pass if exit code 0 AND stdout contains "PASS"
        if result.returncode == 0 and "PASS" in stdout:
            return True, ""

        # Build a concise error for debugging
        error_lines = (stderr or stdout).strip().splitlines()
        brief = "\n".join(error_lines[:10])
        return False, brief

    finally:
        shutil.rmtree(work_dir, ignore_errors=True)


def _build_program_cs(prompt: str, generated_code: str, tests: str) -> str:
    """Assemble a standalone Program.cs from generated code + test assertions.

    MultiPL-E datasets use a fill-in-the-middle format where:
    - ``prompt`` is the start of the C# file (usings, class, method signature)
    - the model generates the function body (the completion)
    - ``tests`` closes the method and provides Main with Debug.Assert calls

    Strategy:
    1. If the generated code already contains the class/using structure from the
       prompt, treat it as a standalone file and append tests via _wrap_tests_in_main.
    2. If tests already contain a Main method (MultiPL-E native format), use the
       fill-in-the-middle assembly: prompt + completion + tests + PASS sentinel.
    3. Otherwise fall back to wrapping generated code with tests.
    """
    # Normalise line endings
    generated_code = generated_code.replace("\r\n", "\n").strip()
    tests = tests.replace("\r\n", "\n").strip()
    prompt = prompt.replace("\r\n", "\n").rstrip()

    # Detect if tests already contain a Main entry point (MultiPL-E native format)
    tests_have_main = "static void Main" in tests or "public static void Main" in tests

    # Check whether the generated code already has the class/using wrapper
    has_class = any(
        tok in generated_code
        for tok in ("class ", "namespace ", "using ")
    )

    if tests_have_main and not has_class:
        # MultiPL-E fill-in-the-middle: prompt + completion + tests
        # The tests already close Main(} and class(}).
        # Inject PASS sentinel just before the final two closing braces.
        tests_with_pass = _inject_pass_sentinel(tests)
        program = (
            prompt
            + "\n"
            + generated_code
            + "\n"
            + tests_with_pass
        )
    elif has_class:
        if tests_have_main:
            # Model repeated the full structure; concatenate with PASS
            tests_with_pass = _inject_pass_sentinel(tests)
            program = (
                generated_code
                + "\n"
                + tests_with_pass
            )
        else:
            # Standalone generated code + separate test assertions
            program = (
                generated_code
                + "\n\n"
                + _wrap_tests_in_main(tests)
            )
    else:
        # Wrap everything: generated code is the function body, tests run inline
        program = (
            "// Auto-generated by Layer 2 harness\n"
            + generated_code
            + "\n\n"
            + tests
            + '\n\nConsole.WriteLine("PASS");\n'
        )

    return program


def _inject_pass_sentinel(tests: str) -> str:
    """Inject a PASS print statement before the final closing braces in MultiPL-E tests.

    MultiPL-E tests end with ``}\\n\\n}\\n`` (closing Main, then closing class).
    We insert a Console.WriteLine("PASS") just before the Main closing brace.
    """
    # Find the last assertion line and insert PASS after it
    lines = tests.rstrip().splitlines()
    # Walk backwards to find the pattern: '}' (close Main), '', '}' (close class)
    insert_idx = len(lines)
    for i in range(len(lines) - 1, -1, -1):
        if lines[i].strip().startswith("Debug.Assert") or lines[i].strip().startswith("//"):
            insert_idx = i + 1
            break
    lines.insert(insert_idx, '    System.Console.WriteLine("PASS");')
    return "\n".join(lines) + "\n"


def _wrap_tests_in_main(tests: str) -> str:
    """Wrap test statements in a static void Main entry point."""
    indented = "\n".join("    " + line for line in tests.splitlines())
    return (
        "class _Layer2Runner\n"
        "{\n"
        "    static void Main()\n"
        "    {\n"
        + indented
        + "\n"
        "        System.Console.WriteLine(\"PASS\");\n"
        "    }\n"
        "}\n"
    )


# ---------------------------------------------------------------------------
# Checkpoint helpers
# ---------------------------------------------------------------------------

def read_checkpoint(path: str) -> dict[str, Any]:
    if os.path.isfile(path):
        with open(path, "r", encoding="utf-8") as fh:
            return json.load(fh)
    return {}


def write_checkpoint(path: str, payload: dict[str, Any]) -> None:
    os.makedirs(os.path.dirname(path) or ".", exist_ok=True)
    with open(path, "w", encoding="utf-8") as fh:
        fh.write(json.dumps(payload, indent=2) + "\n")


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main() -> None:
    parser = argparse.ArgumentParser(
        description="Layer 2: MultiPL-E C# harness for Ollama coding benchmark."
    )
    parser.add_argument("--models", nargs="+", required=True,
                        help="Ollama model names to evaluate")
    parser.add_argument("--checkpoint-dir", default="results",
                        help="Directory for per-model checkpoint JSON files")
    parser.add_argument("--dataset-path", required=True,
                        help="Path to the MultiPL-E C# JSON or JSONL dataset file")
    parser.add_argument("--template-base", default="scripts/coding_tasks/templates",
                        help="Base directory containing .NET project templates")
    parser.add_argument("--limit", type=int, default=0,
                        help="Limit number of problems (0 = all)")
    args = parser.parse_args()

    # --- Load dataset once ---
    print(f"[setup] Loading dataset: {args.dataset_path}")
    problems = load_dataset(args.dataset_path)
    if args.limit > 0:
        problems = problems[: args.limit]
    total = len(problems)
    print(f"[setup] {total} problem(s) loaded")

    if total == 0:
        print("[error] No problems found in dataset.", file=sys.stderr)
        sys.exit(1)

    # --- Pre-restore layer2_project template once ---
    template_dir = os.path.join(args.template_base, "layer2_project")
    cache_dir = os.path.join(args.template_base, ".cache", "layer2_project")
    print(f"[setup] Restoring template cache: layer2_project -> {cache_dir}")
    cached_template = setup_template_cache(template_dir, cache_dir)

    # --- Per-model loop ---
    for model in args.models:
        slug = model_slug(model)
        print(f"\n[model] {model} (slug={slug})")

        model_run_started_at = datetime.datetime.now(datetime.timezone.utc)
        passed_count = 0
        problem_records: list[dict[str, Any]] = []

        for idx, problem in enumerate(problems, start=1):
            name = problem.get("name", f"problem_{idx}")
            print(f"  [{idx}/{total}] {name} ...", end="", flush=True)

            try:
                passed, error = run_problem(problem, model, cached_template)
            except Exception as exc:
                passed = False
                error = str(exc)

            if passed:
                passed_count += 1
                print(" PASS")
            else:
                brief = error.splitlines()[0] if error else "unknown error"
                print(f" FAIL  ({brief})")

            problem_records.append({
                "name": name,
                "passed": passed,
                "error": error,
            })

        model_run_finished_at = datetime.datetime.now(datetime.timezone.utc)
        pass_rate = passed_count / total

        print(
            f"  [score] layer2_pass_rate={pass_rate:.4f} "
            f"({passed_count}/{total} problems passed)"
        )

        # --- Checkpoint: merge layer2 result into existing file if present ---
        checkpoint_path = os.path.join(args.checkpoint_dir, f"coding-{slug}.json")
        checkpoint = read_checkpoint(checkpoint_path)

        # Preserve existing data, overlay layer2 fields
        checkpoint.update({
            "model": model,
            "benchmark": "coding",
            "layer2_run_started_at": model_run_started_at.isoformat(),
            "layer2_run_finished_at": model_run_finished_at.isoformat(),
            "layer2_pass_rate": pass_rate,
            "layer2_passed": passed_count,
            "layer2_total": total,
            "layer2_results": problem_records,
        })

        write_checkpoint(checkpoint_path, checkpoint)
        print(f"  [checkpoint] Written to {checkpoint_path}")

    print("\n[done] Layer 2 benchmark complete.")


if __name__ == "__main__":
    main()
