"""
Tool-use / function-calling benchmark for Ollama models.

Sends prompts with tool definitions to /api/chat and scores whether the model
selects the correct tool(s) with the correct arguments.

Usage:
    python scripts/benchmark_tooluse.py --models qwen3:8b phi4-mini
"""

import argparse
import datetime
import json
import os
import re
import time
import urllib.request
from typing import Any


# ---------------------------------------------------------------------------
# Helpers copied from the existing codebase conventions
# ---------------------------------------------------------------------------

def model_slug(model: str) -> str:
    """Same convention as benchmark_quality.py / task_runner.py."""
    model = re.sub(r":latest$", "", model)
    return re.sub(
        r"[^\w\.-]",
        "_",
        model.replace(":","_").replace("/","_").replace("\\","_"),
    )


def sampling_options(model: str, use_case: str = "tool") -> dict[str, Any]:
    """Return temperature/top_p for the model family.

    Nemotron models use a warmer setting for tool calls; everything else
    gets deterministic temperature=0.
    """
    if model.startswith(("nemotron-3-super", "nemotron-3-nano")):
        if use_case == "tool":
            return {"temperature": 0.6, "top_p": 0.95}
        return {"temperature": 1.0, "top_p": 1.0}
    return {"temperature": 0, "top_p": 1}


def write_json(path: str, payload: dict[str, Any]) -> None:
    os.makedirs(os.path.dirname(path) or ".", exist_ok=True)
    with open(path, "w", encoding="utf-8") as fh:
        fh.write(json.dumps(payload, indent=2) + "\n")


def post_json(path: str, payload: dict, timeout: int = 120) -> dict:
    """POST JSON to Ollama and return parsed response."""
    req = urllib.request.Request(
        f"http://127.0.0.1:11434{path}",
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(req, timeout=timeout) as response:
        return json.loads(response.read().decode("utf-8"))


def parse_arguments(arguments: Any) -> dict[str, Any]:
    """Normalise tool-call arguments that may arrive as str or dict."""
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


# ---------------------------------------------------------------------------
# Task loading
# ---------------------------------------------------------------------------

def load_tool_library(task_dir: str) -> dict[str, dict]:
    """Load the shared tool_definitions.json library."""
    lib_path = os.path.join(task_dir, "tool_definitions.json")
    with open(lib_path, "r", encoding="utf-8") as fh:
        return json.load(fh)


def load_tasks(task_dir: str, tool_library: dict[str, dict]) -> list[dict]:
    """Load all task JSON files and resolve tool_names against the library."""
    tasks: list[dict] = []
    for filename in sorted(os.listdir(task_dir)):
        if not filename.endswith(".json") or filename == "tool_definitions.json":
            continue
        filepath = os.path.join(task_dir, filename)
        with open(filepath, "r", encoding="utf-8") as fh:
            task = json.load(fh)
        # Resolve tool definitions from the library
        tool_names = task.get("tool_names", [])
        task["tools"] = [tool_library[name] for name in tool_names if name in tool_library]
        tasks.append(task)
    return tasks


# ---------------------------------------------------------------------------
# Scoring
# ---------------------------------------------------------------------------

def _values_match(expected: Any, actual: Any) -> bool:
    """Compare expected vs actual argument values with type flexibility.

    Handles int/float coercion and case-insensitive string comparison for
    common fields where models may reasonably vary.
    """
    if expected == actual:
        return True
    # int/float coercion (e.g. expected 5 vs actual 5.0)
    if isinstance(expected, (int, float)) and isinstance(actual, (int, float)):
        return float(expected) == float(actual)
    # string coercion (model may return "5" instead of 5)
    if isinstance(expected, int) and isinstance(actual, str):
        try:
            return expected == int(actual)
        except (ValueError, TypeError):
            return False
    # nested dict/list — fall through to ==
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
    """Score a single task.

    Returns (total, tool_selection, param_accuracy, no_hallucination).
    """
    if weights is None:
        weights = {
            "tool_selection_weight": 0.5,
            "param_accuracy_weight": 0.3,
            "no_hallucination_weight": 0.2,
        }

    w_sel = weights.get("tool_selection_weight", 0.5)
    w_param = weights.get("param_accuracy_weight", 0.3)
    w_hall = weights.get("no_hallucination_weight", 0.2)

    # No tool calls at all → score 0
    if not actual_calls:
        return (0.0, 0.0, 0.0, 0.0)

    expected_names = [c["name"] for c in expected_calls]
    actual_names = [c["function"]["name"] for c in actual_calls if "function" in c]

    # --- Tool selection ---
    if expected_names:
        matched = len(set(expected_names) & set(actual_names))
        selection_score = matched / len(expected_names)
    else:
        selection_score = 1.0

    # --- Parameter accuracy ---
    param_scores: list[float] = []
    for exp in expected_calls:
        exp_args = exp.get("arguments", {})
        # Find a matching actual call
        best = 0.0
        for act in actual_calls:
            fn = act.get("function", {})
            if fn.get("name") != exp["name"]:
                continue
            act_args = parse_arguments(fn.get("arguments", {}))
            if not exp_args:
                # No expected args → full marks if tool was called
                best = 1.0
                break
            matches = sum(
                1 for k, v in exp_args.items() if _values_match(v, act_args.get(k))
            )
            best = max(best, matches / len(exp_args))
        param_scores.append(best)
    param_score = sum(param_scores) / len(param_scores) if param_scores else 0.0

    # --- No hallucination ---
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


# ---------------------------------------------------------------------------
# Runner
# ---------------------------------------------------------------------------

def run_task(model: str, task: dict, timeout: int = 120) -> dict:
    """Send a single tool-use task to Ollama and score the response."""
    options = sampling_options(model, use_case="tool")
    payload = {
        "model": model,
        "messages": [{"role": "user", "content": task["prompt"]}],
        "tools": task["tools"],
        "stream": False,
        "options": {
            **options,
            "num_predict": 512,
            "seed": 42,
        },
    }

    t0 = time.monotonic()
    try:
        resp = post_json("/api/chat", payload, timeout=timeout)
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

    # Extract tool_calls from response
    message = resp.get("message", {})
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


def run_model(model: str, tasks: list[dict], checkpoint_dir: str, run_started_at: str) -> dict:
    """Run all tasks for a single model and write per-model checkpoint."""
    results: list[dict] = []
    print(f"\n{'='*60}")
    print(f"  Model: {model}")
    print(f"  Tasks: {len(tasks)}")
    print(f"{'='*60}")

    for i, task in enumerate(tasks, 1):
        print(f"  [{i:2d}/{len(tasks)}] {task['name']:30s} ", end="", flush=True)
        result = run_task(model, task)
        results.append(result)
        indicator = "PASS" if result["score"] >= 0.5 else "FAIL"
        print(f"  score={result['score']:.2f}  ({indicator})  {result['generation_time_s']:.1f}s")

    # Compute category scores
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
        "benchmark": "tooluse",
        "run_started_at": run_started_at,
        "run_finished_at": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "results": results,
        "category_scores": category_averages,
        "overall_score": overall,
    }

    # Write per-model checkpoint
    slug = model_slug(model)
    ckpt_path = os.path.join(checkpoint_dir, f"tooluse-{slug}.json")
    write_json(ckpt_path, output)
    print(f"  Checkpoint: {ckpt_path}")

    return output


