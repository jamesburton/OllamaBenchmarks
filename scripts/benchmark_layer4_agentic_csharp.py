"""Layer 4 — T3: agentic C# coding (read -> edit -> build -> test until green).

The headline agentic test. Each task seeds a real .NET (xunit v3) sandbox with a
stub/buggy source file and a failing test file. The model is given file-op +
build/test tools and must iterate until the tests pass or a step cap (N=10) is
hit. Score is taken from an **authoritative final `dotnet test`** on the
resulting files — never the model's own "finish" claim.

Generation runs on the GPU host via ``OLLAMA_HOST``; the dotnet build/test runs
locally (the Framework harness), reusing the L3 sandbox + the configurable
``L3_BUILD_TIMEOUT_S`` / ``L3_TEST_TIMEOUT_S`` knobs.

Output: results/layer4-agentic-{slug}.json.
"""

import argparse
import datetime
import json
import os
import shutil
import subprocess
import sys
import tempfile

sys.path.insert(0, os.path.dirname(__file__))
from coding_tasks.task_runner import model_slug, setup_template_cache, _parse_test_counts
from coding_tasks.tool_loop import Tool, run_tool_loop, OLLAMA_HOST

_MAX_TOOL_OUTPUT = 1800  # truncate build/test output fed back to the model


# ---------------------------------------------------------------------------
# Task fixtures: each seeds files (incl. a failing test) the model must fix
# ---------------------------------------------------------------------------

TASKS = [
    {
        "name": "implement_calculator",
        "instruction": (
            "Calculator.cs has Add and Multiply that throw NotImplementedException. "
            "Implement both correctly so every test in CalculatorTests.cs passes."
        ),
        "files": {
            "Calculator.cs": (
                "namespace Sandbox;\n\n"
                "public class Calculator\n{\n"
                "    public int Add(int a, int b) => throw new System.NotImplementedException();\n"
                "    public int Multiply(int a, int b) => throw new System.NotImplementedException();\n"
                "}\n"
            ),
            "CalculatorTests.cs": (
                "using Xunit;\nusing Sandbox;\n\n"
                "public class CalculatorTests\n{\n"
                "    [Fact] public void Adds() => Assert.Equal(5, new Calculator().Add(2, 3));\n"
                "    [Fact] public void Multiplies() => Assert.Equal(12, new Calculator().Multiply(3, 4));\n"
                "}\n"
            ),
        },
    },
    {
        "name": "fix_isprime_bug",
        "instruction": (
            "Numbers.IsPrime has a bug (it mishandles n < 2). Fix Numbers.cs so all "
            "tests in NumbersTests.cs pass. Do not change the tests."
        ),
        "files": {
            "Numbers.cs": (
                "namespace Sandbox;\n\n"
                "public static class Numbers\n{\n"
                "    public static bool IsPrime(int n)\n    {\n"
                "        if (n < 2) return true;  // BUG: 0 and 1 are not prime\n"
                "        for (int i = 2; i < n; i++)\n"
                "            if (n % i == 0) return false;\n"
                "        return true;\n    }\n}\n"
            ),
            "NumbersTests.cs": (
                "using Xunit;\nusing Sandbox;\n\n"
                "public class NumbersTests\n{\n"
                "    [Theory]\n"
                "    [InlineData(0, false)] [InlineData(1, false)] [InlineData(2, true)]\n"
                "    [InlineData(4, false)] [InlineData(7, true)] [InlineData(9, false)]\n"
                "    public void Primality(int n, bool expected) => Assert.Equal(expected, Numbers.IsPrime(n));\n"
                "}\n"
            ),
        },
    },
    {
        "name": "implement_reverse_words",
        "instruction": (
            "TextUtil.ReverseWords should return the words in reverse order separated "
            "by single spaces. It currently returns an empty string. Implement it so "
            "TextUtilTests.cs passes."
        ),
        "files": {
            "TextUtil.cs": (
                "namespace Sandbox;\n\n"
                "public static class TextUtil\n{\n"
                "    public static string ReverseWords(string input) => string.Empty;\n"
                "}\n"
            ),
            "TextUtilTests.cs": (
                "using Xunit;\nusing Sandbox;\n\n"
                "public class TextUtilTests\n{\n"
                "    [Fact] public void Reverses() =>\n"
                "        Assert.Equal(\"world hello\", TextUtil.ReverseWords(\"hello world\"));\n"
                "    [Fact] public void SingleWord() =>\n"
                "        Assert.Equal(\"a\", TextUtil.ReverseWords(\"a\"));\n"
                "}\n"
            ),
        },
    },
]


