#!/usr/bin/env python3
"""Build a completion-style C# training set from The Stack v1 (dedup).

Streams bigcode/the-stack-dedup (C# subset), filters to permissive-licensed
modern C# in a usable size range, extracts function bodies, and writes
chat-format JSONL plus a stratified hold-out split.

Pure helper functions are unit-tested in test_build_stack_csharp_dataset.py.
"""
from __future__ import annotations

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
