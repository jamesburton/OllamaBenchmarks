"""
Code extractor utilities for the coding benchmark.

Strips markdown fences and extracts clean code from LLM responses.
"""

import re
import textwrap


# C# keywords / tokens that indicate the start of real code
_CSHARP_START_TOKENS = (
    "using ",
    "namespace ",
    "public ",
    "internal ",
    "private ",
    "protected ",
    "class ",
    "record ",
    "struct ",
    "interface ",
    "global ",
    "[",
)

# Python keywords that indicate the start of real code
_PYTHON_START_TOKENS = ("def ", "class ", "from ", "import ", "@")


def extract_csharp(text: str) -> str:
    """Extract clean C# code from an LLM response.

    Strips markdown fences (```csharp, ```cs, ```c#, bare ```) and removes
    leading prose lines before the first recognisable C# token.  If multiple
    fenced blocks are present they are concatenated with a newline separator.
    When no fences are found the full text is treated as raw code.

    Returns a dedented, stripped string.
    """
    if not text:
        return ""

    # Find all fenced code blocks (csharp / cs / c# / bare)
    blocks = re.findall(
        r"```(?:csharp|cs|c#)?\s*\n?(.*?)```",
        text,
        flags=re.S | re.I,
    )

    if blocks:
        code = "\n".join(blocks)
    else:
        code = text

    code = textwrap.dedent(code).strip()

    # Strip leading prose lines before the first C# token
    lines = code.splitlines()
    start = 0
    for index, line in enumerate(lines):
        stripped = line.strip()
        if stripped.startswith(_CSHARP_START_TOKENS):
            start = index
            break

    return "\n".join(lines[start:]).strip()


def extract_python(text: str) -> str:
    """Extract clean Python code from an LLM response.

    Strips markdown fences (```python / bare ```) and removes leading prose
    lines before the first recognisable Python token (def / class / from /
    import / @).

    Returns a dedented, stripped string.
    """
    if not text:
        return ""

    match = re.search(r"```(?:python)?\s*(.*?)```", text, flags=re.S | re.I)
    code = match.group(1) if match else text

    code = textwrap.dedent(code).strip()

    lines = code.splitlines()
    start = 0
    for index, line in enumerate(lines):
        if line.strip().startswith(_PYTHON_START_TOKENS):
            start = index
            break

    return "\n".join(lines[start:]).strip()
