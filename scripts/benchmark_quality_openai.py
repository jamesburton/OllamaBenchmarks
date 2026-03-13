import argparse
import datetime
import json
import os
import re
import subprocess
import tempfile
import textwrap
import time
from typing import Any
import urllib.request

from collect_host_info import build_host_info

CODING_TASKS = [
    (
        "longest_common_prefix",
        "Return only Python code. Define longest_common_prefix(strs: list[str]) -> str. Handle empty list.",
        [
            'assert longest_common_prefix(["flower","flow","flight"])=="fl"',
            'assert longest_common_prefix(["dog","racecar","car"])==""',
            'assert longest_common_prefix([])==""',
        ],
    ),
    (
        "top_k_frequent",
        "Return only Python code. Define top_k_frequent(nums: list[int], k: int) -> list[int]. Sort by frequency desc, tie by smaller number first.",
        [
            "assert top_k_frequent([1,1,1,2,2,3],2)==[1,2]",
            "assert top_k_frequent([4,4,1,1,2,2],2)==[1,2]",
            "assert top_k_frequent([5],1)==[5]",
        ],
    ),
]

TOOL_TASKS = [
    (
        "add_numbers",
        "Call add_numbers with a=17 and b=25. Tool call only.",
        {
            "type": "function",
            "function": {
                "name": "add_numbers",
                "description": "Add two integers",
                "parameters": {
                    "type": "object",
                    "properties": {"a": {"type": "integer"}, "b": {"type": "integer"}},
                    "required": ["a", "b"],
                },
            },
        },
        "add_numbers",
        {"a": 17, "b": 25},
    ),
    (
        "city_timezone",
        'Call city_timezone with city="Tokyo". Tool call only.',
        {
            "type": "function",
            "function": {
                "name": "city_timezone",
                "description": "Get timezone",
                "parameters": {
                    "type": "object",
                    "properties": {"city": {"type": "string"}},
                    "required": ["city"],
                },
            },
        },
        "city_timezone",
        {"city": "Tokyo"},
    ),
]

PLAN_AGENT_TASKS = [
    {
        "name": "write_summary_report",
        "prompt": (
            "You are coordinating work on a tiny benchmark reporting task. "
            "First create a short plan using the create_plan tool. "
            "Then request exactly one sub-agent with request_subagent to draft a summary from benchmark data. "
            "After the tool returns, finish by calling finalize_result with a concise manager summary. "
            "Do not skip steps."
        ),
        "expected_plan_keywords": ["benchmark", "summary"],
        "subagent_result": {
            "status": "completed",
            "summary": "Drafted a three-line benchmark summary for qwen3-coder-next and lfm2.",
        },
    }
]


def model_slug(model: str) -> str:
    return re.sub(r"[^\w\.-]", "_", model.replace(":", "_").replace("/", "_").replace("\\", "_"))


def write_json(path: str, payload: dict[str, Any]) -> None:
    os.makedirs(os.path.dirname(path) or ".", exist_ok=True)
    with open(path, "w", encoding="utf-8") as handle:
        handle.write(json.dumps(payload, indent=2) + "\n")


def post_json(base_url: str, path: str, payload: dict[str, Any], api_key: str | None = None, timeout: int = 1200) -> dict[str, Any]:
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


def chat_completion(base_url: str, model: str, messages: list[dict[str, Any]], api_key: str | None = None, tools: list[dict[str, Any]] | None = None, max_tokens: int = 220) -> dict[str, Any]:
    payload: dict[str, Any] = {
        "model": model,
        "messages": messages,
        "temperature": 0,
        "max_tokens": max_tokens,
    }
    if tools:
        payload["tools"] = tools
    return post_json(base_url, "/v1/chat/completions", payload, api_key=api_key)


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


def extract_code(text: str) -> str:
    if not text:
        return ""
    match = re.search(r"```(?:python)?\s*(.*?)```", text, flags=re.S | re.I)
    code = match.group(1) if match else text
    code = textwrap.dedent(code).strip()
    lines = code.splitlines()
    start = 0
    for index, line in enumerate(lines):
        if line.strip().startswith(("def ", "class ", "from ", "import ", "@")):
            start = index
            break
    return "\n".join(lines[start:]).strip()


