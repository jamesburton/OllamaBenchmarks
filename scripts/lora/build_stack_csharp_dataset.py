#!/usr/bin/env python3
"""Build a completion-style C# training set from The Stack v1 (dedup).

Streams bigcode/the-stack-dedup (C# subset), filters to permissive-licensed
modern C# in a usable size range, extracts function bodies, and writes
chat-format JSONL plus a stratified hold-out split.

Pure helper functions are unit-tested in test_build_stack_csharp_dataset.py.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import random
import re
import sys
import textwrap
from pathlib import Path

PERMISSIVE_LICENSES = {
    "MIT", "Apache-2.0", "BSD-3-Clause", "BSD-2-Clause", "ISC", "MIT-0",
    "Unlicense", "0BSD", "Apache-2.0+", "BSD",
}

MODERN_MARKERS = ("namespace", "record", "async")

# Iteration-2 quality gates (phase-1 5k regressed base partly on low-quality
# training bodies). Prefer code from more-vetted repos, drop symbol-heavy/junk
# files, and drop minified/generated/data-blob files (very long lines).
MIN_STARS = 5
MIN_ALPHANUM_FRACTION = 0.5
MAX_LINE_LENGTH = 200


def passes_quality(record: dict) -> bool:
    """Repo/file-level quality gate using The Stack metadata fields."""
    stars = record.get("max_stars_count")
    if stars is None or float(stars) < MIN_STARS:
        return False
    af = record.get("alphanum_fraction")
    if af is not None and float(af) < MIN_ALPHANUM_FRACTION:
        return False
    mll = record.get("max_line_length")
    if mll is not None and float(mll) > MAX_LINE_LENGTH:
        return False
    return True


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


# Keywords that may appear as C# modifiers/access specifiers but are never valid
# return types.  When the token immediately before the method name is one of
# these, the "match" is actually a constructor (e.g. `public Person(...)` where
# `public` is mistaken for the return type) — skip it.
_MODIFIER_KEYWORDS = frozenset({
    "public", "private", "protected", "internal",
    "static", "virtual", "override", "sealed", "partial",
    "new", "extern", "unsafe", "async", "abstract",
})

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
    """Return the index just past the matching close brace, or -1.

    Scans character-by-character tracking lexical state so that ``{``/``}``
    inside string literals, char literals, and comments do not affect the brace
    depth counter.  Recognised states:

    - NORMAL        — regular code; ``{``/``}`` change depth.
    - STRING        — inside a double-quoted literal ``"..."``; ``\\`` escapes
                      the next character, including ``\\"``.
    - VERBATIM      — inside a verbatim string ``@"..."``; ``\\`` is not an
                      escape character, but ``""`` is an in-string escaped
                      double-quote (stays in the string).
    - CHAR          — inside a char literal ``'...'``; ``\\`` escapes the next
                      character.
    - LINE_COMMENT  — from ``//`` to end of line.
    - BLOCK_COMMENT — from ``/*`` to ``*/``.
    """
    NORMAL, STRING, VERBATIM, CHAR, LINE_COMMENT, BLOCK_COMMENT = range(6)
    state = NORMAL
    depth = 0
    i = brace_start
    n = len(text)

    while i < n:
        ch = text[i]

        if state == NORMAL:
            if ch == "{":
                depth += 1
            elif ch == "}":
                depth -= 1
                if depth == 0:
                    return i + 1
            elif ch == "/" and i + 1 < n:
                nxt = text[i + 1]
                if nxt == "/":
                    state = LINE_COMMENT
                    i += 2
                    continue
                elif nxt == "*":
                    state = BLOCK_COMMENT
                    i += 2
                    continue
            elif ch == "@" and i + 1 < n and text[i + 1] == '"':
                state = VERBATIM
                i += 2
                continue
            elif ch == '"':
                state = STRING
            elif ch == "'":
                state = CHAR

        elif state == STRING:
            if ch == "\\":
                i += 2  # skip escaped character
                continue
            elif ch == '"':
                state = NORMAL

        elif state == VERBATIM:
            if ch == '"':
                # A doubled quote is an escaped quote — stay in the string.
                if i + 1 < n and text[i + 1] == '"':
                    i += 2
                    continue
                else:
                    state = NORMAL

        elif state == CHAR:
            if ch == "\\":
                i += 2  # skip escaped character
                continue
            elif ch == "'":
                state = NORMAL

        elif state == LINE_COMMENT:
            if ch == "\n":
                state = NORMAL

        elif state == BLOCK_COMMENT:
            if ch == "*" and i + 1 < n and text[i + 1] == "/":
                state = NORMAL
                i += 2
                continue

        i += 1

    return -1


def extract_functions(text: str) -> list[tuple[str, str]]:
    results: list[tuple[str, str]] = []
    for m in _SIG_RE.finditer(text):
        sig = m.group(0).strip()

        # --- Constructor guard ---
        # Split off the parameter list to get the tokens before the first `(`.
        # The regex guarantees `(` is present.  The token immediately before the
        # method name is the return type.  If it is a modifier/access keyword the
        # match is a constructor (e.g. `public Person(...)` where `public` filled
        # the "return type" slot), so we skip it.
        before_paren = sig.split("(")[0]
        tokens = before_paren.split()
        if len(tokens) >= 2 and tokens[-2] in _MODIFIER_KEYWORDS:
            continue

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


def normalize_body(body: str) -> str:
    """Dedent a method body to column 0.

    Stack bodies keep their original class-level indentation, so the first line
    (usually `{`) is unindented but inner lines carry 8-12 spaces. textwrap.dedent
    alone does nothing (no common prefix across all lines). We dedent only the
    inner lines (everything after the opening brace) by their common indentation,
    so the model learns cleanly-formatted output instead of over-indented code.
    """
    lines = body.splitlines()
    if len(lines) <= 1:
        return body.strip()
    first, rest = lines[0], lines[1:]
    dedented_rest = textwrap.dedent("\n".join(rest))
    return (first.strip() + "\n" + dedented_rest).strip()


def to_chat_example(signature: str, body: str) -> dict:
    user = (
        "Implement the following C# method. Return only the complete method.\n\n"
        f"{signature}"
    )
    assistant = f"{signature}\n{normalize_body(body)}"
    return {
        "messages": [
            {"role": "system", "content": SYSTEM_PROMPT},
            {"role": "user", "content": user},
            {"role": "assistant", "content": assistant},
        ]
    }


def content_key(example: dict) -> str:
    """Return a normalized hash key (sha1 of assistant content with whitespace collapsed)."""
    assistant = example["messages"][-1]["content"]
    normalized = " ".join(assistant.split())
    return hashlib.sha1(normalized.encode("utf-8")).hexdigest()


def dedup(examples: list[dict]) -> list[dict]:
    """Keep first occurrence per content_key, preserve order."""
    seen: set[str] = set()
    out: list[dict] = []
    for ex in examples:
        k = content_key(ex)
        if k not in seen:
            seen.add(k)
            out.append(ex)
    return out


def split_holdout(
    examples: list[dict], fraction: float = 0.10, seed: int = 42
) -> tuple[list[dict], list[dict]]:
    """Return (train, holdout) with holdout size = round(len*fraction), seeded shuffle, deterministic."""
    idx = list(range(len(examples)))
    random.Random(seed).shuffle(idx)
    n_hold = round(len(examples) * fraction)
    hold_idx = set(idx[:n_hold])
    train = [ex for i, ex in enumerate(examples) if i not in hold_idx]
    holdout = [examples[i] for i in idx[:n_hold]]
    return train, holdout


def build(target: int, limit: int | None, seed: int):
    from datasets import load_dataset

    ds = load_dataset(
        "bigcode/the-stack-dedup",
        data_dir="data/c-sharp",
        split="train",
        streaming=True,
    )

    examples: list[dict] = []
    scanned = 0
    for record in ds:
        scanned += 1
        if limit is not None and scanned > limit:
            break
        text = record.get("content") or ""
        if not passes_license(record):
            continue
        if not passes_quality(record):
            continue
        if not passes_size(text):
            continue
        if not is_modern_csharp(text):
            continue
        for sig, body in extract_functions(text):
            if not passes_size(body, lo=40, hi=6000):
                continue
            examples.append(to_chat_example(sig, body))
        if len(examples) >= target * 3:  # over-collect; dedup trims later
            break

    examples = dedup(examples)
    if len(examples) > target:
        examples = examples[:target]
    print(f"scanned={scanned} kept={len(examples)}")
    return split_holdout(examples, fraction=0.10, seed=seed)


def write_jsonl(path: Path, rows: list[dict]):
    path.parent.mkdir(parents=True, exist_ok=True)
    with open(path, "w", encoding="utf-8") as fh:
        for row in rows:
            fh.write(json.dumps(row, ensure_ascii=False) + "\n")


def main() -> int:
    ap = argparse.ArgumentParser(description="Build Stack v1 C# training set")
    ap.add_argument("--target", type=int, default=5000)
    ap.add_argument("--out-dir", type=Path, default=Path(__file__).parent / "data")
    ap.add_argument("--limit", type=int, default=None,
                    help="max raw files to scan (for smoke runs)")
    ap.add_argument("--seed", type=int, default=42)
    args = ap.parse_args()

    train, holdout = build(args.target, args.limit, args.seed)
    write_jsonl(args.out_dir / "stack_csharp_train.jsonl", train)
    write_jsonl(args.out_dir / "stack_csharp_holdout.jsonl", holdout)
    print(f"train={len(train)} holdout={len(holdout)} -> {args.out_dir}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
