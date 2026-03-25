"""
Multimodal vision benchmark for Ollama models.

Sends images + prompts via Ollama /api/chat with base64-encoded images,
then scores text responses against known ground truth across 6 categories:
OCR, Document Understanding, Chart Reading, Visual QA, Code from Visual, Math.
"""

import argparse
import base64
import datetime
import json
import os
import re
import time
import urllib.request


# ---------------------------------------------------------------------------
# Helpers (matching conventions from task_runner.py / benchmark_quality.py)
# ---------------------------------------------------------------------------

def model_slug(model: str) -> str:
    model = re.sub(r":latest$", "", model)
    return re.sub(
        r"[^\w\.-]", "_",
        model.replace(":", "_").replace("/", "_").replace("\\", "_"),
    )


def sampling_options(model: str) -> dict:
    if model.startswith(("nemotron-3-super", "nemotron-3-nano")):
        return {"temperature": 1.0, "top_p": 1.0}
    return {"temperature": 0, "top_p": 1}


def write_json(path: str, payload: dict) -> None:
    os.makedirs(os.path.dirname(path) or ".", exist_ok=True)
    with open(path, "w", encoding="utf-8") as fh:
        fh.write(json.dumps(payload, indent=2) + "\n")


def encode_image(path: str) -> str:
    with open(path, "rb") as fh:
        return base64.b64encode(fh.read()).decode("utf-8")


def call_ollama_vision(
    model: str,
    prompt: str,
    image_b64: str,
    max_tokens: int = 2048,
    timeout: int = 600,
) -> str:
    """POST to Ollama /api/chat with an image. Returns generated text."""
    options = sampling_options(model)
    payload = {
        "model": model,
        "messages": [{
            "role": "user",
            "content": prompt,
            "images": [image_b64],
        }],
        "stream": False,
        "options": {
            "num_predict": max_tokens,
            "temperature": options["temperature"],
            "top_p": options["top_p"],
            "seed": 42,
        },
    }
    data = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(
        "http://127.0.0.1:11434/api/chat",
        data=data,
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=timeout) as resp:
            body = json.loads(resp.read().decode("utf-8"))
        content = body.get("message", {}).get("content", "")
        # Strip thinking tags if present
        if "<think>" in content:
            content = re.sub(r"<think>.*?</think>", "", content, flags=re.S).strip()
        return content
    except (urllib.error.URLError, OSError, TimeoutError, KeyError, IndexError) as exc:
        print(f"    [call_ollama_vision] Error: {type(exc).__name__}: {exc}")
        return ""


# ---------------------------------------------------------------------------
# Scoring functions
# ---------------------------------------------------------------------------

def _levenshtein(a: str, b: str) -> int:
    """Compute Levenshtein edit distance between two strings."""
    if len(a) < len(b):
        return _levenshtein(b, a)
    if len(b) == 0:
        return len(a)
    prev_row = list(range(len(b) + 1))
    for i, ca in enumerate(a):
        curr_row = [i + 1]
        for j, cb in enumerate(b):
            cost = 0 if ca == cb else 1
            curr_row.append(min(
                curr_row[j] + 1,
                prev_row[j + 1] + 1,
                prev_row[j] + cost,
            ))
        prev_row = curr_row
    return prev_row[-1]


def score_ocr(response: str, ground_truth: str) -> float:
    """Normalized edit distance score. 1.0 = perfect match."""
    resp_clean = " ".join(response.split())
    gt_clean = " ".join(ground_truth.split())
    max_len = max(len(resp_clean), len(gt_clean), 1)
    dist = _levenshtein(resp_clean.lower(), gt_clean.lower())
    return max(0.0, 1.0 - dist / max_len)


def score_fields(response: str, required_fields: list[str]) -> float:
    """Check if required field values appear in the response."""
    resp_lower = response.lower()
    found = sum(1 for f in required_fields if f.lower() in resp_lower)
    return found / max(len(required_fields), 1)


def score_numeric_values(response: str, values: list[float], tolerance: float = 0.05) -> float:
    """Check if numeric values appear in the response within tolerance."""
    # Extract all numbers from response
    numbers = re.findall(r"[\d]+(?:\.[\d]+)?", response)
    response_nums = [float(n) for n in numbers]

    found = 0
    for expected in values:
        for actual in response_nums:
            if expected == 0:
                if actual == 0:
                    found += 1
                    break
            elif abs(actual - expected) / max(abs(expected), 1e-9) <= tolerance:
                found += 1
                break
    return found / max(len(values), 1)


