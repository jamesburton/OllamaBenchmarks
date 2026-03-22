"""Perplexity (PPL) benchmark for Ollama models.

Estimates perplexity by computing the mean negative-log-probability of
ground-truth continuations across a small fixed corpus.  Lower PPL is
better.  This is a *proxy* — Ollama does not expose true log-probs, so
we use the /api/generate endpoint with ``prompt`` set to the context and
measure how closely the model reproduces the known continuation.

Usage:
    python benchmark_ppl.py --models qwen3.5:9b cogito:14b
    python benchmark_ppl.py --models qwen3.5:4b --checkpoint-dir ../results
"""

from __future__ import annotations

import argparse
import datetime as dt
import json
import math
import os
import re
import textwrap
import urllib.request
from typing import Any

from collect_host_info import build_host_info

# ---------------------------------------------------------------------------
# Corpus: short passages the model must predict token-by-token.
# Each entry is (name, context, continuation).
# ---------------------------------------------------------------------------
PPL_PASSAGES = [
    (
        "python_fibonacci",
        "Write a Python function to compute Fibonacci numbers:\n\n```python\ndef fibonacci(n):\n",
        '    if n <= 1:\n        return n\n    return fibonacci(n - 1) + fibonacci(n - 2)\n```',
    ),
    (
        "explain_http",
        "Explain the HTTP request-response cycle in one paragraph:\n\n",
        "When a client sends an HTTP request to a server, it includes a method "
        "(such as GET or POST), a URL, headers, and optionally a body. The server "
        "processes the request, performs the necessary operations, and returns an "
        "HTTP response containing a status code, headers, and a body with the "
        "requested resource or an error message.",
    ),
    (
        "sql_query",
        "Write a SQL query to find the top 5 customers by total order amount:\n\n```sql\n",
        "SELECT c.customer_name, SUM(o.amount) AS total\n"
        "FROM customers c\n"
        "JOIN orders o ON c.id = o.customer_id\n"
        "GROUP BY c.customer_name\n"
        "ORDER BY total DESC\n"
        "LIMIT 5;\n```",
    ),
    (
        "science_gravity",
        "Describe how gravity works according to general relativity:\n\n",
        "In general relativity, gravity is not a force but a curvature of "
        "spacetime caused by mass and energy. Objects follow geodesics — the "
        "straightest possible paths through curved spacetime — which we perceive "
        "as gravitational attraction. The more massive an object, the more it "
        "warps the fabric of spacetime around it.",
    ),
    (
        "json_example",
        "Create a JSON object representing a book with title, author, year, and tags:\n\n```json\n",
        '{\n  "title": "The Great Gatsby",\n  "author": "F. Scott Fitzgerald",\n'
        '  "year": 1925,\n  "tags": ["classic", "fiction", "american"]\n}\n```',
    ),
]

OLLAMA_HOST = os.environ.get("OLLAMA_HOST", "http://127.0.0.1:11434")


def ollama_generate(model: str, prompt: str, num_predict: int = 256) -> dict[str, Any]:
    """Call /api/chat and return the parsed JSON response.

    Uses the chat endpoint so thinking-mode models return content
    without consuming tokens on <think> blocks.
    """
    payload = json.dumps({
        "model": model,
        "messages": [
            {"role": "system", "content": "Respond directly. Do not explain your reasoning."},
            {"role": "user", "content": prompt},
        ],
        "stream": False,
        "think": False,
        "options": {
            "temperature": 0,
            "top_p": 1,
            "seed": 42,
            "num_predict": num_predict,
        },
    }).encode()
    req = urllib.request.Request(
        f"{OLLAMA_HOST}/api/chat",
        data=payload,
        headers={"Content-Type": "application/json"},
    )
    with urllib.request.urlopen(req, timeout=300) as resp:
        data = json.loads(resp.read())
    # Normalise to generate-like response for downstream code
    data["response"] = data.get("message", {}).get("content", "")
    return data


def normalise(text: str) -> str:
    """Collapse whitespace for comparison."""
    return " ".join(text.split()).strip().lower()


def score_continuation(generated: str, expected: str) -> dict[str, Any]:
    """Score how well the generated text matches the expected continuation.

    Returns overlap ratio (0-1), exact-start ratio, and character-level
    edit-distance ratio as a proxy for log-prob quality.
    """
    gen_norm = normalise(generated)
    exp_norm = normalise(expected)

    if not exp_norm:
        return {"overlap": 0.0, "prefix_match": 0.0, "char_similarity": 0.0}

    # Token-level overlap (unigram)
    gen_tokens = set(gen_norm.split())
    exp_tokens = set(exp_norm.split())
    overlap = len(gen_tokens & exp_tokens) / max(len(exp_tokens), 1)

    # Prefix match — how many leading characters match
    prefix_len = 0
    for a, b in zip(gen_norm, exp_norm):
        if a == b:
            prefix_len += 1
        else:
            break
    prefix_match = prefix_len / max(len(exp_norm), 1)

    # Character similarity (1 - normalised edit distance)
    # Use a simple ratio to avoid heavy computation
    shorter = min(len(gen_norm), len(exp_norm))
    longer = max(len(gen_norm), len(exp_norm))
    matching_chars = sum(1 for a, b in zip(gen_norm, exp_norm) if a == b)
    char_similarity = matching_chars / max(longer, 1)

    return {
        "overlap": round(overlap, 4),
        "prefix_match": round(prefix_match, 4),
        "char_similarity": round(char_similarity, 4),
    }


