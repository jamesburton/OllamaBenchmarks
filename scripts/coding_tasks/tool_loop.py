"""Generic tool-calling / ReAct driver over Ollama ``/api/chat``.

Shared by the Layer 4 tool-use (T1/T2) and agentic-C# (T3) harnesses. A
:class:`Tool` bundles a JSON-schema function spec with a Python callable that
executes it; :func:`run_tool_loop` runs the
``prompt -> tool_calls -> execute -> feed-result`` cycle until the model calls a
terminal tool (e.g. ``finish``) or a step cap is hit, capturing a transcript.
:func:`request_tool_calls` is the single-turn variant used by T1 correctness
checks (inspect the emitted call without executing).

Honors ``OLLAMA_HOST`` so generation can target a remote GPU (T5500/Strix) while
this process runs on another box. Tool-call format confirmed for qwen3.5 (see
docs/agentic-tool-longcontext-benchmark-plan.md): structured
``message.tool_calls[].function.{name, arguments}`` with ``arguments`` already
an object.
"""

import json
import os
import time
import urllib.error
import urllib.request
from dataclasses import dataclass, field
from typing import Any, Callable

OLLAMA_HOST = os.environ.get("OLLAMA_HOST", "http://127.0.0.1:11434").rstrip("/")


def post_chat(payload: dict, timeout: int = 600, max_retries: int = 5) -> dict:
    """POST /api/chat with 429 backoff. Mirrors benchmark_quality.post_json."""
    data = json.dumps(payload).encode("utf-8")
    for attempt in range(max_retries):
        req = urllib.request.Request(
            f"{OLLAMA_HOST}/api/chat",
            data=data,
            headers={"Content-Type": "application/json"},
            method="POST",
        )
        try:
            with urllib.request.urlopen(req, timeout=timeout) as resp:
                return json.loads(resp.read().decode("utf-8"))
        except urllib.error.HTTPError as exc:
            if exc.code == 429 and attempt < max_retries - 1:
                wait = min(30 * (2 ** attempt), 300)
                print(f"    [429] waiting {wait}s (attempt {attempt+1}/{max_retries})")
                time.sleep(wait)
                continue
            raise
    return {}


def parse_arguments(arguments: Any) -> dict[str, Any]:
    """Normalise a tool-call ``arguments`` value to a dict (object or JSON str)."""
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


@dataclass
class Tool:
    """A callable tool plus its JSON-schema spec for the ``tools`` parameter."""

    name: str
    description: str
    parameters: dict
    func: Callable[[dict], Any]

    def spec(self) -> dict:
        """Return the Ollama/OpenAI ``tools`` entry for this tool."""
        return {
            "type": "function",
            "function": {
                "name": self.name,
                "description": self.description,
                "parameters": self.parameters,
            },
        }


@dataclass
class StepRecord:
    """One executed tool call within a loop (or a terminal finish)."""

    tool: str
    arguments: dict
    result: Any = None
    error: str | None = None


@dataclass
class LoopResult:
    """Outcome of :func:`run_tool_loop`."""

    finished: bool
    final_args: dict | None
    steps: list[StepRecord]
    transcript: list[dict]
    stop_reason: str  # finish | step_cap | no_tool_call


def request_tool_calls(
    model: str,
    user: str,
    tool_specs: list[dict],
    *,
    system: str | None = None,
    think: Any = False,
    num_predict: int = 256,
    seed: int = 42,
    options: dict | None = None,
    timeout: int = 300,
) -> tuple[dict, list[dict]]:
    """Single-turn: send one prompt, return (assistant_message, tool_calls).

    Used by T1 correctness checks — the caller asserts on the emitted call(s)
    without executing them.
    """
    messages: list[dict] = []
    if system:
        messages.append({"role": "system", "content": system})
    messages.append({"role": "user", "content": user})
    resp = post_chat(
        {
            "model": model,
            "stream": False,
            "messages": messages,
            "tools": tool_specs,
            "think": think,
            "options": {**(options or {"temperature": 0, "top_p": 1}),
                        "num_predict": num_predict, "seed": seed},
        },
        timeout=timeout,
    )
    message = resp.get("message") or {}
    return message, (message.get("tool_calls") or [])


def run_tool_loop(
    model: str,
    user: str,
    tools: list[Tool],
    *,
    system: str | None = None,
    finish_tool: str = "finish",
    max_steps: int = 8,
    think: Any = False,
    num_predict: int = 512,
    seed: int = 42,
    options: dict | None = None,
    timeout: int = 600,
    on_step: Callable[[StepRecord], None] | None = None,
) -> LoopResult:
    """Run the ReAct loop until ``finish_tool`` is called or ``max_steps`` hit.

    Parallel tool calls in a single turn are all executed and fed back in order.
    A tool that raises (or an unknown tool name) feeds an ``{"error": ...}``
    result back so the model can recover, and is recorded with ``error`` set.
    """
    registry = {t.name: t for t in tools}
    specs = [t.spec() for t in tools]
    messages: list[dict] = []
    if system:
        messages.append({"role": "system", "content": system})
    messages.append({"role": "user", "content": user})

    steps: list[StepRecord] = []
    for _ in range(max_steps):
        resp = post_chat(
            {
                "model": model,
                "stream": False,
                "messages": messages,
                "tools": specs,
                "think": think,
                "options": {**(options or {"temperature": 0, "top_p": 1}),
                            "num_predict": num_predict, "seed": seed},
            },
            timeout=timeout,
        )
        msg = resp.get("message") or {}
        calls = msg.get("tool_calls") or []
        # Echo the assistant turn back into the transcript (with its tool_calls).
        assistant_msg: dict = {"role": "assistant", "content": msg.get("content", "")}
        if calls:
            assistant_msg["tool_calls"] = calls
        messages.append(assistant_msg)

        if not calls:
            return LoopResult(False, None, steps, messages, "no_tool_call")

        for call in calls:
            fn = call.get("function") or {}
            name = fn.get("name")
            args = parse_arguments(fn.get("arguments"))

            if name == finish_tool:
                rec = StepRecord(name, args, result="<finish>")
                steps.append(rec)
                if on_step:
                    on_step(rec)
                return LoopResult(True, args, steps, messages, "finish")

            tool = registry.get(name)
            if tool is None:
                rec = StepRecord(name, args, error=f"unknown tool '{name}'")
                result: Any = {"error": rec.error}
            else:
                try:
                    result = tool.func(args)
                    rec = StepRecord(name, args, result=result)
                except Exception as exc:  # surface to the model, keep looping
                    rec = StepRecord(name, args, error=str(exc))
                    result = {"error": str(exc)}
            steps.append(rec)
            if on_step:
                on_step(rec)
            messages.append({
                "role": "tool",
                "tool_name": name,
                "content": json.dumps(result, ensure_ascii=False),
            })

    return LoopResult(False, None, steps, messages, "step_cap")
