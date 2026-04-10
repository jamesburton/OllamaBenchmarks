"""Non-dotnet task runner for L4 benchmark.

Handles English, Maths, and Logic questions that don't need dotnet compilation.
Delegates to validators.py for answer checking.
"""

import dataclasses
import json
import os
import re
import sys
import time
import urllib.request
from typing import Any

# Add parent scripts dir to path for imports
sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
from coding_tasks.task_runner import call_ollama, call_llama_server, model_slug
from benchmark_layer4.validators import validate


@dataclasses.dataclass
class L4TaskResult:
    task: str
    category: str
    subcategory: str
    difficulty: str
    weight: int
    passed: bool
    validator_type: str
    harness_error: str | None
    model_response: str
    expected_answer: str | None
    validation_reason: str
    generation_time_s: float


def run_non_dotnet_task(
    task_def: dict,
    model: str,
    context_prompt: str | None = None,
    timeout: int = 120,
) -> L4TaskResult:
    """Call model with the question prompt, validate the response."""
    task_name = task_def.get("name", "unknown")
    category = task_def.get("category", "unknown")
    subcategory = task_def.get("subcategory", "")
    difficulty = task_def.get("difficulty", "medium")
    weight = task_def.get("weight", 1)
    validator_type = task_def.get("validator_type", "exact")
    max_tokens = task_def.get("max_tokens", 512)
    prompt = task_def.get("prompt", "")

    # If context_prompt provided, temporarily override system prompt
    if context_prompt:
        from coding_tasks.task_runner import _load_prompt_config
        config_dir = os.path.join(os.path.dirname(__file__), "..", "prompt_configs")
        os.makedirs(config_dir, exist_ok=True)
        slug = re.sub(r"[^\w\.-]", "_", model.replace(":", "_").replace("/", "_"))
        config_path = os.path.join(config_dir, f"{slug}.json")
        # Save existing config
        old_config = None
        if os.path.isfile(config_path):
            old_config = json.loads(open(config_path).read())
        # Write context config
        with open(config_path, "w") as f:
            json.dump({"model": model, "system_prompt": context_prompt}, f)

    try:
        t0 = time.monotonic()
        llama_url = os.environ.get("LLAMA_SERVER_URL")
        if llama_url:
            raw = call_llama_server(model, prompt, max_tokens=max_tokens, timeout=timeout, base_url=llama_url)
        else:
            raw = call_ollama(model, prompt, max_tokens=max_tokens, timeout=timeout)
        gen_time = time.monotonic() - t0

        if not raw.strip():
            return L4TaskResult(
                task=task_name, category=category, subcategory=subcategory,
                difficulty=difficulty, weight=weight, passed=False,
                validator_type=validator_type, harness_error="Empty response",
                model_response="", expected_answer=task_def.get("expected_answer"),
                validation_reason="Empty response from model",
                generation_time_s=gen_time,
            )

        passed, reason = validate(task_def, raw)
        return L4TaskResult(
            task=task_name, category=category, subcategory=subcategory,
            difficulty=difficulty, weight=weight, passed=passed,
            validator_type=validator_type, harness_error=None,
            model_response=raw.strip()[:500],
            expected_answer=task_def.get("expected_answer"),
            validation_reason=reason,
            generation_time_s=gen_time,
        )

    except Exception as exc:
        return L4TaskResult(
            task=task_name, category=category, subcategory=subcategory,
            difficulty=difficulty, weight=weight, passed=False,
            validator_type=validator_type, harness_error=str(exc)[:200],
            model_response="", expected_answer=task_def.get("expected_answer"),
            validation_reason=f"Error: {exc}",
            generation_time_s=0,
        )

    finally:
        # Restore original config
        if context_prompt:
            if old_config:
                with open(config_path, "w") as f:
                    json.dump(old_config, f)
            elif os.path.isfile(config_path):
                os.unlink(config_path)
