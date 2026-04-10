"""Validators for L4 benchmark questions.

Each validator returns (passed: bool, reason: str).
The `validate` function dispatches on `validator_type`.
"""

import re
from typing import Any


def validate(question: dict, model_response: str) -> tuple[bool, str]:
    """Dispatch to the appropriate validator based on question['validator_type']."""
    vtype = question.get("validator_type", "exact")
    handler = VALIDATORS.get(vtype)
    if not handler:
        return False, f"Unknown validator_type: {vtype}"
    return handler(question, model_response)


def _normalise(text: str) -> str:
    """Strip whitespace, lowercase, remove trailing punctuation."""
    return re.sub(r"[.\s]+$", "", text.strip().lower())


def validate_exact(question: dict, response: str) -> tuple[bool, str]:
    """Exact match after normalisation."""
    expected = question.get("expected_answer", "")
    if not expected:
        return False, "No expected_answer defined"
    norm_resp = _normalise(response)
    norm_exp = _normalise(expected)
    if norm_resp == norm_exp:
        return True, "Exact match"
    # For single-letter multiple choice, accept letter with punctuation
    if len(norm_exp) == 1 and norm_exp.isalpha():
        # Extract first letter from response
        match = re.search(r"\b([a-zA-Z])\b", response.strip())
        if match and match.group(1).lower() == norm_exp:
            return True, "Letter match"
    return False, f"Expected '{expected}', got '{response.strip()[:100]}'"


def validate_logic(question: dict, response: str) -> tuple[bool, str]:
    """Relaxed exact match — strip punctuation, lowercase."""
    expected = question.get("expected_answer", "")
    config = question.get("validator_config", {})
    accepted = config.get("accepted_answers", [expected] if expected else [])
    norm_resp = re.sub(r"[^\w\s]", "", response.strip().lower()).strip()
    for ans in accepted:
        norm_ans = re.sub(r"[^\w\s]", "", ans.strip().lower()).strip()
        if norm_resp == norm_ans:
            return True, f"Logic match: '{ans}'"
    return False, f"Expected one of {accepted}, got '{response.strip()[:100]}'"


def validate_numeric(question: dict, response: str) -> tuple[bool, str]:
    """Extract numeric values and compare within tolerance."""
    config = question.get("validator_config", {})
    expected_str = question.get("expected_answer", "")
    tolerance = float(config.get("tolerance", 0.001))
    pattern = config.get("extract_pattern", r"(-?\d+(?:\.\d+)?)")
    multi = config.get("multi_value", False)

    if multi:
        exp_matches = re.findall(pattern, expected_str)
        resp_matches = re.findall(pattern, response)
        if not resp_matches:
            return False, f"No numbers found in response: '{response.strip()[:100]}'"
        try:
            exp_vals = sorted(float(v) for v in exp_matches)
            resp_vals = sorted(float(v) for v in resp_matches[:len(exp_vals)])
            if len(resp_vals) < len(exp_vals):
                return False, f"Expected {len(exp_vals)} values, found {len(resp_vals)}"
            for ev, rv in zip(exp_vals, resp_vals):
                if abs(ev - rv) > tolerance:
                    return False, f"Value mismatch: expected {ev}, got {rv} (tol={tolerance})"
            return True, f"Numeric match (multi): {resp_vals}"
        except ValueError as e:
            return False, f"Parse error: {e}"
    else:
        match = re.search(pattern, response)
        if not match:
            return False, f"No number found in response: '{response.strip()[:100]}'"
        try:
            resp_val = float(match.group(1) if match.lastindex else match.group(0))
            exp_val = float(re.search(pattern, expected_str).group(1) if re.search(pattern, expected_str).lastindex else re.search(pattern, expected_str).group(0))
            if abs(resp_val - exp_val) <= tolerance:
                return True, f"Numeric match: {resp_val} (expected {exp_val}, tol={tolerance})"
            return False, f"Numeric mismatch: expected {exp_val}, got {resp_val} (tol={tolerance})"
        except (ValueError, AttributeError) as e:
            return False, f"Parse error: {e}"


def validate_regex(question: dict, response: str) -> tuple[bool, str]:
    """Check keyword presence and word count constraints."""
    config = question.get("validator_config", {})
    must_contain = config.get("must_contain_any", [])
    min_words = config.get("min_words", 0)
    max_words = config.get("max_words", 10000)

    words = response.strip().split()
    word_count = len(words)

    if word_count < min_words:
        return False, f"Too few words: {word_count} < {min_words}"
    if word_count > max_words:
        return False, f"Too many words: {word_count} > {max_words}"

    if must_contain:
        resp_lower = response.lower()
        found = [kw for kw in must_contain if kw.lower() in resp_lower]
        if not found:
            return False, f"None of required keywords found: {must_contain}"
        return True, f"Keyword match: {found}"

    return True, "Passed word count check"


def validate_rubric_keywords(question: dict, response: str) -> tuple[bool, str]:
    """Keyword presence + word count (for summarisation tasks)."""
    return validate_regex(question, response)


# Validator registry
VALIDATORS = {
    "exact": validate_exact,
    "logic": validate_logic,
    "numeric": validate_numeric,
    "regex": validate_regex,
    "rubric_keywords": validate_rubric_keywords,
    # "dotnet" is handled separately by the orchestrator via task_runner
}