# ---------------------------------------------------------------------------
# Leaderboard
# ---------------------------------------------------------------------------

def print_leaderboard(all_results: list[dict]) -> None:
    """Print a summary leaderboard to stdout."""
    print(f"\n{'='*70}")
    print("  TOOL-USE BENCHMARK LEADERBOARD")
    print(f"{'='*70}")

    # Sort by overall score descending
    ranked = sorted(all_results, key=lambda r: r["overall_score"], reverse=True)

    # Header
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

    # Per-category breakdown
    for r in ranked:
        print(f"\n  {r['model']}:")
        for res in r["results"]:
            status = "PASS" if res["score"] >= 0.5 else "FAIL"
            expected_names = [c["name"] for c in res.get("expected_calls", [])]
            actual_names = [
                c.get("function", {}).get("name", "?")
                for c in res.get("actual_calls", [])
            ]
            print(
                f"    {status} {res['task']:<30s}  "
                f"score={res['score']:.2f}  "
                f"sel={res['tool_selection']:.2f}  "
                f"param={res['param_accuracy']:.2f}  "
                f"hall={res['no_hallucination']:.2f}  "
                f"expected={expected_names}  "
                f"actual={actual_names}"
            )


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main() -> None:
    parser = argparse.ArgumentParser(
        description="Tool-use / function-calling benchmark for Ollama models."
    )
    parser.add_argument("--models", nargs="+", required=True, help="Model names to benchmark")
    parser.add_argument("--checkpoint-dir", default="results", help="Directory for result files")
    parser.add_argument("--task-dir", default="scripts/tooluse_tasks", help="Directory containing task JSON files")
    args = parser.parse_args()

    run_started_at = datetime.datetime.now(datetime.timezone.utc).isoformat()

    # Load tool library and tasks
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
            result = run_model(model, tasks, args.checkpoint_dir, run_started_at)
            all_results.append(result)
        except Exception as exc:
            print(f"\n  ERROR running {model}: {exc}")
            # Write error checkpoint
            slug = model_slug(model)
            err_path = os.path.join(args.checkpoint_dir, f"tooluse-{slug}.json")
            write_json(err_path, {
                "model": model,
                "benchmark": "tooluse",
                "run_started_at": run_started_at,
                "run_finished_at": datetime.datetime.now(datetime.timezone.utc).isoformat(),
                "error": str(exc),
                "results": [],
                "category_scores": {},
                "overall_score": 0.0,
            })

    # Print leaderboard
    if all_results:
        print_leaderboard(all_results)

    # Write aggregate result
    agg_path = os.path.join(args.checkpoint_dir, "tooluse-current.json")
    write_json(agg_path, {
        "benchmark": "tooluse",
        "run_started_at": run_started_at,
        "run_finished_at": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "models": args.models,
        "results": all_results,
    })
    print(f"\nAggregate results: {agg_path}")


if __name__ == "__main__":
    main()
