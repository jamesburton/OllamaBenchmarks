"""Layer 4 — T1 (tool-use correctness) + T2 (agentic ReAct loop).

Pure generation (no dotnet), so this runs cross-machine via ``OLLAMA_HOST``.

T1 — single-turn tool correctness, one point per case:
  single            : pick the only tool, exact args
  multi_distractor  : pick the right tool among distractors
  nested_args       : object-valued argument filled correctly
  parallel          : emit >=2 tool calls in ONE turn (parallel calling)
  abstention        : do NOT call a tool when none is relevant (over-eager guard)
  sequential_dep    : 2-step chain where tool B's arg is tool A's RESULT
Score = fraction of cases passed.

T2 — multi-step ReAct loop (search -> calculate -> store -> finish) with a
second scenario that injects a transient tool error to test recovery. Score =
fraction of scenarios that finished with the correct answer.

Output: results/layer4-tools-{slug}.json.
"""

import argparse
import ast
import datetime
import json
import operator
import os
import sys

sys.path.insert(0, os.path.dirname(__file__))
from coding_tasks.task_runner import model_slug
from coding_tasks.tool_loop import (
    Tool, run_tool_loop, request_tool_calls, parse_arguments, OLLAMA_HOST,
)


# ---------------------------------------------------------------------------
# Safe arithmetic evaluator for the calculator tool
# ---------------------------------------------------------------------------

_OPS = {
    ast.Add: operator.add, ast.Sub: operator.sub, ast.Mult: operator.mul,
    ast.Div: operator.truediv, ast.Pow: operator.pow, ast.Mod: operator.mod,
    ast.USub: operator.neg, ast.UAdd: operator.pos,
}


def safe_eval(expr: str) -> float:
    """Evaluate a pure-arithmetic expression (no names/calls). Raises on anything else."""
    def _ev(node: ast.AST) -> float:
        if isinstance(node, ast.Constant) and isinstance(node.value, (int, float)):
            return node.value
        if isinstance(node, ast.BinOp) and type(node.op) in _OPS:
            return _OPS[type(node.op)](_ev(node.left), _ev(node.right))
        if isinstance(node, ast.UnaryOp) and type(node.op) in _OPS:
            return _OPS[type(node.op)](_ev(node.operand))
        raise ValueError(f"unsupported expression element: {ast.dump(node)}")
    return _ev(ast.parse(expr, mode="eval").body)


def _deep_match(actual, expected) -> bool:
    """Recursively check expected values appear in actual (nested dicts)."""
    if isinstance(expected, dict):
        if not isinstance(actual, dict):
            return False
        return all(_deep_match(actual.get(k), v) for k, v in expected.items())
    return actual == expected


# ---------------------------------------------------------------------------
# Tool specs (schemas only, for the single-turn T1 cases)
# ---------------------------------------------------------------------------

def _fn(name, desc, props, required):
    return {"type": "function", "function": {
        "name": name, "description": desc,
        "parameters": {"type": "object", "properties": props, "required": required}}}


T_ADD = _fn("add_numbers", "Add two integers",
            {"a": {"type": "integer"}, "b": {"type": "integer"}}, ["a", "b"])
T_MUL = _fn("multiply_numbers", "Multiply two integers",
            {"a": {"type": "integer"}, "b": {"type": "integer"}}, ["a", "b"])
T_TZ = _fn("city_timezone", "Get timezone for a city",
           {"city": {"type": "string"}}, ["city"])
T_WEATHER = _fn("get_weather", "Get current weather for a city",
                {"city": {"type": "string"},
                 "unit": {"type": "string", "enum": ["celsius", "fahrenheit"]}},
                ["city", "unit"])
T_SHIP = _fn("create_shipment", "Create a shipping order",
             {"order_id": {"type": "string"},
              "address": {"type": "object", "properties": {
                  "street": {"type": "string"}, "city": {"type": "string"},
                  "zip": {"type": "string"}}, "required": ["street", "city", "zip"]},
              "priority": {"type": "string", "enum": ["standard", "express"]}},
             ["order_id", "address", "priority"])


# ---------------------------------------------------------------------------
# T1 — single-turn correctness
# ---------------------------------------------------------------------------

def t1_single(model, think):
    msg, calls = request_tool_calls(
        model, "Call add_numbers with a=17 and b=25. Tool call only.", [T_ADD], think=think)
    ok = bool(calls) and calls[0]["function"]["name"] == "add_numbers" and \
        _deep_match(parse_arguments(calls[0]["function"]["arguments"]), {"a": 17, "b": 25})
    return ok, {"calls": _calls_brief(calls)}


def t1_multi_distractor(model, think):
    msg, calls = request_tool_calls(
        model, "I need the product of 6 and 9. Use the correct tool. Tool call only.",
        [T_MUL, T_ADD, T_TZ], think=think)
    ok = bool(calls) and calls[0]["function"]["name"] == "multiply_numbers" and \
        _deep_match(parse_arguments(calls[0]["function"]["arguments"]), {"a": 6, "b": 9})
    return ok, {"calls": _calls_brief(calls)}