def run_python_asserts(code: str, asserts: list[str]) -> bool:
    script = code + "\n\n" + "\n".join(asserts) + '\nprint("OK")\n'
    with tempfile.NamedTemporaryFile("w", suffix=".py", delete=False, encoding="utf-8") as handle:
        handle.write(script)
        path = handle.name
    try:
        completed = subprocess.run(["python", path], capture_output=True, text=True, timeout=20)
        return completed.returncode == 0 and "OK" in completed.stdout
    finally:
        try:
            os.unlink(path)
        except OSError:
            pass


def run_tool_task(base_url: str, api_key: str | None, model: str, prompt: str, tool: dict[str, Any], expected_name: str, expected_args: dict[str, Any]) -> bool:
    response = chat_completion(base_url, model, [{"role": "user", "content": prompt}], api_key=api_key, tools=[tool], max_tokens=100)
    message = response.get("choices", [{}])[0].get("message", {})
    calls = message.get("tool_calls") or []
    if not calls:
        return False
    function = calls[0].get("function") or {}
    if function.get("name") != expected_name:
        return False
    arguments = parse_arguments(function.get("arguments"))
    return all(arguments.get(key) == value for key, value in expected_args.items())


def run_plan_agent_task(base_url: str, api_key: str | None, model: str, task: dict[str, Any]) -> bool:
    tools = [
        {
            "type": "function",
            "function": {
                "name": "create_plan",
                "description": "Create a short execution plan before delegating work",
                "parameters": {
                    "type": "object",
                    "properties": {
                        "title": {"type": "string"},
                        "steps": {"type": "array", "items": {"type": "string"}, "minItems": 1},
                    },
                    "required": ["title", "steps"],
                },
            },
        },
        {
            "type": "function",
            "function": {
                "name": "request_subagent",
                "description": "Ask a sub-agent to perform one scoped subtask",
                "parameters": {
                    "type": "object",
                    "properties": {
                        "worker_type": {"type": "string"},
                        "task": {"type": "string"},
                    },
                    "required": ["worker_type", "task"],
                },
            },
        },
        {
            "type": "function",
            "function": {
                "name": "finalize_result",
                "description": "Return the final concise manager summary",
                "parameters": {
                    "type": "object",
                    "properties": {"summary": {"type": "string"}},
                    "required": ["summary"],
                },
            },
        },
    ]

    messages: list[dict[str, Any]] = [{"role": "user", "content": task["prompt"]}]
    first = chat_completion(base_url, model, messages, api_key=api_key, tools=tools)
    first_message = first.get("choices", [{}])[0].get("message", {})
    tool_calls = first_message.get("tool_calls") or []
    if not tool_calls:
        return False
    plan_call = tool_calls[0].get("function") or {}
    if plan_call.get("name") != "create_plan":
        return False
    plan_args = parse_arguments(plan_call.get("arguments"))
    steps = plan_args.get("steps")
    if not isinstance(steps, list) or len(steps) < 1:
        return False
    plan_text = " ".join([str(plan_args.get("title") or "")] + [str(step) for step in steps]).lower()
    for expected in task["expected_plan_keywords"]:
        if expected not in plan_text:
            return False

    messages.extend([
        {"role": "assistant", "content": first_message.get("content") or "", "tool_calls": tool_calls},
        {"role": "tool", "tool_call_id": tool_calls[0].get("id") or "create_plan_call", "content": json.dumps({"status": "ok", "accepted": True})},
    ])

    second = chat_completion(base_url, model, messages, api_key=api_key, tools=tools)
    second_message = second.get("choices", [{}])[0].get("message", {})
    second_calls = second_message.get("tool_calls") or []
    if not second_calls:
        return False
    subagent_call = second_calls[0].get("function") or {}
    if subagent_call.get("name") != "request_subagent":
        return False
    subagent_args = parse_arguments(subagent_call.get("arguments"))
    if not subagent_args.get("task") or not subagent_args.get("worker_type"):
        return False

    messages.extend([
        {"role": "assistant", "content": second_message.get("content") or "", "tool_calls": second_calls},
        {"role": "tool", "tool_call_id": second_calls[0].get("id") or "request_subagent_call", "content": json.dumps(task["subagent_result"])},
    ])

    third = chat_completion(base_url, model, messages, api_key=api_key, tools=tools)
    third_message = third.get("choices", [{}])[0].get("message", {})
    third_calls = third_message.get("tool_calls") or []
    if not third_calls:
        return False
    final_call = third_calls[0].get("function") or {}
    if final_call.get("name") != "finalize_result":
        return False
    final_args = parse_arguments(final_call.get("arguments"))
    summary = str(final_args.get("summary") or "").lower()
    return "summary" in summary or "draft" in summary or "benchmark" in summary