def score_terms(response: str, terms: list[str]) -> float:
    """Check if key terms appear in response."""
    resp_lower = response.lower()
    found = sum(1 for t in terms if t.lower() in resp_lower)
    return found / max(len(terms), 1)


def score_binary(response: str, expected: list[str]) -> float:
    """Binary: 1.0 if all expected items found, else 0.0."""
    resp_lower = response.lower()
    return 1.0 if all(e.lower() in resp_lower for e in expected) else 0.0


# ---------------------------------------------------------------------------
# Task definitions
# ---------------------------------------------------------------------------

# Import ground truth data from the generator module
from generate_multimodal_assets import (
    OCR_CODE_TEXT,
    OCR_MIXED_FONTS_FULL,
    OCR_NUMBERS_FLAT,
    OCR_HANDWRITING_TEXT,
    INVOICE_DATA,
    TABLE_HEADERS,
    TABLE_ROWS,
    FORM_FIELDS,
    BAR_LABELS,
    BAR_VALUES,
    PIE_LABELS,
    PIE_VALUES,
    LINE_X,
    LINE_Y,
)

TASKS = [
    # --- OCR ---
    {
        "task": "ocr_code",
        "category": "ocr",
        "image": "ocr_code.png",
        "prompt": "Read and transcribe the code shown in this image exactly as written. Return only the code text.",
        "scorer": lambda resp: score_ocr(resp, OCR_CODE_TEXT),
        "pass_threshold": 0.8,
    },
    {
        "task": "ocr_mixed_fonts",
        "category": "ocr",
        "image": "ocr_mixed_fonts.png",
        "prompt": "Read all the text in this image, including the header and body. Return the full text.",
        "scorer": lambda resp: score_ocr(resp, OCR_MIXED_FONTS_FULL),
        "pass_threshold": 0.8,
    },
    {
        "task": "ocr_numbers",
        "category": "ocr",
        "image": "ocr_numbers.png",
        "prompt": "Read all numbers from this table, row by row, left to right, top to bottom. Return the numbers separated by spaces.",
        "scorer": lambda resp: score_ocr(resp, OCR_NUMBERS_FLAT),
        "pass_threshold": 0.8,
    },
    {
        "task": "ocr_handwriting_style",
        "category": "ocr",
        "image": "ocr_handwriting_style.png",
        "prompt": "Read the handwritten-style text in this image. Return the exact text.",
        "scorer": lambda resp: score_ocr(resp, OCR_HANDWRITING_TEXT),
        "pass_threshold": 0.8,
    },
    # --- Document Understanding ---
    {
        "task": "doc_invoice",
        "category": "document",
        "image": "doc_invoice.png",
        "prompt": "Extract all information from this invoice: company name, date, each line item (description, quantity, price), and total. Return as structured text.",
        "scorer": lambda resp: score_fields(resp, [
            INVOICE_DATA["company"],
            INVOICE_DATA["date"],
            "Widget A", "Widget B", "Service Fee",
            "10", "5", "1",
            "5.00", "12.50", "25.00",
            "137.50",
        ]),
        "pass_threshold": 0.7,
    },
    {
        "task": "doc_table",
        "category": "document",
        "image": "doc_table.png",
        "prompt": "Extract all data from this table including headers. Return each row's values.",
        "scorer": lambda resp: score_fields(
            resp,
            TABLE_HEADERS + [val for row in TABLE_ROWS for val in row],
        ),
        "pass_threshold": 0.7,
    },
    {
        "task": "doc_form",
        "category": "document",
        "image": "doc_form.png",
        "prompt": "Extract all field names and their filled-in values from this form.",
        "scorer": lambda resp: score_fields(
            resp,
            list(FORM_FIELDS.keys()) + list(FORM_FIELDS.values()),
        ),
        "pass_threshold": 0.7,
    },
    # --- Chart Reading ---
    {
        "task": "chart_bar",
        "category": "chart",
        "image": "chart_bar.png",
        "prompt": "Read the bar chart and list each bar's label and value.",
        "scorer": lambda resp: score_numeric_values(resp, [float(v) for v in BAR_VALUES]),
        "pass_threshold": 0.7,
    },
    {
        "task": "chart_pie",
        "category": "chart",
        "image": "chart_pie.png",
        "prompt": "Read the pie chart and list each segment's label and percentage.",
        "scorer": lambda resp: score_numeric_values(resp, [float(v) for v in PIE_VALUES]),
        "pass_threshold": 0.7,
    },
    {
        "task": "chart_line",
        "category": "chart",
        "image": "chart_line.png",
        "prompt": "Read the line chart and list each data point's x and y values.",
        "scorer": lambda resp: score_numeric_values(resp, [float(v) for v in LINE_Y]),
        "pass_threshold": 0.7,
    },
    # --- Visual QA ---
    {
        "task": "vqa_shapes",
        "category": "vqa",
        "image": "vqa_shapes.png",
        "prompt": "How many shapes are in this image? What color is the circle? What are all the shapes?",
        "scorer": lambda resp: score_terms(resp, ["3", "three", "red", "circle", "square", "triangle"]),
        "pass_threshold": 0.6,
    },
    {
        "task": "vqa_layout",
        "category": "vqa",
        "image": "vqa_layout.png",
        "prompt": "Describe the layout of this UI wireframe. What sections are visible?",
        "scorer": lambda resp: score_terms(resp, ["header", "sidebar", "content", "menu", "navigation"]),
        "pass_threshold": 0.5,
    },
    {
        "task": "vqa_diagram",
        "category": "vqa",
        "image": "vqa_diagram.png",
        "prompt": "What are the steps in this flowchart? List them in order.",
        "scorer": lambda resp: score_terms(resp, ["start", "process", "decision", "end"]),
        "pass_threshold": 0.7,
    },
    # --- Code from Visual ---
    {
        "task": "code_ui_buttons",
        "category": "code",
        "image": "code_ui_buttons.png",
        "prompt": "Generate HTML code that recreates this UI with the three buttons shown. Include appropriate styling.",
        "scorer": lambda resp: score_terms(resp, [
            "save", "cancel", "delete", "button", "<"
        ]),
        "pass_threshold": 0.7,
    },
    {
        "task": "code_class_diagram",
        "category": "code",
        "image": "code_class_diagram.png",
        "prompt": "Generate a C# class from this UML class diagram. Include all properties and methods shown.",
        "scorer": lambda resp: score_terms(resp, [
            "UserProfile", "Name", "Email", "Age",
            "GetDisplayName", "IsValid", "UpdateEmail",
        ]),
        "pass_threshold": 0.7,
    },
    # --- Math ---
    {
        "task": "math_equation",
        "category": "math",
        "image": "math_equation.png",
        "prompt": "What equation is shown in this image? What is the result?",
        "scorer": lambda resp: score_binary(resp, ["1/3"]),
        "pass_threshold": 1.0,
    },
    {
        "task": "math_expression",
        "category": "math",
        "image": "math_expression.png",
        "prompt": "What system of equations is shown? Solve for x and y.",
        "scorer": lambda resp: score_terms(resp, ["2", "1"]),
        "pass_threshold": 0.7,
    },
]


