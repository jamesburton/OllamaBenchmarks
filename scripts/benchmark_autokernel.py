#!/usr/bin/env python3
"""Autokernel — systematic GPU inference optimisation sweep for Ollama models.

Explores the Ollama inference parameter space to find the fastest configuration
for a given model on the current hardware. Runs a multi-phase sweep:

  Phase 1: GPU layer sweep (num_gpu 0..max) — find the optimal offload point
  Phase 2: Batch size sweep at optimal num_gpu
  Phase 3: Context window sweep at optimal num_gpu + batch
  Phase 4: Thread count sweep at optimal num_gpu + batch + ctx
  Phase 5: Flash attention toggle
  Phase 6: Final confirmation run (3x) at best settings

Each phase picks the best value, locks it in, and moves to the next.
Results are checkpointed after every measurement.

Usage:
    python scripts/benchmark_autokernel.py --model gemma4
    python scripts/benchmark_autokernel.py --model gemma4 --runs 3 --skip-phase 1
    python scripts/benchmark_autokernel.py --model cogito:8b --num-ctx 4096
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
WARMUP_PROMPT = "Warmup. Reply OK."


def think_setting(model: str):
    """Disable thinking for most models; low for gpt-oss."""
    if model.startswith("gpt-oss"):
        return "low"
    return False


def model_slug(model: str) -> str:
    return model.replace(":", "_").replace("/", "_").rstrip("_latest")


def post_generate(model: str, prompt: str, options: dict, timeout: int = 300) -> dict:
    payload = {
        "model": model,
        "prompt": prompt,
        "think": think_setting(model),
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


def get_model_info(model: str) -> dict:
    """Fetch model metadata from Ollama."""
    request = urllib.request.Request(
        "http://127.0.0.1:11434/api/show",
        data=json.dumps({"name": model}).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(request, timeout=30) as response:
        return json.loads(response.read().decode("utf-8"))


def measure(model: str, options: dict, runs: int = 2) -> dict:
    """Run the benchmark prompt multiple times and return stats."""
    measurements = []
    errors = []
    for i in range(runs):
        try:
            r = post_generate(model, PROMPT, {**options, "num_predict": 192, "temperature": 0, "seed": 42})
            eval_s = r["eval_duration"] / 1e9
            prompt_s = r.get("prompt_eval_duration", 0) / 1e9
            tps = r["eval_count"] / eval_s if eval_s > 0 else 0
            measurements.append({
                "tps": round(tps, 2),
                "eval_s": round(eval_s, 3),
                "prompt_s": round(prompt_s, 3),
                "total_s": round(r["total_duration"] / 1e9, 3),
                "eval_count": r["eval_count"],
                "prompt_eval_count": r.get("prompt_eval_count", 0),
            })
        except urllib.error.HTTPError as exc:
            try:
                body = exc.read().decode("utf-8")
            except Exception:
                body = ""
            errors.append(f"HTTP {exc.code}: {body[:200]}")
        except Exception as exc:
            errors.append(str(exc)[:200])

    result = {"runs": len(measurements), "errors": errors}
    if measurements:
        tps_values = [m["tps"] for m in measurements]
        result.update({
            "tps_avg": round(statistics.mean(tps_values), 2),
            "tps_min": round(min(tps_values), 2),
            "tps_max": round(max(tps_values), 2),
            "tps_stdev": round(statistics.stdev(tps_values), 2) if len(tps_values) > 1 else 0,
            "total_s_avg": round(statistics.mean(m["total_s"] for m in measurements), 3),
            "prompt_s_avg": round(statistics.mean(m["prompt_s"] for m in measurements), 3),
            "eval_count": measurements[0]["eval_count"],
            "measurements": measurements,
        })
    return result


def warmup(model: str, options: dict):
    """Load model into memory with a short warmup prompt."""
    try:
        post_generate(model, WARMUP_PROMPT, {**options, "num_predict": 8, "temperature": 0, "seed": 42})
    except Exception:
        pass


def get_layer_count(model: str) -> int:
    """Get the number of layers in the model."""
    try:
        info = get_model_info(model)
        model_info = info.get("model_info", {})
        for key, val in model_info.items():
            if key.endswith(".block_count") and not any(x in key for x in ["audio", "vision"]):
                return int(val)
    except Exception:
        pass
    return 42  # sensible default


def save_checkpoint(path: str, data: dict):
    tmp = path + ".tmp"
    with open(tmp, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2, default=str)
    os.replace(tmp, path)


def print_phase_header(phase: int, name: str):
    print(f"\n{'='*60}")
    print(f"  Phase {phase}: {name}")
    print(f"{'='*60}")


def print_result_row(label: str, result: dict, marker: str = ""):
    if result.get("tps_avg"):
        print(f"  {label:>20s}  ->  {result['tps_avg']:6.1f} tok/s  (+/-{result.get('tps_stdev',0):.1f})  {marker}")
    else:
        err = result.get("errors", ["unknown error"])
        print(f"  {label:>20s}  ->  FAILED: {err[0][:60]}")


def pick_best(results: list[tuple[str, dict, dict]]) -> tuple[str, dict, dict]:
    """From list of (label, opts, result), pick the one with highest tps_avg."""
    valid = [(l, o, r) for l, o, r in results if r.get("tps_avg")]
    if not valid:
        return results[0]
    return max(valid, key=lambda x: x[2]["tps_avg"])


def main():
    parser = argparse.ArgumentParser(description="Autokernel GPU inference optimisation sweep")
    parser.add_argument("--model", required=True, help="Ollama model name")
    parser.add_argument("--runs", type=int, default=2, help="Measurements per config (default: 2)")
    parser.add_argument("--num-ctx", type=int, default=8192, help="Starting context window size")
    parser.add_argument("--skip-phase", type=int, nargs="*", default=[], help="Phase numbers to skip")
    parser.add_argument("--output", help="Output JSON path (auto-generated if omitted)")
    args = parser.parse_args()

    run_started = datetime.datetime.now(datetime.timezone.utc)
    slug = model_slug(args.model)
    if not args.output:
        args.output = f"results/autokernel-{slug}.json"

    os.makedirs(os.path.dirname(args.output) or ".", exist_ok=True)

    layer_count = get_layer_count(args.model)
    print(f"Model: {args.model}")
    print(f"Layers: {layer_count}")
    print(f"Runs per config: {args.runs}")
    print(f"Starting num_ctx: {args.num_ctx}")
    print(f"Output: {args.output}")

    # Accumulate best options across phases
    best_opts = {"num_ctx": args.num_ctx}
    all_phases = {}

    # ── Phase 1: GPU layer sweep ─────────────────────────────────────────
    if 1 not in args.skip_phase:
        print_phase_header(1, "GPU Layer Sweep (num_gpu)")
        # Test: 0 (CPU only), then steps up to max, then 99 (all)
        # For models with many layers, sample at intervals
        gpu_values = [0]
        if layer_count <= 20:
            gpu_values += list(range(1, layer_count + 1))
        else:
            # Sample: every ~5 layers, plus the boundaries
            step = max(1, layer_count // 8)
            gpu_values += list(range(step, layer_count, step))
            if layer_count not in gpu_values:
                gpu_values.append(layer_count)
        gpu_values.append(99)  # "all" sentinel
        gpu_values = sorted(set(gpu_values))

        phase1_results = []
        for ng in gpu_values:
            label = f"num_gpu={ng}"
            opts = {**best_opts, "num_gpu": ng}
            print(f"  Testing {label}...", end="", flush=True)
            warmup(args.model, opts)
            result = measure(args.model, opts, args.runs)
            print_result_row(label, result)
            phase1_results.append((label, {"num_gpu": ng}, result))

        best_label, best_gpu_opts, best_result = pick_best(phase1_results)
        best_opts.update(best_gpu_opts)
        all_phases["phase1_gpu_layers"] = {
            "results": [{**r, "label": l, "opts": o} for l, o, r in phase1_results],
            "best": best_label,
            "best_tps": best_result.get("tps_avg"),
        }
        print(f"\n  * Best: {best_label} -> {best_result.get('tps_avg', '?')} tok/s")
        save_checkpoint(args.output, {"phases": all_phases, "best_opts": best_opts})
    else:
        print(f"\n  [Skipping Phase 1]")

    # ── Phase 2: Batch size sweep ────────────────────────────────────────
    if 2 not in args.skip_phase:
        print_phase_header(2, "Batch Size Sweep (num_batch)")
        batch_values = [128, 256, 512, 1024, 2048]

        phase2_results = []
        for nb in batch_values:
            label = f"num_batch={nb}"
            opts = {**best_opts, "num_batch": nb}
            print(f"  Testing {label}...", end="", flush=True)
            warmup(args.model, opts)
            result = measure(args.model, opts, args.runs)
            print_result_row(label, result)
            phase2_results.append((label, {"num_batch": nb}, result))

        best_label, best_batch_opts, best_result = pick_best(phase2_results)
        best_opts.update(best_batch_opts)
        all_phases["phase2_batch_size"] = {
            "results": [{**r, "label": l, "opts": o} for l, o, r in phase2_results],
            "best": best_label,
            "best_tps": best_result.get("tps_avg"),
        }
        print(f"\n  * Best: {best_label} -> {best_result.get('tps_avg', '?')} tok/s")
        save_checkpoint(args.output, {"phases": all_phases, "best_opts": best_opts})
    else:
        print(f"\n  [Skipping Phase 2]")

    # ── Phase 3: Context window sweep ────────────────────────────────────
    if 3 not in args.skip_phase:
        print_phase_header(3, "Context Window Sweep (num_ctx)")
        ctx_values = [2048, 4096, 8192, 16384, 32768]

        phase3_results = []
        for nc in ctx_values:
            label = f"num_ctx={nc}"
            opts = {**best_opts, "num_ctx": nc}
            print(f"  Testing {label}...", end="", flush=True)
            warmup(args.model, opts)
            result = measure(args.model, opts, args.runs)
            print_result_row(label, result)
            phase3_results.append((label, {"num_ctx": nc}, result))

        best_label, best_ctx_opts, best_result = pick_best(phase3_results)
        best_opts.update(best_ctx_opts)
        all_phases["phase3_context_window"] = {
            "results": [{**r, "label": l, "opts": o} for l, o, r in phase3_results],
            "best": best_label,
            "best_tps": best_result.get("tps_avg"),
        }
        print(f"\n  * Best: {best_label} -> {best_result.get('tps_avg', '?')} tok/s")
        save_checkpoint(args.output, {"phases": all_phases, "best_opts": best_opts})
    else:
        print(f"\n  [Skipping Phase 3]")

    # ── Phase 4: Thread count sweep ──────────────────────────────────────
    if 4 not in args.skip_phase:
        print_phase_header(4, "Thread Count Sweep (num_thread)")
        # Get CPU thread count
        try:
            import multiprocessing
            max_threads = multiprocessing.cpu_count()
        except Exception:
            max_threads = 16
        thread_values = sorted(set([1, 2, 4, 8, max_threads // 2, max_threads]))
        thread_values = [t for t in thread_values if t >= 1]

        phase4_results = []
        for nt in thread_values:
            label = f"num_thread={nt}"
            opts = {**best_opts, "num_thread": nt}
            print(f"  Testing {label}...", end="", flush=True)
            warmup(args.model, opts)
            result = measure(args.model, opts, args.runs)
            print_result_row(label, result)
            phase4_results.append((label, {"num_thread": nt}, result))

        best_label, best_thread_opts, best_result = pick_best(phase4_results)
        best_opts.update(best_thread_opts)
        all_phases["phase4_threads"] = {
            "results": [{**r, "label": l, "opts": o} for l, o, r in phase4_results],
            "best": best_label,
            "best_tps": best_result.get("tps_avg"),
        }
        print(f"\n  * Best: {best_label} -> {best_result.get('tps_avg', '?')} tok/s")
        save_checkpoint(args.output, {"phases": all_phases, "best_opts": best_opts})
    else:
        print(f"\n  [Skipping Phase 4]")

    # ── Phase 5: Flash attention toggle ──────────────────────────────────
    if 5 not in args.skip_phase:
        print_phase_header(5, "Flash Attention Toggle")
        phase5_results = []
        for fa in [False, True]:
            label = f"flash_attention={'on' if fa else 'off'}"
            # flash_attention is set via environment, but Ollama also accepts it as option
            opts = {**best_opts}
            if fa:
                opts["flash_attention"] = True
            print(f"  Testing {label}...", end="", flush=True)
            warmup(args.model, opts)
            result = measure(args.model, opts, args.runs)
            print_result_row(label, result)
            phase5_results.append((label, {"flash_attention": fa}, result))

        best_label, best_fa_opts, best_result = pick_best(phase5_results)
        if best_fa_opts.get("flash_attention"):
            best_opts["flash_attention"] = True
        all_phases["phase5_flash_attention"] = {
            "results": [{**r, "label": l, "opts": o} for l, o, r in phase5_results],
            "best": best_label,
            "best_tps": best_result.get("tps_avg"),
        }
        print(f"\n  * Best: {best_label} -> {best_result.get('tps_avg', '?')} tok/s")
        save_checkpoint(args.output, {"phases": all_phases, "best_opts": best_opts})
    else:
        print(f"\n  [Skipping Phase 5]")

    # ── Phase 6: Final confirmation ──────────────────────────────────────
    print_phase_header(6, "Final Confirmation (best settings, 3 runs)")
    warmup(args.model, best_opts)
    final_result = measure(args.model, best_opts, max(args.runs, 3))
    print_result_row("FINAL", final_result, "***")
    all_phases["phase6_final"] = {
        "opts": best_opts,
        **final_result,
    }

    # ── Save final output ────────────────────────────────────────────────
    run_finished = datetime.datetime.now(datetime.timezone.utc)
    output = {
        "benchmark": "autokernel",
        "model": args.model,
        "run_started_at": run_started.isoformat(),
        "run_finished_at": run_finished.isoformat(),
        "duration_s": round((run_finished - run_started).total_seconds(), 1),
        "output_path": os.path.abspath(args.output),
        "ollama_host": "http://127.0.0.1:11434",
        "host_details": build_host_info(),
        "layer_count": layer_count,
        "best_options": best_opts,
        "best_tps": final_result.get("tps_avg"),
        "phases": all_phases,
    }
    save_checkpoint(args.output, output)

    print(f"\n{'='*60}")
    print(f"  DONE — {args.model}")
    print(f"  Best config: {json.dumps(best_opts)}")
    print(f"  Best throughput: {final_result.get('tps_avg', '?')} tok/s")
    print(f"  Duration: {output['duration_s']}s")
    print(f"  Saved: {args.output}")
    print(f"{'='*60}")


if __name__ == "__main__":
    main()