def t1_nested_args(model, think):
    msg, calls = request_tool_calls(
        model,
        'Create a shipment for order "ORD-42" to 123 Main St, Seattle, zip 98101, '
        "with express priority. Tool call only.", [T_SHIP], think=think)
    expected = {"order_id": "ORD-42",
                "address": {"street": "123 Main St", "city": "Seattle", "zip": "98101"},
                "priority": "express"}
    ok = bool(calls) and calls[0]["function"]["name"] == "create_shipment" and \
        _deep_match(parse_arguments(calls[0]["function"]["arguments"]), expected)
    return ok, {"calls": _calls_brief(calls)}


def t1_parallel(model, think):
    # A single turn should emit two get_weather calls (Paris + Tokyo).
    msg, calls = request_tool_calls(
        model,
        "Get the current weather in celsius for BOTH Paris and Tokyo. "
        "Make all needed tool calls now.", [T_WEATHER], think=think, num_predict=256)
    names = [c["function"]["name"] for c in calls]
    cities = {parse_arguments(c["function"]["arguments"]).get("city", "").lower()
              for c in calls}
    ok = len(calls) >= 2 and all(n == "get_weather" for n in names) and \
        {"paris", "tokyo"}.issubset(cities)
    return ok, {"n_calls": len(calls), "calls": _calls_brief(calls)}


def t1_abstention(model, think):
    # Only an irrelevant tool is offered; the model must NOT call it.
    msg, calls = request_tool_calls(
        model,
        "What is the capital of France? Answer in plain text — do not use any tool.",
        [T_WEATHER], think=think)
    ok = len(calls) == 0
    return ok, {"n_calls": len(calls), "content_head": (msg.get("content") or "")[:80]}


def t1_sequential_dep(model, think):
    # 2-step chain: get_user_id(name) -> user_id; get_order_count(user_id) -> count.
    seen = {}

    def get_user_id(args):
        seen["name"] = args.get("name")
        return {"user_id": "U-1001"}

    def get_order_count(args):
        seen["passed_user_id"] = args.get("user_id")
        return {"count": 7}

    tools = [
        Tool("get_user_id", "Look up a user's id by their name",
             {"type": "object", "properties": {"name": {"type": "string"}},
              "required": ["name"]}, get_user_id),
        Tool("get_order_count", "Get the number of orders for a user id",
             {"type": "object", "properties": {"user_id": {"type": "string"}},
              "required": ["user_id"]}, get_order_count),
        Tool("finish", "Return the final order count",
             {"type": "object", "properties": {"count": {"type": "integer"}},
              "required": ["count"]}, lambda a: None),
    ]
    res = run_tool_loop(
        model,
        "How many orders does the user named 'alice' have? First call get_user_id "
        "to get her id, then call get_order_count with that id, then finish with the count.",
        tools, max_steps=5, think=think)
    chained = seen.get("passed_user_id") == "U-1001"  # B used A's returned id
    ok = res.finished and chained
    return ok, {"chained": chained, "stop": res.stop_reason,
                "steps": [s.tool for s in res.steps], "seen": seen}


def _calls_brief(calls):
    return [{"name": c.get("function", {}).get("name"),
             "args": parse_arguments(c.get("function", {}).get("arguments"))}
            for c in calls]


T1_CASES = [
    ("single", t1_single),
    ("multi_distractor", t1_multi_distractor),
    ("nested_args", t1_nested_args),
    ("parallel", t1_parallel),
    ("abstention", t1_abstention),
    ("sequential_dep", t1_sequential_dep),
]


# ---------------------------------------------------------------------------
# T2 — multi-step ReAct loop
# ---------------------------------------------------------------------------

_CORPUS = {
    "widget": "The catalogue price of widget X is 80 dollars.",
    "shipping": "Standard shipping is 12 dollars per order.",
    "tax": "Sales tax is 9 percent.",
}


def _make_t2_tools(kv: dict, fail_first_search: bool, fail_state: dict):
    def search_corpus(args):
        if fail_first_search and not fail_state.get("searched_once"):
            fail_state["searched_once"] = True
            return {"error": "search temporarily unavailable, please retry"}
        q = (args.get("query") or "").lower()
        for key, val in _CORPUS.items():
            if key in q:
                return {"result": val}
        return {"result": "no matching entry"}

    def calculator(args):
        try:
            return {"value": safe_eval(str(args.get("expression", "")))}
        except Exception as exc:
            return {"error": f"bad expression: {exc}"}

    def kv_set(args):
        kv[str(args.get("key"))] = args.get("value")
        return {"ok": True, "stored": {args.get("key"): args.get("value")}}

    return [
        Tool("search_corpus", "Search the product corpus for a fact",
             {"type": "object", "properties": {"query": {"type": "string"}},
              "required": ["query"]}, search_corpus),
        Tool("calculator", "Evaluate an arithmetic expression, e.g. '80 * 0.85'",
             {"type": "object", "properties": {"expression": {"type": "string"}},
              "required": ["expression"]}, calculator),
        Tool("kv_store_set", "Store a value under a key",
             {"type": "object", "properties": {"key": {"type": "string"},
              "value": {}}, "required": ["key", "value"]}, kv_set),
        Tool("finish", "Return the final numeric answer",
             {"type": "object", "properties": {"answer": {"type": "number"}},
              "required": ["answer"]}, lambda a: None),
    ]


