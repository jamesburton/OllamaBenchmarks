"""Tool-use / function-calling benchmark — OpenAI-compatible backend variant.

Sibling of ``benchmark_tooluse.py`` (which hits Ollama's native ``/api/chat``)
following the same pattern already used for ``benchmark_quality.py`` vs.
``benchmark_quality_openai.py`` in this repo: same task set, same weighted
scoring (tool_selection / param_accuracy / no_hallucination), same 41-task
library under ``scripts/tooluse_tasks/`` — just posting to an OpenAI-compatible
``/v1/chat/completions`` endpoint (dotLLM, llama-server, vLLM, etc.) instead of
Ollama's ``/api/chat``.

This is NOT redundant with ``benchmark_quality_openai.py``'s built-in tool-use
check: that script only runs 2 trivial single-call tasks with strict exact-match
scoring, whereas this harness runs the full 41-task library (multi-tool
selection, nested/object arguments, hallucination checks, category breakdowns)
with graded partial-credit scoring.

Usage:
    python scripts/benchmark_tooluse_openai.py --models ggml-model-i2_s \
        --base-url http://127.0.0.1:8081
"""

import argparse
import datetime
import json
import os
import re
import time
import urllib.error
import urllib.request
from typing import Any


def model_slug(model: str) -> str:
    model = re.sub(r":latest$", "", model)
    return re.sub(
        r"[^\w\.-]",
        "_",
        model.replace(":", "_").replace("/", "_").replace("\\", "_"),
    )


def sampling_options(model: str, use_case: str = "tool") -> dict[str, Any]:
    if model.startswith(("nemotron-3-super", "nemotron-3-nano")):
        if use_case == "tool":
            return {"temperature": 0.6, "top_p": 0.95}
        return {"temperature": 1.0, "top_p": 1.0}
    return {"temperature": 0, "top_p": 1}


def write_json(path: str, payload: dict[str, Any]) -> None:
    os.makedirs(os.path.dirname(path) or ".", exist_ok=True)
    with open(path, "w", encoding="utf-8") as fh:
        fh.write(json.dumps(payload, indent=2) + "\n")


