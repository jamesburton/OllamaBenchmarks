"""
Task runner core for the coding benchmark.

Central engine that sends prompts to Ollama, writes generated code into
.NET projects, compiles, tests, and scores results.
"""

import dataclasses
import json
import os
import re
import shutil
import subprocess
import tempfile
import time
import urllib.request

import yaml

from coding_tasks.code_extractor import extract_csharp


@dataclasses.dataclass
class TaskResult:
    task: str
    category: str
    weight: int
    passed: bool
    harness_error: str | None
    build_success: bool
    tests_passed: int
    tests_total: int
    generation_time_s: float
    generated_code_path: str | None


def model_slug(model: str) -> str:
    """Same convention as benchmark_quality.py."""
    model = re.sub(r":latest$", "", model)
    return re.sub(
        r"[^\w\.-]",
        "_",
        model.replace(":", "_").replace("/", "_").replace("\\", "_"),
    )


def sampling_options(model: str) -> dict:
    """Return temperature/top_p for the model family.

    Nemotron models get temperature=1.0, top_p=1.0; all others get
    temperature=0, top_p=1.
    """
    if model.startswith(("nemotron-3-super", "nemotron-3-nano")):
        return {"temperature": 1.0, "top_p": 1.0}
    return {"temperature": 0, "top_p": 1}


def load_task(yaml_path: str, references_dir: str) -> dict:
    """Parse a task YAML and inject reference content into the prompt."""
    with open(yaml_path, "r", encoding="utf-8") as fh:
        task = yaml.safe_load(fh)

    ref_texts: list[str] = []
    for ref_filename in task.get("references", []):
        ref_path = os.path.join(references_dir, ref_filename)
        with open(ref_path, "r", encoding="utf-8") as fh:
            ref_texts.append(fh.read())

    combined_refs = "\n\n".join(ref_texts)
    task["prompt"] = task["prompt"].replace("{references}", combined_refs)
    return task


def call_ollama(
    model: str,
    prompt: str,
    max_tokens: int = 4096,
    num_ctx: int = 12288,
    seed: int = 42,
    timeout: int = 600,
) -> str:
    """POST to Ollama native /api/chat endpoint. Returns generated text.

    Uses native API (not /v1/chat/completions) for reliability with long prompts.
    Returns empty string on timeout or connection error (does not crash).
    """
    options = sampling_options(model)
    payload = {
        "model": model,
        "messages": [{"role": "user", "content": prompt}],
        "stream": False,
        "options": {
            "num_predict": max_tokens,
            "num_ctx": num_ctx,
            "temperature": options["temperature"],
            "top_p": options["top_p"],
            "seed": seed,
        },
    }
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
        # Strip thinking tags if present (some models use <think>...</think>)
        if "<think>" in content:
            content = re.sub(r"<think>.*?</think>", "", content, flags=re.S).strip()
        return content
    except (urllib.error.URLError, OSError, TimeoutError, KeyError, IndexError) as exc:
        print(f"    [call_ollama] Error: {type(exc).__name__}: {exc}")
        return ""


def setup_template_cache(template_dir: str, cache_dir: str) -> str:
    """Copy template to cache_dir and run dotnet restore once.

    Idempotent — skips if cache_dir already exists.  Returns cache_dir.
    """
    if os.path.isdir(cache_dir):
        return cache_dir

    shutil.copytree(template_dir, cache_dir)
    subprocess.run(
        ["dotnet", "restore"],
        cwd=cache_dir,
        timeout=120,
        capture_output=True,
    )
    return cache_dir