def estimate_ppl_proxy(scores: list[dict[str, Any]]) -> float:
    """Convert similarity scores to a perplexity-like proxy.

    Higher similarity → lower PPL proxy.  We use:
        ppl_proxy = exp(-mean(log(similarity + epsilon)))
    where similarity is the average of our three metrics.
    """
    eps = 1e-6
    vals = []
    for s in scores:
        avg_sim = (s["overlap"] + s["prefix_match"] + s["char_similarity"]) / 3
        avg_sim = max(avg_sim, eps)
        vals.append(-math.log(avg_sim))
    return round(math.exp(sum(vals) / max(len(vals), 1)), 4)


def benchmark_model(model: str) -> dict[str, Any]:
    """Run PPL benchmark for a single model."""
    passage_results = []
    for name, context, expected in PPL_PASSAGES:
        num_predict = len(expected.split()) + 50  # generous margin
        try:
            resp = ollama_generate(model, context, num_predict=num_predict)
            generated = resp.get("response", "")
            scores = score_continuation(generated, expected)
            passage_results.append({
                "passage": name,
                "scores": scores,
                "generated_preview": generated[:200],
                "eval_count": resp.get("eval_count"),
                "eval_duration_ms": round(resp.get("eval_duration", 0) / 1e6, 1),
            })
        except Exception as e:
            passage_results.append({
                "passage": name,
                "error": str(e),
                "scores": {"overlap": 0.0, "prefix_match": 0.0, "char_similarity": 0.0},
            })

    valid_scores = [p["scores"] for p in passage_results if "error" not in p]
    ppl_proxy = estimate_ppl_proxy(valid_scores) if valid_scores else None
    avg_overlap = round(sum(s["overlap"] for s in valid_scores) / max(len(valid_scores), 1), 4)
    avg_prefix = round(sum(s["prefix_match"] for s in valid_scores) / max(len(valid_scores), 1), 4)
    avg_char_sim = round(sum(s["char_similarity"] for s in valid_scores) / max(len(valid_scores), 1), 4)

    return {
        "model": model,
        "ppl_proxy": ppl_proxy,
        "avg_overlap": avg_overlap,
        "avg_prefix_match": avg_prefix,
        "avg_char_similarity": avg_char_sim,
        "passages": passage_results,
    }


def get_model_slug(model: str) -> str:
    model = re.sub(r":latest$", "", model)
    return re.sub(r"[:/\\]", "_", model).rstrip("_")


def main() -> None:
    parser = argparse.ArgumentParser(description="Perplexity proxy benchmark for Ollama models")
    parser.add_argument("--models", nargs="+", required=True, help="Model names to benchmark")
    parser.add_argument("--output", help="Output JSON path (default: auto-timestamped)")
    parser.add_argument("--checkpoint-dir", default=".", help="Directory for per-model checkpoints")
    args = parser.parse_args()

    host_info = build_host_info()
    started = dt.datetime.now(dt.timezone.utc).isoformat()

    results = []
    completed = []
    failed = []
    for model in args.models:
        print(f"[ppl] benchmarking {model} ...")
        try:
            result = benchmark_model(model)
            results.append(result)
            completed.append(model)
            print(f"  ppl_proxy={result['ppl_proxy']}  overlap={result['avg_overlap']}  "
                  f"prefix={result['avg_prefix_match']}  char_sim={result['avg_char_similarity']}")

            # Per-model checkpoint
            slug = get_model_slug(model)
            ckpt_path = os.path.join(args.checkpoint_dir, f"ppl-{slug}.json")
            ckpt = {
                "benchmark": "ppl",
                "host_details": host_info,
                "model": model,
                **result,
            }
            with open(ckpt_path, "w") as f:
                json.dump(ckpt, f, indent=2)
        except Exception as e:
            failed.append({"model": model, "error": str(e)})
            print(f"  FAILED: {e}")

    finished = dt.datetime.now(dt.timezone.utc).isoformat()

    ts = dt.datetime.now().strftime("%Y%m%d-%H%M%S")
    output_path = args.output or os.path.join(
        args.checkpoint_dir, f"ppl-{ts}.json"
    )

    payload = {
        "benchmark": "ppl",
        "run_started_at": started,
        "run_finished_at": finished,
        "output_path": os.path.abspath(output_path),
        "ollama_host": OLLAMA_HOST,
        "host_details": host_info,
        "models": args.models,
        "completed_models": completed,
        "failed_models": failed,
        "results": results,
    }
    with open(output_path, "w") as f:
        json.dump(payload, f, indent=2)

    print(json.dumps(payload, indent=2))


if __name__ == "__main__":
    main()