def run_model(base_url: str, api_key: str | None, model: str) -> dict[str, Any]:
    row = {
        "model": model,
        "coding_pass": 0,
        "coding_total": len(CODING_TASKS),
        "tool_pass": 0,
        "tool_total": len(TOOL_TASKS),
        "agentic_pass": 0,
        "agentic_total": len(PLAN_AGENT_TASKS),
    }

    for _, prompt, asserts in CODING_TASKS:
        try:
            response = chat_completion(base_url, model, [{"role": "user", "content": prompt}], api_key=api_key)
            text = response.get("choices", [{}])[0].get("message", {}).get("content", "")
            code = extract_code(text)
            if run_python_asserts(code, asserts):
                row["coding_pass"] += 1
        except Exception:
            pass

    for _, prompt, tool, expected_name, expected_args in TOOL_TASKS:
        try:
            if run_tool_task(base_url, api_key, model, prompt, tool, expected_name, expected_args):
                row["tool_pass"] += 1
        except Exception:
            pass

    for task in PLAN_AGENT_TASKS:
        try:
            if run_plan_agent_task(base_url, api_key, model, task):
                row["agentic_pass"] += 1
        except Exception:
            pass

    row["score"] = row["coding_pass"] + row["tool_pass"] + row["agentic_pass"]
    row["score_max"] = row["coding_total"] + row["tool_total"] + row["agentic_total"]
    return row


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument('--models', nargs='+', required=True)
    parser.add_argument('--base-url', required=True)
    parser.add_argument('--api-key')
    parser.add_argument('--output')
    parser.add_argument('--checkpoint-dir')
    args = parser.parse_args()

    run_started_at = datetime.datetime.now(datetime.timezone.utc)
    if not args.output:
        args.output = os.path.join('.', 'results', f'quality-openai-{run_started_at.strftime("%Y%m%d-%H%M%S")}.json')
    checkpoint_dir = args.checkpoint_dir or os.path.dirname(args.output) or '.'
    host_details = build_host_info()
    results: list[dict[str, Any]] = []
    failed_models: list[dict[str, str]] = []

    for model in args.models:
        try:
            row = run_model(args.base_url, args.api_key, model)
            results.append(row)
            per_model_path = os.path.join(checkpoint_dir, f'quality-{model_slug(model)}.json')
            write_json(per_model_path, {
                'benchmark': 'quality_openai',
                'run_started_at': run_started_at.isoformat(),
                'run_finished_at': datetime.datetime.now(datetime.timezone.utc).isoformat(),
                'output_path': os.path.abspath(per_model_path),
                'base_url': args.base_url,
                'host_details': host_details,
                'models': [model],
                'results': [row],
            })
        except Exception as exc:
            failed_models.append({'model': model, 'error': str(exc)})

        write_json(args.output, {
            'benchmark': 'quality_openai',
            'run_started_at': run_started_at.isoformat(),
            'run_finished_at': datetime.datetime.now(datetime.timezone.utc).isoformat(),
            'output_path': os.path.abspath(args.output),
            'base_url': args.base_url,
            'host_details': host_details,
            'models': args.models,
            'completed_models': [row['model'] for row in results],
            'failed_models': failed_models,
            'incomplete_models': [m for m in args.models if m not in [row['model'] for row in results] and m not in [f['model'] for f in failed_models]],
            'results': results,
        })

    print(json.dumps({
        'benchmark': 'quality_openai',
        'run_started_at': run_started_at.isoformat(),
        'run_finished_at': datetime.datetime.now(datetime.timezone.utc).isoformat(),
        'output_path': os.path.abspath(args.output),
        'base_url': args.base_url,
        'host_details': host_details,
        'models': args.models,
        'completed_models': [row['model'] for row in results],
        'failed_models': failed_models,
        'incomplete_models': [m for m in args.models if m not in [row['model'] for row in results] and m not in [f['model'] for f in failed_models]],
        'results': results,
    }, indent=2))


if __name__ == '__main__':
    main()