# ---------------------------------------------------------------------------
# Main benchmark
# ---------------------------------------------------------------------------

def ensure_assets(asset_dir: str) -> None:
    """Generate test images if not already present."""
    expected = [t["image"] for t in TASKS]
    missing = [f for f in expected if not os.path.isfile(os.path.join(asset_dir, f))]
    if missing:
        print(f"[multimodal] Generating {len(missing)} missing asset(s)...")
        from generate_multimodal_assets import generate_all
        generate_all(asset_dir)


def run_task(model: str, task_def: dict, asset_dir: str) -> dict:
    """Run a single multimodal task and return the result dict."""
    image_path = os.path.join(asset_dir, task_def["image"])
    image_b64 = encode_image(image_path)

    t0 = time.monotonic()
    response = call_ollama_vision(model, task_def["prompt"], image_b64)
    gen_time = time.monotonic() - t0

    score = task_def["scorer"](response) if response else 0.0
    passed = score >= task_def["pass_threshold"]

    print(f"  [{task_def['category']}] {task_def['task']:<25s}  score={score:.2f}  "
          f"{'PASS' if passed else 'FAIL'}  ({gen_time:.1f}s)")

    return {
        "task": task_def["task"],
        "category": task_def["category"],
        "score": round(score, 4),
        "passed": passed,
        "response_excerpt": (response[:200] + "...") if len(response) > 200 else response,
        "generation_time_s": round(gen_time, 2),
    }


