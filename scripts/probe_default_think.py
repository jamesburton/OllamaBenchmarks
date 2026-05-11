#!/usr/bin/env python3
"""Probe whether each installed thinking-capable model emits <think> by default.

Two checks:
  1. With NO `think` param sent: does response contain <think>...</think>?
     -> tells us the model's natural default
  2. With `think: False` explicitly: does response still contain <think>?
     -> if YES, model ignores think:false and our 'no-think' baselines are actually think runs
"""
import json
import urllib.error
import urllib.request

MODELS = [
    "gemma4:e2b",
    "gemma4",
    "gemma4:26b",
    "gemma4:31b",
    "qwen3.6",
    "qwen3.6:27b",
    "qwen3:14b",
    "qwen3.5",
    "qwen3.5:4b",
    "qwen3.5:9b",
    "glm-4.7-flash",
    "glm-4.7-flash-reap-toolfix",
    "nemotron-3-nano",
    "nemotron-3-nano:30b-a3b-q8_0",
    "nemotron-3-super",
    "nemotron-nano-9b-v2-toolfix",
    "gpt-oss:20b",
    "gpt-oss:120b",
    "lfm2.5-thinking",
    "lfm2.5-thinking:1.2b",
]

PROMPT = "Add 7 and 11. Reply with just the number, nothing else."


def call(model: str, think_param):
    payload = {
        "model": model,
        "messages": [{"role": "user", "content": PROMPT}],
        "stream": False,
        "options": {"num_predict": 200, "num_ctx": 4096, "temperature": 0, "top_p": 1},
    }
    if think_param is not None:
        payload["think"] = think_param
    req = urllib.request.Request(
        "http://127.0.0.1:11434/api/chat",
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=120) as resp:
            body = json.loads(resp.read().decode("utf-8"))
        msg = body.get("message", {})
        content = msg.get("content", "") or ""
        thinking = msg.get("thinking", "") or ""
        has_think_tags = "<think>" in content
        return {
            "ok": True,
            "thinking_field": bool(thinking),
            "content_has_think_tags": has_think_tags,
            "thinking_len": len(thinking),
            "content_len": len(content),
            "content_preview": content[:80].replace("\n", " "),
        }
    except urllib.error.HTTPError as e:
        try:
            err = json.loads(e.read().decode("utf-8")).get("error", str(e))
        except Exception:
            err = str(e)
        return {"ok": False, "error": err}
    except Exception as e:
        return {"ok": False, "error": str(e)}


def fmt(r):
    if not r["ok"]:
        return f"ERROR: {r['error'][:80]}"
    parts = []
    if r["thinking_field"]:
        parts.append(f"thinking_field={r['thinking_len']}b")
    if r["content_has_think_tags"]:
        parts.append("<think>tags")
    if not parts:
        parts.append("no thinking")
    parts.append(f"content={r['content_len']}b")
    return " ".join(parts) + f"  preview={r['content_preview']!r}"


print(f"{'Model':<40}  {'unset think param':<60}  {'think:false explicit':<60}")
print("-" * 170)

for m in MODELS:
    r_unset = call(m, None)
    r_false = call(m, False)
    print(f"{m:<40}  {fmt(r_unset):<60}  {fmt(r_false):<60}")
