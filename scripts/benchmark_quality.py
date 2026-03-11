import argparse
import json
import os
import re
import subprocess
import tempfile
import textwrap
import urllib.request


CODING_TASKS = [
    (
        "longest_common_prefix",
        "Return only Python code. Define longest_common_prefix(strs: list[str]) -> str. Handle empty list.",
        [
            'assert longest_common_prefix(["flower","flow","flight"])=="fl"',
            'assert longest_common_prefix(["dog","racecar","car"])==""',
            'assert longest_common_prefix([])==""',
        ],
    ),
    (
        "top_k_frequent",
        "Return only Python code. Define top_k_frequent(nums: list[int], k: int) -> list[int]. Sort by frequency desc, tie by smaller number first.",
        [
            "assert top_k_frequent([1,1,1,2,2,3],2)==[1,2]",
            "assert top_k_frequent([4,4,1,1,2,2],2)==[1,2]",
            "assert top_k_frequent([5],1)==[5]",
        ],
    ),
]

TOOL_TASKS = [
    (
        "add_numbers",
        "Call add_numbers with a=17 and b=25. Tool call only.",
        {
            "type": "function",
            "function": {
                "name": "add_numbers",
                "description": "Add two integers",
                "parameters": {
                    "type": "object",
                    "properties": {"a": {"type": "integer"}, "b": {"type": "integer"}},
                    "required": ["a", "b"],
                },
            },
        },
        "add_numbers",
        {"a": 17, "b": 25},
    ),
    (
        "city_timezone",
        'Call city_timezone with city="Tokyo". Tool call only.',
        {
            "type": "function",
            "function": {
                "name": "city_timezone",
                "description": "Get timezone",
                "parameters": {
                    "type": "object",
                    "properties": {"city": {"type": "string"}},
                    "required": ["city"],
                },
            },
        },
        "city_timezone",
        {"city": "Tokyo"},
    ),
]


def post_json(path: str, payload: dict, timeout: int = 1200) -> dict:
    req = urllib.request.Request(
        f"http://127.0.0.1:11434{path}",
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(req, timeout=timeout) as response:
        return json.loads(response.read().decode("utf-8"))


def extract_code(text: str) -> str:
    if not text:
        return ""
    match = re.search(r"```(?:python)?\s*(.*?)```", text, flags=re.S | re.I)
    code = match.group(1) if match else text
    code = textwrap.dedent(code).strip()
    lines = code.splitlines()
    start = 0
    for index, line in enumerate(lines):
        if line.strip().startswith(("def ", "class ", "from ", "import ", "@")):
            start = index
            break
    return "\n".join(lines[start:]).strip()


def run_python_asserts(code: str, asserts: list[str]) -> bool:
    script = code + "\n\n" + "\n".join(asserts) + '\nprint("OK")\n'
    with tempfile.NamedTemporaryFile("w", suffix=".py", delete=False, encoding="utf-8") as handle:
        handle.write(script)
        path = handle.name
    try:
        completed = subprocess.run(
            ["python", path],
            capture_output=True,
            text=True,
            timeout=20,
        )
        return completed.returncode == 0 and "OK" in completed.stdout
    finally:
        try:
            os.unlink(path)
        except OSError:
            pass


def run_model(model: str) -> dict:
    row = {
        "model": model,
        "coding_pass": 0,
        "coding_total": len(CODING_TASKS),
        "tool_pass": 0,
        "tool_total": len(TOOL_TASKS),
    }

    for _, prompt, asserts in CODING_TASKS:
        try:
            response = post_json(
                "/api/generate",
                {
                    "model": model,
                    "prompt": prompt,
                    "stream": False,
                    "options": {"temperature": 0, "num_predict": 220, "seed": 42},
                },
            )
            code = extract_code(response.get("response", ""))
            if run_python_asserts(code, asserts):
                row["coding_pass"] += 1
        except Exception:
            pass

    for _, prompt, tool, expected_name, expected_args in TOOL_TASKS:
        try:
            response = post_json(
                "/api/chat",
                {
                    "model": model,
                    "stream": False,
                    "messages": [{"role": "user", "content": prompt}],
                    "tools": [tool],
                    "options": {"temperature": 0, "num_predict": 100, "seed": 42},
                },
            )
            calls = (response.get("message") or {}).get("tool_calls") or []
            if not calls:
                continue
            function = calls[0].get("function") or {}
            if function.get("name") != expected_name:
                continue
            arguments = function.get("arguments") or {}
            if all(arguments.get(key) == value for key, value in expected_args.items()):
                row["tool_pass"] += 1
        except Exception:
            pass

    row["score"] = row["coding_pass"] + row["tool_pass"]
    row["score_max"] = row["coding_total"] + row["tool_total"]
    return row


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--models", nargs="+", required=True)
    parser.add_argument("--output")
    args = parser.parse_args()

    results = [run_model(model) for model in args.models]
    payload = json.dumps(results, indent=2)
    if args.output:
      os.makedirs(os.path.dirname(args.output) or ".", exist_ok=True)
      with open(args.output, "w", encoding="utf-8") as handle:
          handle.write(payload + "\n")
    print(payload)


if __name__ == "__main__":
    main()