def run_model(model: str, asset_dir: str) -> dict:
    """Run all tasks for a single model."""
    run_started = datetime.datetime.now(datetime.timezone.utc)
    print(f"\n[multimodal] === {model} ===")

    results = []
    for task_def in TASKS:
        result = run_task(model, task_def, asset_dir)
        results.append(result)

    # Compute category scores
    categories: dict[str, list[float]] = {}
    for r in results:
        categories.setdefault(r["category"], []).append(r["score"])
    category_scores = {cat: round(sum(scores) / len(scores), 4)
                       for cat, scores in categories.items()}

    all_scores = [r["score"] for r in results]
    overall = round(sum(all_scores) / max(len(all_scores), 1), 4)

    run_finished = datetime.datetime.now(datetime.timezone.utc)

    print(f"[multimodal] {model} — overall={overall:.2f}  "
          f"categories={category_scores}")

    return {
        "model": model,
        "benchmark": "multimodal",
        "run_started_at": run_started.isoformat(),
        "run_finished_at": run_finished.isoformat(),
        "results": results,
        "category_scores": category_scores,
        "overall_score": overall,
    }


def print_leaderboard(all_results: list[dict]) -> None:
    """Print a summary leaderboard across all models."""
    print("\n" + "=" * 72)
    print("MULTIMODAL VISION BENCHMARK — LEADERBOARD")
    print("=" * 72)
    sorted_results = sorted(all_results, key=lambda r: r["overall_score"], reverse=True)
    print(f"{'Model':<35s} {'Overall':>8s}  {'OCR':>5s}  {'Doc':>5s}  "
          f"{'Chart':>5s}  {'VQA':>5s}  {'Code':>5s}  {'Math':>5s}")
    print("-" * 72)
    for r in sorted_results:
        cs = r["category_scores"]
        print(f"{r['model']:<35s} {r['overall_score']:>8.2f}  "
              f"{cs.get('ocr', 0):>5.2f}  {cs.get('document', 0):>5.2f}  "
              f"{cs.get('chart', 0):>5.2f}  {cs.get('vqa', 0):>5.2f}  "
              f"{cs.get('code', 0):>5.2f}  {cs.get('math', 0):>5.2f}")
    print("=" * 72)


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Multimodal vision benchmark for Ollama models",
    )
    parser.add_argument("--models", nargs="+", required=True,
                        help="Ollama model names to benchmark")
    parser.add_argument("--checkpoint-dir", default="results",
                        help="Directory for per-model result checkpoints")
    parser.add_argument("--asset-dir", default="scripts/multimodal_assets",
                        help="Directory containing test images")
    args = parser.parse_args()

    ensure_assets(args.asset_dir)

    all_results: list[dict] = []
    for model in args.models:
        try:
            result = run_model(model, args.asset_dir)
            all_results.append(result)

            # Write per-model checkpoint
            slug = model_slug(model)
            checkpoint_path = os.path.join(args.checkpoint_dir, f"multimodal-{slug}.json")
            write_json(checkpoint_path, result)
            print(f"[multimodal] Checkpoint saved: {checkpoint_path}")

        except Exception as exc:
            print(f"[multimodal] ERROR running {model}: {type(exc).__name__}: {exc}")
            # Write error checkpoint
            slug = model_slug(model)
            checkpoint_path = os.path.join(args.checkpoint_dir, f"multimodal-{slug}.json")
            write_json(checkpoint_path, {
                "model": model,
                "benchmark": "multimodal",
                "run_started_at": datetime.datetime.now(datetime.timezone.utc).isoformat(),
                "run_finished_at": datetime.datetime.now(datetime.timezone.utc).isoformat(),
                "error": str(exc),
                "results": [],
                "category_scores": {},
                "overall_score": 0.0,
            })

    if all_results:
        print_leaderboard(all_results)


if __name__ == "__main__":
    main()