# ---------------------------------------------------------------------------
# Sandbox tools (bound to one work_dir)
# ---------------------------------------------------------------------------

def _safe_path(work_dir: str, rel: str) -> str:
    """Resolve ``rel`` inside work_dir; raise if it escapes the sandbox."""
    full = os.path.realpath(os.path.join(work_dir, rel))
    if os.path.commonpath([full, os.path.realpath(work_dir)]) != os.path.realpath(work_dir):
        raise ValueError(f"path escapes sandbox: {rel}")
    return full


def _dotnet(work_dir: str, sub_args: list[str], timeout: int) -> tuple[int, str]:
    proc = subprocess.run(
        ["dotnet", *sub_args], cwd=work_dir, capture_output=True, text=True,
        timeout=timeout, stdin=subprocess.DEVNULL,
    )
    return proc.returncode, (proc.stdout or "") + "\n" + (proc.stderr or "")


def _build_timeout() -> int:
    return int(os.environ.get("L3_BUILD_TIMEOUT_S", "60"))


def _test_timeout() -> int:
    return int(os.environ.get("L3_TEST_TIMEOUT_S", "60"))


def make_tools(work_dir: str, counters: dict) -> list[Tool]:
    def list_files(_args):
        names = [f for f in os.listdir(work_dir) if f.endswith(".cs")]
        return {"files": sorted(names)}

    def read_file(args):
        with open(_safe_path(work_dir, args["path"]), "r", encoding="utf-8") as fh:
            return {"path": args["path"], "content": fh.read()}

    def write_file(args):
        path = _safe_path(work_dir, args["path"])
        with open(path, "w", encoding="utf-8") as fh:
            fh.write(args.get("content", ""))
        counters["writes"] += 1
        return {"ok": True, "path": args["path"], "bytes": len(args.get("content", ""))}

    def run_build(_args):
        counters["builds"] += 1
        rc, out = _dotnet(work_dir, ["build", "--no-restore"], _build_timeout())
        return {"build_success": rc == 0, "output": out[-_MAX_TOOL_OUTPUT:]}

    def run_tests(_args):
        counters["tests"] += 1
        # Build then test --no-build: a lone `dotnet test --no-restore` re-evaluates
        # the restore graph and can spuriously fail; this mirrors the L3 harness.
        b_rc, b_out = _dotnet(work_dir, ["build", "--no-restore"], _build_timeout())
        if b_rc != 0:
            return {"all_passed": False, "tests_passed": 0, "tests_total": 0,
                    "build_success": False, "output": b_out[-_MAX_TOOL_OUTPUT:]}
        rc, out = _dotnet(work_dir, ["test", "--no-restore", "--no-build"], _test_timeout())
        passed, total = _parse_test_counts(out)
        return {"all_passed": rc == 0, "tests_passed": passed, "tests_total": total,
                "build_success": True, "output": out[-_MAX_TOOL_OUTPUT:]}

    return [
        Tool("list_files", "List the .cs files in the project",
             {"type": "object", "properties": {}}, list_files),
        Tool("read_file", "Read a source file's full contents",
             {"type": "object", "properties": {"path": {"type": "string"}},
              "required": ["path"]}, read_file),
        Tool("write_file", "Overwrite a source file with new contents",
             {"type": "object", "properties": {"path": {"type": "string"},
              "content": {"type": "string"}}, "required": ["path", "content"]}, write_file),
        Tool("run_build", "Compile the project (dotnet build)",
             {"type": "object", "properties": {}}, run_build),
        Tool("run_tests", "Run the test suite (dotnet test)",
             {"type": "object", "properties": {}}, run_tests),
        Tool("finish", "Declare the task complete (tests should be green)",
             {"type": "object", "properties": {}}, lambda a: None),
    ]


_SYSTEM = (
    "You are a C# coding agent working in a real .NET project sandbox. Use the "
    "tools to inspect files, edit the source, build, and run the tests. Iterate "
    "until `run_tests` reports all tests passing, then call `finish`. Do not "
    "modify the test files. Make one tool call per step."
)


def authoritative_test(work_dir: str) -> tuple[bool, int, int]:
    """Final ground-truth dotnet test on the resulting files (build then test)."""
    try:
        b_rc, _ = _dotnet(work_dir, ["build", "--no-restore"], _build_timeout())
        if b_rc != 0:
            return False, 0, 0
        rc, out = _dotnet(work_dir, ["test", "--no-restore", "--no-build"], _test_timeout())
    except subprocess.SubprocessError:
        return False, 0, 0
    passed, total = _parse_test_counts(out)
    green = rc == 0 and total > 0
    if total == 0 and rc == 0:
        passed = total = 1
        green = True
    return green, passed, total