_T2_GOAL = (
    "Goal: look up the catalogue price of widget X using search_corpus, apply a "
    "15% discount with the calculator, store the discounted price under the key "
    "'final_price' with kv_store_set, then call finish with the discounted price. "
    "Use one tool per step and use the actual tool results."
)


def run_t2(model, think, fail_first_search):
    kv: dict = {}
    fail_state: dict = {}
    tools = _make_t2_tools(kv, fail_first_search, fail_state)
    res = run_tool_loop(model, _T2_GOAL, tools, max_steps=10, think=think, num_predict=512)
    answer = (res.final_args or {}).get("answer")
    correct = answer is not None and abs(float(answer) - 68.0) < 0.51  # 80 * 0.85
    used_search = any(s.tool == "search_corpus" and not s.error for s in res.steps)
    recovered = (not fail_first_search) or (
        fail_state.get("searched_once") and used_search and correct)
    return {
        "finished": res.finished,
        "correct": bool(correct),
        "answer": answer,
        "steps": [s.tool for s in res.steps],
        "n_steps": len(res.steps),
        "stop_reason": res.stop_reason,
        "kv_final_price": kv.get("final_price"),
        "error_injected": fail_first_search,
        "recovered": bool(recovered),
    }


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main():
    p = argparse.ArgumentParser(description="Layer 4 T1+T2: tool-use + agentic loop.")
    p.add_argument("--model", required=True)
    p.add_argument("--output", default="")
    p.add_argument("--think", default="false",
                   help="think setting passed to Ollama (false/true/low/medium/high)")
    args = p.parse_args()

    think_raw = args.think.strip().lower()
    think: object = think_raw if think_raw in ("low", "medium", "high") else (think_raw == "true")
    slug = model_slug(args.model)
    out_path = args.output or f"results/layer4-tools-{slug}.json"

    print(f"[setup] model={args.model} host={OLLAMA_HOST} think={think!r}")
    started = datetime.datetime.now(datetime.timezone.utc)

    # --- T1 ---
    print("\n[T1] tool-use correctness")
    t1_results = []
    for name, fn in T1_CASES:
        try:
            ok, detail = fn(args.model, think)
        except Exception as exc:
            ok, detail = False, {"error": f"{type(exc).__name__}: {exc}"}
        t1_results.append({"case": name, "passed": bool(ok), "detail": detail})
        print(f"  {'PASS' if ok else 'FAIL'}  {name}")
    t1_passed = sum(1 for r in t1_results if r["passed"])
    t1_score = t1_passed / len(t1_results)

    # --- T2 ---
    print("\n[T2] agentic ReAct loop")
    t2_results = []
    for label, fail in [("multi_step", False), ("error_recovery", True)]:
        try:
            r = run_t2(args.model, think, fail)
        except Exception as exc:
            r = {"error": f"{type(exc).__name__}: {exc}", "correct": False}
        r["scenario"] = label
        t2_results.append(r)
        print(f"  {'PASS' if r.get('correct') else 'FAIL'}  {label}  "
              f"steps={r.get('steps')} answer={r.get('answer')} recovered={r.get('recovered')}")
    t2_correct = sum(1 for r in t2_results if r.get("correct"))
    t2_score = t2_correct / len(t2_results)

    payload = {
        "benchmark": "layer4-tools-t1-t2",
        "model": args.model,
        "ollama_host": OLLAMA_HOST,
        "think_setting": think_raw,
        "run_started_at": started.isoformat(),
        "run_finished_at": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "t1_score": t1_score, "t1_passed": t1_passed, "t1_total": len(t1_results),
        "t1_results": t1_results,
        "t2_score": t2_score, "t2_correct": t2_correct, "t2_total": len(t2_results),
        "t2_results": t2_results,
    }
    os.makedirs(os.path.dirname(out_path) or ".", exist_ok=True)
    with open(out_path, "w", encoding="utf-8") as fh:
        fh.write(json.dumps(payload, indent=2) + "\n")

    print(f"\n[score] T1={t1_passed}/{len(t1_results)} ({t1_score:.3f})  "
          f"T2={t2_correct}/{len(t2_results)} ({t2_score:.3f})")
    print(f"[done] written to {out_path}")


if __name__ == "__main__":
    main()