def run_dotnet_task(
    generated_code: str,
    test_code: str,
    cached_template_dir: str,
    work_dir: str | None = None,
) -> tuple[bool, bool, int, int, str]:
    """Compile and test generated code inside a .NET project copy.

    Returns (build_success, all_passed, tests_passed, tests_total, output).
    """
    if work_dir is None:
        work_dir = tempfile.mkdtemp(prefix="coding_bench_")

    # Copy pre-restored template (including obj/) to work_dir
    if os.path.isdir(work_dir):
        shutil.rmtree(work_dir)
    shutil.copytree(cached_template_dir, work_dir)

    # Write generated code and tests
    with open(os.path.join(work_dir, "Generated.cs"), "w", encoding="utf-8") as fh:
        fh.write(generated_code)
    with open(os.path.join(work_dir, "Tests.cs"), "w", encoding="utf-8") as fh:
        fh.write(test_code)

    # Build
    build = subprocess.run(
        ["dotnet", "build", "--no-restore"],
        cwd=work_dir,
        capture_output=True,
        text=True,
        timeout=60,
    )
    if build.returncode != 0:
        return (False, False, 0, 0, build.stderr or build.stdout)

    # Test
    test = subprocess.run(
        ["dotnet", "test", "--no-restore", "--no-build"],
        cwd=work_dir,
        capture_output=True,
        text=True,
        timeout=60,
    )
    output = test.stdout + "\n" + test.stderr

    # Parse test results from stdout
    tests_passed, tests_total = _parse_test_counts(output)
    all_passed = test.returncode == 0

    # If parsing found no counts but exit code was 0, treat as 1/1
    if tests_total == 0 and all_passed:
        tests_passed = 1
        tests_total = 1

    return (True, all_passed, tests_passed, tests_total, output)


def _parse_test_counts(output: str) -> tuple[int, int]:
    """Extract passed/total counts from dotnet test output."""
    # dotnet test outputs lines like:
    #   Passed!  - Failed:     0, Passed:     3, Skipped:     0, Total:     3
    #   Failed!  - Failed:     1, Passed:     2, Skipped:     0, Total:     3
    match = re.search(
        r"Failed:\s*(\d+),\s*Passed:\s*(\d+),\s*Skipped:\s*\d+,\s*Total:\s*(\d+)",
        output,
    )
    if match:
        passed = int(match.group(2))
        total = int(match.group(3))
        return (passed, total)
    return (0, 0)


def run_task(
    task_def: dict,
    model: str,
    cached_template_dir: str,
    output_dir: str,
    save_code: bool = True,
) -> TaskResult:
    """Full orchestration: prompt -> Ollama -> extract -> compile -> test -> score."""
    task_name = task_def["name"]
    category = task_def.get("category", "unknown")
    weight = task_def.get("weight", 1)
    max_tokens = task_def.get("max_tokens", 4096)
    num_ctx = task_def.get("num_ctx", 12288)

    try:
        # Determine generation timeout
        gen_timeout = 300 if weight >= 2 else 120

        # Call Ollama and measure time
        t0 = time.monotonic()
        raw_response = call_ollama(
            model,
            task_def["prompt"],
            max_tokens=max_tokens,
            num_ctx=num_ctx,
            timeout=gen_timeout,
        )
        generation_time = time.monotonic() - t0

        # Extract C# code
        generated_code = extract_csharp(raw_response)

        if not generated_code:
            return TaskResult(
                task=task_name,
                category=category,
                weight=weight,
                passed=False,
                harness_error="Empty code after extraction",
                build_success=False,
                tests_passed=0,
                tests_total=0,
                generation_time_s=round(generation_time, 2),
                generated_code_path=None,
            )

        # Run dotnet build + test
        work_dir = tempfile.mkdtemp(prefix=f"coding_{task_name}_")
        try:
            build_ok, all_passed, t_passed, t_total, output = run_dotnet_task(
                generated_code,
                task_def.get("test_code", ""),
                cached_template_dir,
                work_dir,
            )
        finally:
            shutil.rmtree(work_dir, ignore_errors=True)

        # Optionally save generated code as sidecar file
        code_path = None
        if save_code:
            slug = model_slug(model)
            code_dir = os.path.join(output_dir, "coding-generated", slug)
            os.makedirs(code_dir, exist_ok=True)
            code_path = os.path.join(code_dir, f"{task_name}.cs")
            with open(code_path, "w", encoding="utf-8") as fh:
                fh.write(generated_code)

        return TaskResult(
            task=task_name,
            category=category,
            weight=weight,
            passed=all_passed,
            harness_error=None,
            build_success=build_ok,
            tests_passed=t_passed,
            tests_total=t_total,
            generation_time_s=round(generation_time, 2),
            generated_code_path=code_path,
        )

    except Exception as exc:
        return TaskResult(
            task=task_name,
            category=category,
            weight=weight,
            passed=False,
            harness_error=str(exc),
            build_success=False,
            tests_passed=0,
            tests_total=0,
            generation_time_s=0.0,
            generated_code_path=None,
        )
