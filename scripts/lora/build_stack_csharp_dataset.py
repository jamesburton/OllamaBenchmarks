#!/usr/bin/env python3
"""Build a completion-style C# training set from The Stack v1 (dedup).

Streams bigcode/the-stack-dedup (C# subset), filters to permissive-licensed
modern C# in a usable size range, extracts function bodies, and writes
chat-format JSONL plus a stratified hold-out split.

Pure helper functions are unit-tested in test_build_stack_csharp_dataset.py.
"""
from __future__ import annotations

import re

PERMISSIVE_LICENSES = {
    "MIT", "Apache-2.0", "BSD-3-Clause", "BSD-2-Clause", "ISC", "MIT-0",
    "Unlicense", "0BSD", "Apache-2.0+", "BSD",
}

MODERN_MARKERS = ("namespace", "record", "async")


def passes_license(record: dict) -> bool:
    val = record.get("license")
    if val is None:
        val = record.get("max_stars_repo_licenses")
    if val is None:
        return False
    licenses = val if isinstance(val, (list, tuple)) else [val]
    return any(str(lic).strip() in PERMISSIVE_LICENSES for lic in licenses)


def passes_size(text: str, lo: int = 200, hi: int = 8192) -> bool:
    n = len(text.encode("utf-8"))
    return lo <= n <= hi


def is_modern_csharp(text: str) -> bool:
    return any(marker in text for marker in MODERN_MARKERS)


SYSTEM_PROMPT = (
    "You are an expert C#/.NET developer. When asked to write code, "
    "return ONLY valid C# code in a single file. Do not include markdown "
    "fences, explanations, or commentary — just the raw C# source code."
)

# A C# method signature: optional modifiers, a return type, a name, a
# parenthesised parameter list, then an opening brace on the same or next line.
_SIG_RE = re.compile(
    r"^[ \t]*"
    r"(?:(?:public|private|protected|internal|static|async|virtual|override|sealed|"
    r"partial|new|extern|unsafe)\s+)*"
    r"[\w<>\[\],\.\?]+\s+"          # return type
    r"[A-Za-z_]\w*\s*"              # method name
    r"\([^;{]*\)\s*"               # parameter list (no ; — excludes declarations)
    r"$",
    re.MULTILINE,
)


def _match_block(text: str, brace_start: int) -> int:
    """Return the index just past the matching close brace, or -1."""
    depth = 0
    for i in range(brace_start, len(text)):
        if text[i] == "{":
            depth += 1
        elif text[i] == "}":
            depth -= 1
            if depth == 0:
                return i + 1
    return -1


def extract_functions(text: str) -> list[tuple[str, str]]:
    results: list[tuple[str, str]] = []
    for m in _SIG_RE.finditer(text):
        sig = m.group(0).strip()
        # Find the next '{' after the signature.
        rest = text[m.end():]
        brace_rel = rest.find("{")
        if brace_rel == -1:
            continue
        brace_abs = m.end() + brace_rel
        end = _match_block(text, brace_abs)
        if end == -1:
            continue
        body = text[brace_abs:end]
        results.append((sig, body))
    return results


def to_chat_example(signature: str, body: str) -> dict:
    user = (
        "Implement the following C# method. Return only the complete method.\n\n"
        f"{signature}"
    )
    assistant = f"{signature}\n{body}"
    return {
        "messages": [
            {"role": "system", "content": SYSTEM_PROMPT},
            {"role": "user", "content": user},
            {"role": "assistant", "content": assistant},
        ]
    }
