#!/usr/bin/env python3
"""Probe which installed Ollama models support `think:true`.

Sends a tiny chat completion with think:true; if Ollama rejects with
'does not support thinking', the model is skipped.
"""
import json
import sys
import urllib.error
import urllib.request

MODELS = [
    "gemma4:e2b",
    "granite4:7b-a1b-h",
    "granite4:32b-a9b-h",
    "qwen3.6",
    "qwen3.6:27b",
    "qwen3-coder-next",
    "qwen3:14b",
    "glm-4.7-flash",
    "nemotron-3-nano:30b-a3b-q8_0",
    "gpt-oss:20b",
    "nemotron-3-super",
]


def supports_think(model: str) -> tuple[bool, str]:
    payload = {
        "model": model,
        "messages": [{"role": "user", "content": "hi"}],
        "stream": False,
        "think": True,
        "options": {"num_predict": 1},
    }
    req = urllib.request.Request(
        "http://127.0.0.1:11434/api/chat",
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=60) as resp:
            body = json.loads(resp.read().decode("utf-8"))
        return True, "ok"
    except urllib.error.HTTPError as e:
        try:
            err = json.loads(e.read().decode("utf-8")).get("error", str(e))
        except Exception:
            err = str(e)
        return False, err
    except Exception as e:
        return False, str(e)


for m in MODELS:
    ok, info = supports_think(m)
    print(f"{'YES' if ok else 'NO ':>3} {m}  -> {info[:120]}")
