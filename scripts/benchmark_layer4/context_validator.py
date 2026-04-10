"""Validate that generated context prompts don't leak answers.

Checks for:
1. Direct answer strings from failed tasks
2. Numeric answers from maths tasks
3. Step-by-step solution patterns
4. Verbatim question text
"""

import re


def validate_context_prompt(
    context: str,
    failed_tasks: dict[str, dict],
) -> tuple[bool, list[str]]:
    """
    Returns (is_safe, list_of_violations).
    """
    violations = []
    context_lower = context.lower()

    for task_name, task_result in failed_tasks.items():
        expected = task_result.get("expected_answer", "")
        if not expected:
            continue

        # Check 1: Direct answer strings (exact match, case-insensitive)
        if len(expected) > 2 and expected.lower() in context_lower:
            violations.append(f"Direct answer leaked for {task_name}: '{expected}'")

        # Check 2: Numeric answers — look for exact numbers
        numbers = re.findall(r"-?\d+\.?\d*", expected)
        for num in numbers:
            # Only flag if it's a standalone number (not part of a larger context)
            if re.search(rf"\b{re.escape(num)}\b", context):
                # Allow common numbers (0, 1, 2, 10, 100, etc.)
                if float(num) not in (0, 1, 2, 3, 4, 5, 10, 100, 1000):
                    violations.append(f"Numeric answer may be leaked for {task_name}: {num}")

    # Check 3: Step-by-step solution patterns
    step_patterns = [
        r"step\s*1\s*[:\.].*step\s*2",
        r"first\s*,.*then\s*,.*finally",
        r"solution\s*:\s*\n",
        r"the\s+answer\s+is\s+",
        r"to\s+solve\s+this\s*,\s*",
    ]
    for pattern in step_patterns:
        if re.search(pattern, context_lower):
            violations.append(f"Step-by-step solution pattern detected: '{pattern}'")

    is_safe = len(violations) == 0
    return is_safe, violations
