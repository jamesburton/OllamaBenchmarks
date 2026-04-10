"""Generate context prompts from failure analysis using a frontier model.

Analyses which tasks failed per model/category, then asks a frontier model
to produce generalised guidance (no direct answers) as a system prompt.
"""

import json
import os
import sys
from collections import Counter

sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
from coding_tasks.task_runner import call_ollama


CONTEXT_GENERATION_PROMPT = """You are a benchmark advisor. Analyse these failure patterns and produce
a system prompt that provides generalised guidance to help a language model
perform better on this category of questions.

CRITICAL RULES:
- Do NOT include any specific answers to any of the failed questions
- Do NOT include worked examples that directly solve any failed question
- Do NOT include step-by-step solutions to specific problem types
- Do NOT quote any of the failed question prompts verbatim
- Only provide: conceptual reminders, notation conventions, common error warnings,
  and general approach guidance
- Keep the system prompt under 500 words

Category: {category}
Total failed: {total_failed} out of {total_tasks}

Failure patterns by subcategory:
{subcategory_breakdown}

Failure patterns by difficulty:
{difficulty_breakdown}

Sample failure categories (no answers included):
{sample_failures}

Output only the system prompt text, nothing else."""


def analyse_failures(checkpoint_path: str) -> dict:
    """Load checkpoint, group failed tasks by subcategory and difficulty."""
    with open(checkpoint_path, "r") as f:
        data = json.load(f)

    completed = data.get("completed_tasks", {})
    if not completed:
        return {"total_failed": 0, "total_tasks": 0}

    failed = {k: v for k, v in completed.items() if not v.get("passed")}
    total = len(completed)

    by_sub = Counter(v.get("subcategory", "unknown") for v in failed.values())
    by_diff = Counter(v.get("difficulty", "unknown") for v in failed.values())

    # Sample failed task descriptions (prompts, not answers)
    samples = []
    for name, result in list(failed.items())[:10]:
        samples.append(f"- {name} ({result.get('subcategory', '?')}, {result.get('difficulty', '?')}): {result.get('validation_reason', '?')[:80]}")

    return {
        "total_failed": len(failed),
        "total_tasks": total,
        "by_subcategory": dict(by_sub.most_common()),
        "by_difficulty": dict(by_diff.most_common()),
        "sample_failures": samples,
        "failed_tasks": failed,
    }


def generate_context_prompt(
    category: str,
    failure_analysis: dict,
    frontier_model: str = "glm-5:cloud",
) -> str:
    """Call frontier model to generate context guidance prompt."""
    if failure_analysis["total_failed"] == 0:
        return ""

    prompt = CONTEXT_GENERATION_PROMPT.format(
        category=category,
        total_failed=failure_analysis["total_failed"],
        total_tasks=failure_analysis["total_tasks"],
        subcategory_breakdown=json.dumps(failure_analysis["by_subcategory"], indent=2),
        difficulty_breakdown=json.dumps(failure_analysis["by_difficulty"], indent=2),
        sample_failures="\n".join(failure_analysis["sample_failures"]),
    )

    response = call_ollama(frontier_model, prompt, max_tokens=1024, timeout=120)
    return response.strip()