def post_json(base_url: str, path: str, payload: dict, api_key: str | None = None, timeout: int = 120) -> dict:
    req = urllib.request.Request(
        f"{base_url.rstrip('/')}{path}",
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    if api_key:
        req.add_header("Authorization", f"Bearer {api_key}")
    with urllib.request.urlopen(req, timeout=timeout) as response:
        return json.loads(response.read().decode("utf-8"))


def parse_arguments(arguments: Any) -> dict[str, Any]:
    if isinstance(arguments, dict):
        return arguments
    if isinstance(arguments, str):
        try:
            parsed = json.loads(arguments)
            if isinstance(parsed, dict):
                return parsed
        except json.JSONDecodeError:
            return {}
    return {}


def load_tool_library(task_dir: str) -> dict[str, dict]:
    lib_path = os.path.join(task_dir, "tool_definitions.json")
    with open(lib_path, "r", encoding="utf-8") as fh:
        return json.load(fh)


def load_tasks(task_dir: str, tool_library: dict[str, dict]) -> list[dict]:
    tasks: list[dict] = []
    for filename in sorted(os.listdir(task_dir)):
        if not filename.endswith(".json") or filename == "tool_definitions.json":
            continue
        filepath = os.path.join(task_dir, filename)
        with open(filepath, "r", encoding="utf-8") as fh:
            task = json.load(fh)
        tool_names = task.get("tool_names", [])
        task["tools"] = [tool_library[name] for name in tool_names if name in tool_library]
        tasks.append(task)
    return tasks


def _values_match(expected: Any, actual: Any) -> bool:
    if expected == actual:
        return True
    if isinstance(expected, (int, float)) and isinstance(actual, (int, float)):
        return float(expected) == float(actual)
    if isinstance(expected, int) and isinstance(actual, str):
        try:
            return expected == int(actual)
        except (ValueError, TypeError):
            return False
    if isinstance(expected, dict) and isinstance(actual, dict):
        if set(expected.keys()) != set(actual.keys()):
            return False
        return all(_values_match(expected[k], actual[k]) for k in expected)
    return False


def score_task(
    expected_calls: list[dict],
    actual_calls: list[dict],
    weights: dict | None = None,
) -> tuple[float, float, float, float]:
    if weights is None:
        weights = {
            "tool_selection_weight": 0.5,
            "param_accuracy_weight": 0.3,
            "no_hallucination_weight": 0.2,
        }

    w_sel = weights.get("tool_selection_weight", 0.5)
    w_param = weights.get("param_accuracy_weight", 0.3)
    w_hall = weights.get("no_hallucination_weight", 0.2)

    if not actual_calls:
        return (0.0, 0.0, 0.0, 0.0)

    expected_names = [c["name"] for c in expected_calls]
    actual_names = [c["function"]["name"] for c in actual_calls if "function" in c]

    if expected_names:
        matched = len(set(expected_names) & set(actual_names))
        selection_score = matched / len(expected_names)
    else:
        selection_score = 1.0

    param_scores: list[float] = []
    for exp in expected_calls:
        exp_args = exp.get("arguments", {})
        best = 0.0
        for act in actual_calls:
            fn = act.get("function", {})
            if fn.get("name") != exp["name"]:
                continue
            act_args = parse_arguments(fn.get("arguments", {}))
            if not exp_args:
                best = 1.0
                break
            matches = sum(
                1 for k, v in exp_args.items() if _values_match(v, act_args.get(k))
            )
            best = max(best, matches / len(exp_args))
        param_scores.append(best)
    param_score = sum(param_scores) / len(param_scores) if param_scores else 0.0

    hallucinated = len(set(actual_names) - set(expected_names))
    hallucination_score = 1.0 - min(
        hallucinated / max(len(expected_names), 1), 1.0
    )

    total = w_sel * selection_score + w_param * param_score + w_hall * hallucination_score
    return (
        round(total, 4),
        round(selection_score, 4),
        round(param_score, 4),
        round(hallucination_score, 4),
    )


def run_task(base_url: str, api_key: str | None, model: str, task: dict, timeout: int = 120) -> dict:
    """Send a single tool-use task to an OpenAI-compatible /v1/chat/completions and score it."""
    options = sampling_options(model, use_case="tool")
    payload = {
        "model": model,
        "messages": [{"role": "user", "content": task["prompt"]}],
        "tools": task["tools"],
        "max_tokens": 512,
        **options,
        "chat_template_kwargs": {"enable_thinking": False},
    }

    t0 = time.monotonic()
    try:
        resp = post_json(base_url, "/v1/chat/completions", payload, api_key=api_key, timeout=timeout)
    except (urllib.error.URLError, OSError, TimeoutError) as exc:
        elapsed = round(time.monotonic() - t0, 2)
        return {
            "task": task["name"],
            "category": task.get("category", "unknown"),
            "score": 0.0,
            "tool_selection": 0.0,
            "param_accuracy": 0.0,
            "no_hallucination": 0.0,
            "expected_calls": task.get("expected_calls", []),
            "actual_calls": [],
            "error": f"{type(exc).__name__}: {exc}",
            "generation_time_s": elapsed,
        }
    elapsed = round(time.monotonic() - t0, 2)

    message = resp.get("choices", [{}])[0].get("message", {})
    actual_calls = message.get("tool_calls") or []

    weights = task.get("scoring", None)
    total, sel, param, hall = score_task(
        task.get("expected_calls", []),
        actual_calls,
        weights,
    )

    return {
        "task": task["name"],
        "category": task.get("category", "unknown"),
        "score": total,
        "tool_selection": sel,
        "param_accuracy": param,
        "no_hallucination": hall,
        "expected_calls": task.get("expected_calls", []),
        "actual_calls": [
            {
                "function": {
                    "name": c.get("function", {}).get("name", ""),
                    "arguments": parse_arguments(
                        c.get("function", {}).get("arguments", {})
                    ),
                }
            }
            for c in actual_calls
        ],
        "generation_time_s": elapsed,
    }


def run_model(base_url: str, api_key: str | None, model: str, tasks: list[dict], checkpoint_dir: str, run_started_at: str) -> dict:
    results: list[dict] = []
    print(f"\n{'='*60}")
    print(f"  Model: {model}")
    print(f"  Tasks: {len(tasks)}")
    print(f"{'='*60}")

    for i, task in enumerate(tasks, 1):
        print(f"  [{i:2d}/{len(tasks)}] {task['name']:30s} ", end="", flush=True)
        result = run_task(base_url, api_key, model, task)
        results.append(result)
        indicator = "PASS" if result["score"] >= 0.5 else "FAIL"
        print(f"  score={result['score']:.2f}  ({indicator})  {result['generation_time_s']:.1f}s")

    cat_scores: dict[str, list[float]] = {}
    for r in results:
        cat_scores.setdefault(r["category"], []).append(r["score"])
    category_averages = {
        cat: round(sum(scores) / len(scores), 4) for cat, scores in cat_scores.items()
    }

    overall = round(
        sum(r["score"] for r in results) / len(results), 4
    ) if results else 0.0

    output = {
        "model": model,
        "benchmark": "tooluse_openai",
        "base_url": base_url,
        "run_started_at": run_started_at,
        "run_finished_at": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "results": results,
        "category_scores": category_averages,
        "overall_score": overall,
    }

    slug = model_slug(model)
    ckpt_path = os.path.join(checkpoint_dir, f"tooluse-openai-{slug}.json")
    write_json(ckpt_path, output)
    print(f"  Checkpoint: {ckpt_path}")

    return output


def print_leaderboard(all_results: list[dict]) -> None:
    print(f"\n{'='*70}")
    print("  TOOL-USE BENCHMARK LEADERBOARD (OpenAI-compatible)")
    print(f"{'='*70}")

    ranked = sorted(all_results, key=lambda r: r["overall_score"], reverse=True)

    cats = set()
    for r in ranked:
        cats.update(r.get("category_scores", {}).keys())
    cats_sorted = sorted(cats)

    header = f"  {'Model':<35s} {'Overall':>8s}"
    for c in cats_sorted:
        header += f" {c[:10]:>10s}"
    print(header)
    print(f"  {'-'*35} {'-'*8}" + "".join(f" {'-'*10}" for _ in cats_sorted))

    for r in ranked:
        line = f"  {r['model']:<35s} {r['overall_score']:>8.2%}"
        for c in cats_sorted:
            val = r.get("category_scores", {}).get(c, 0)
            line += f" {val:>10.2%}"
        print(line)
    print()


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Tool-use / function-calling benchmark for OpenAI-compatible servers (dotLLM, llama-server, vLLM, ...)."
    )
    parser.add_argument("--models", nargs="+", required=True, help="Model id(s) to benchmark")
    parser.add_argument("--base-url", required=True, help="e.g. http://127.0.0.1:8081")
    parser.add_argument("--api-key")
    parser.add_argument("--checkpoint-dir", default="results", help="Directory for result files")
    parser.add_argument("--task-dir", default="scripts/tooluse_tasks", help="Directory containing task JSON files")
    args = parser.parse_args()

    run_started_at = datetime.datetime.now(datetime.timezone.utc).isoformat()

    tool_library = load_tool_library(args.task_dir)
    tasks = load_tasks(args.task_dir, tool_library)

    if not tasks:
        print(f"No task files found in {args.task_dir}")
        return

    print(f"Loaded {len(tasks)} tasks from {args.task_dir}")
    print(f"Tool library: {len(tool_library)} tool definitions")

    all_results: list[dict] = []

    for model in args.models:
        try:
            result = run_model(args.base_url, args.api_key, model, tasks, args.checkpoint_dir, run_started_at)
            all_results.append(result)
        except Exception as exc:
            print(f"\n  ERROR running {model}: {exc}")
            slug = model_slug(model)
            err_path = os.path.join(args.checkpoint_dir, f"tooluse-openai-{slug}.json")
            write_json(err_path, {
                "model": model,
                "benchmark": "tooluse_openai",
                "base_url": args.base_url,
                "run_started_at": run_started_at,
                "run_finished_at": datetime.datetime.now(datetime.timezone.utc).isoformat(),
                "error": str(exc),
                "results": [],
                "category_scores": {},
                "overall_score": 0.0,
            })

    if all_results:
        print_leaderboard(all_results)

    agg_path = os.path.join(args.checkpoint_dir, "tooluse-openai-current.json")
    write_json(agg_path, {
        "benchmark": "tooluse_openai",
        "base_url": args.base_url,
        "run_started_at": run_started_at,
        "run_finished_at": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "models": args.models,
        "results": all_results,
    })
    print(f"\nAggregate results: {agg_path}")


if __name__ == "__main__":
    main()