def run_one(model: str, task: dict, cached_template: str, think, max_steps: int) -> dict:
    work_dir = tempfile.mkdtemp(prefix=f"t3_{task['name']}_")
    shutil.rmtree(work_dir)
    shutil.copytree(cached_template, work_dir)
    for fname, content in task["files"].items():
        with open(os.path.join(work_dir, fname), "w", encoding="utf-8") as fh:
            fh.write(content)

    counters = {"writes": 0, "builds": 0, "tests": 0}
    tools = make_tools(work_dir, counters)
    user = (f"Task: {task['instruction']}\n\n"
            "Start by listing and reading the relevant files.")
    try:
        res = run_tool_loop(model, user, tools, system=_SYSTEM, max_steps=max_steps,
                            think=think, num_predict=1536, timeout=600)
        green, t_passed, t_total = authoritative_test(work_dir)
        return {
            "task": task["name"],
            "passed": green,
            "final_tests_passed": t_passed,
            "final_tests_total": t_total,
            "loop_finished": res.finished,
            "stop_reason": res.stop_reason,
            "n_steps": len(res.steps),
            "tool_steps": [s.tool for s in res.steps],
            "writes": counters["writes"],
            "builds": counters["builds"],
            "tests": counters["tests"],
        }
    finally:
        shutil.rmtree(work_dir, ignore_errors=True)


def main():
    p = argparse.ArgumentParser(description="Layer 4 T3: agentic C# coding loop.")
    p.add_argument("--model", required=True)
    p.add_argument("--output", default="")
    p.add_argument("--max-steps", type=int, default=10)
    p.add_argument("--think", default="false")
    p.add_argument("--template-base", default="scripts/coding_tasks/templates")
    args = p.parse_args()

    think_raw = args.think.strip().lower()
    think: object = think_raw if think_raw in ("low", "medium", "high") else (think_raw == "true")
    slug = model_slug(args.model)
    out_path = args.output or f"results/layer4-agentic-{slug}.json"

    # Dedicated minimal xunit sandbox (see templates/agentic_csharp): lean deps +
    # a nuget.config that bypasses the broken private feed, so restore/build are
    # reliable. (The L3 test_project template pulls MassTransit/EF which fail to
    # restore here.)
    template_dir = os.path.join(args.template_base, "agentic_csharp")
    cache_dir = os.path.join(args.template_base, ".cache", "agentic_csharp")
    print(f"[setup] model={args.model} host={OLLAMA_HOST} think={think!r}")
    print(f"[setup] restoring sandbox template -> {cache_dir}")
    cached_template = setup_template_cache(template_dir, cache_dir)

    started = datetime.datetime.now(datetime.timezone.utc)
    results = []
    for task in TASKS:
        print(f"\n[task] {task['name']} ...", flush=True)
        try:
            r = run_one(args.model, task, cached_template, think, args.max_steps)
        except Exception as exc:
            r = {"task": task["name"], "passed": False,
                 "harness_error": f"{type(exc).__name__}: {exc}"}
        results.append(r)
        print(f"  {'PASS' if r.get('passed') else 'FAIL'}  "
              f"tests={r.get('final_tests_passed')}/{r.get('final_tests_total')} "
              f"steps={r.get('n_steps')} builds={r.get('builds')} tests_run={r.get('tests')} "
              f"stop={r.get('stop_reason')}")

    passed = sum(1 for r in results if r.get("passed"))
    score = passed / len(results) if results else 0.0
    payload = {
        "benchmark": "layer4-agentic-csharp-t3",
        "model": args.model,
        "ollama_host": OLLAMA_HOST,
        "think_setting": think_raw,
        "run_started_at": started.isoformat(),
        "run_finished_at": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "max_steps": args.max_steps,
        "t3_score": score, "t3_passed": passed, "t3_total": len(results),
        "results": results,
    }
    os.makedirs(os.path.dirname(out_path) or ".", exist_ok=True)
    with open(out_path, "w", encoding="utf-8") as fh:
        fh.write(json.dumps(payload, indent=2) + "\n")
    print(f"\n[score] T3={passed}/{len(results)} ({score:.3f})")
    print(f"[done] written to {out_path}")


if __name__ == "__main__":
    main()
