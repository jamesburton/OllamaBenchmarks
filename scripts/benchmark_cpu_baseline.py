#!/usr/bin/env python3
"""CPU-only baseline benchmark for Framework (no eGPU).

Runs each model with num_gpu=0, tests a few thread counts, and records
the best CPU throughput. Much faster than the full autokernel sweep since
it skips GPU layer testing and uses fewer configs.

Usage:
    python scripts/benchmark_cpu_baseline.py --models gemma4:e2b cogito:8b llama3.2:3b
"""

import argparse
import datetime
import json
import os
import statistics
import sys
import time
import urllib.error
import urllib.request

sys.path.insert(0, os.path.dirname(__file__))
from collect_host_info import build_host_info

PROMPT = "Write a concise explanation of dependency injection with one short Python example."


def post_generate(model, prompt, options, timeout=300):
    payload = {
        "model": model,
        "prompt": prompt,
        "think": False,
        "stream": False,
        "options": options,
    }
    request = urllib.request.Request(
        "http://127.0.0.1:11434/api/generate",
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(request, timeout=timeout) as response:
        return json.loads(response.read().decode("utf-8"))


def measure(model, options, num_predict=192):
    """Single measurement run."""
    try:
        r = post_generate(model, PROMPT, {**options, "num_predict": num_predict, "temperature": 0, "seed": 42})
        eval_s = r["eval_duration"] / 1e9
        prompt_s = r.get("prompt_eval_duration", 0) / 1e9
        tps = r["eval_count"] / eval_s if eval_s > 0 else 0
        prompt_tps = r.get("prompt_eval_count", 0) / prompt_s if prompt_s > 0 else 0
        return {
            "tps": round(tps, 2),
            "prompt_tps": round(prompt_tps, 2),
            "eval_count": r["eval_count"],
            "eval_s": round(eval_s, 3),
            "total_s": round(r["total_duration"] / 1e9, 3),
        }
    except Exception as exc:
        return {"tps": 0, "error": str(exc)[:200]}


def warmup(model, options):
    try:
        post_generate(model, "Hi", {**options, "num_predict": 5, "temperature": 0, "seed": 42})
    except Exception:
        pass


def model_slug(model):
    return model.replace(":", "_").replace("/", "_").rstrip("_latest")


def main():
    parser = argparse.ArgumentParser(description="CPU-only baseline benchmark")
    parser.add_argument("--models", nargs="+", required=True)
    parser.add_argument("--num-ctx", type=int, default=8192)
    parser.add_argument("--output", default="results/cpu-baseline-framework-nogpu.json")
    args = parser.parse_args()

    run_started = datetime.datetime.now(datetime.timezone.utc)
    import multiprocessing
    max_threads = multiprocessing.cpu_count()

    # Thread counts to test: 1, half P-cores, all P-cores, half total, all total
    # Core Ultra 7 155H: 6P + 8E + 2LP = 16 cores / 22 threads
    thread_tests = sorted(set([1, 4, 6, 8, 11, 16, max_threads]))
    thread_tests = [t for t in thread_tests if 1 <= t <= max_threads]

    print(f"CPU Baseline Benchmark — Framework (no eGPU)")
    print(f"Models: {', '.join(args.models)}")
    print(f"Thread sweep: {thread_tests}")
    print(f"Context: {args.num_ctx}")
    print(f"{'='*70}")

    all_results = []

    for model in args.models:
        slug = model_slug(model)
        print(f"\n--- {model} ({slug}) ---")

        base_opts = {"num_gpu": 0, "num_ctx": args.num_ctx}

        # Quick single measurement at default threads for baseline
        warmup(model, base_opts)
        baseline = measure(model, base_opts)
        print(f"  baseline (default threads): {baseline.get('tps', 0):.1f} tok/s")

        # Thread sweep
        best_tps = baseline.get("tps", 0)
        best_threads = "default"
        thread_results = []

        for nt in thread_tests:
            opts = {**base_opts, "num_thread": nt}
            warmup(model, opts)
            result = measure(model, opts)
            tps = result.get("tps", 0)
            marker = " *" if tps > best_tps else ""
            print(f"  threads={nt:2d}: {tps:6.1f} tok/s  (prompt: {result.get('prompt_tps',0):.1f} tok/s){marker}")
            thread_results.append({"threads": nt, **result})
            if tps > best_tps:
                best_tps = tps
                best_threads = nt

        # Unload model to free memory for next
        try:
            post_generate(model, "", {"num_predict": 0, "keep_alive": "0"})
        except Exception:
            pass
        # Use keep_alive=0 via the proper API
        try:
            req = urllib.request.Request(
                "http://127.0.0.1:11434/api/generate",
                data=json.dumps({"model": model, "keep_alive": 0}).encode("utf-8"),
                headers={"Content-Type": "application/json"},
                method="POST",
            )
            urllib.request.urlopen(req, timeout=10)
        except Exception:
            pass
        time.sleep(2)

        model_result = {
            "model": model,
            "slug": slug,
            "baseline_tps": baseline.get("tps", 0),
            "best_tps": best_tps,
            "best_threads": best_threads,
            "thread_results": thread_results,
        }
        all_results.append(model_result)
        print(f"  >> Best: {best_tps:.1f} tok/s at threads={best_threads}")

    # Summary
    print(f"\n{'='*70}")
    print(f"  CPU BASELINE SUMMARY — Framework (no eGPU)")
    print(f"{'='*70}")
    print(f"  {'Model':<25s} {'Best tok/s':>10s} {'Threads':>8s} {'Prompt tok/s':>13s}")
    print(f"  {'-'*25} {'-'*10} {'-'*8} {'-'*13}")
    for r in sorted(all_results, key=lambda x: x["best_tps"], reverse=True):
        # Find prompt tps at best thread count
        prompt_tps = 0
        for tr in r["thread_results"]:
            if tr.get("threads") == r["best_threads"]:
                prompt_tps = tr.get("prompt_tps", 0)
                break
        print(f"  {r['model']:<25s} {r['best_tps']:>10.1f} {str(r['best_threads']):>8s} {prompt_tps:>13.1f}")

    # Save
    run_finished = datetime.datetime.now(datetime.timezone.utc)
    output = {
        "benchmark": "cpu-baseline",
        "platform": "framework-nogpu",
        "run_started_at": run_started.isoformat(),
        "run_finished_at": run_finished.isoformat(),
        "duration_s": round((run_finished - run_started).total_seconds(), 1),
        "host_details": build_host_info(),
        "num_ctx": args.num_ctx,
        "thread_sweep": thread_tests,
        "results": all_results,
    }
    os.makedirs(os.path.dirname(args.output) or ".", exist_ok=True)
    with open(args.output, "w", encoding="utf-8") as f:
        json.dump(output, f, indent=2, default=str)
    print(f"\n  Saved: {args.output}")


if __name__ == "__main__":
    main()
