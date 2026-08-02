"""Config sweep benchmark for dotLLM's own inference server.

dotLLM has no per-request equivalent of Ollama's ``options.num_thread`` /
``num_batch`` / ``num_gpu`` — its threading and GPU-layer configuration is set
at server *startup* via CLI flags (``--threads``, ``--decode-threads``,
``--gpu-layers``, ``--device``). This script adapts the *concept* of
``benchmark_sweep.py`` (compare a handful of configurations) rather than its
literal Ollama field names: for each named variant it launches a fresh
``dotllm serve`` subprocess with a different set of startup flags, waits for
``/v1/models`` to become ready, measures single-shot generation throughput
against ``/v1/chat/completions`` (the same request shape
``benchmark_throughput_openai.py`` uses), then shuts the server down before
moving to the next variant — so each variant gets an uncontended CPU/GPU.

Usage:
    python benchmark_sweep_dotllm.py \
        --dotllm-repo E:\\Development\\dotLLM \
        --model-path "E:\\.cache\\huggingface\\hub\\...\\ggml-model-i2_s.gguf" \
        --model-id ggml-model-i2_s
"""

from __future__ import annotations

import argparse
import datetime
import json
import os
import statistics
import subprocess
import time
import urllib.error
import urllib.request
from typing import Any

from collect_host_info import build_host_info

# Each variant: (name, extra CLI flags appended to `dotllm serve <model> --port N --no-browser --no-ui`)
DEFAULT_SWEEP = [
    ("baseline_cpu", ["--device", "cpu"]),
    ("cpu_threads_8", ["--device", "cpu", "--threads", "8"]),
    ("cpu_threads_16", ["--device", "cpu", "--threads", "16"]),
    ("device_gpu", ["--device", "gpu"]),
]

PROMPT = "Write a concise explanation of dependency injection with one short Python example."


def post_json(url: str, payload: dict, timeout: int = 1800) -> dict:
    req = urllib.request.Request(
        url,
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(req, timeout=timeout) as response:
        return json.loads(response.read().decode("utf-8"))


def wait_for_ready(base_url: str, timeout_s: int = 180) -> bool:
    deadline = time.monotonic() + timeout_s
    while time.monotonic() < deadline:
        try:
            with urllib.request.urlopen(f"{base_url}/v1/models", timeout=5) as resp:
                if resp.status == 200:
                    return True
        except Exception:
            pass
        time.sleep(2)
    return False


def start_server(dotllm_repo: str, model_path: str, port: int, extra_args: list[str], log_path: str) -> subprocess.Popen:
    cmd = [
        "dotnet", "run", "--project", os.path.join(dotllm_repo, "src", "DotLLM.Cli"),
        "-c", "Release", "--",
        "serve", model_path,
        "--port", str(port),
        "--no-browser",
    ] + extra_args
    log_handle = open(log_path, "w", encoding="utf-8")
    return subprocess.Popen(
        cmd,
        cwd=dotllm_repo,
        stdout=log_handle,
        stderr=subprocess.STDOUT,
        text=True,
    )


def stop_server(proc: subprocess.Popen) -> None:
    proc.terminate()
    try:
        proc.wait(timeout=20)
    except subprocess.TimeoutExpired:
        proc.kill()
        proc.wait(timeout=20)


def run_once(base_url: str, model: str) -> dict[str, Any]:
    payload = {
        "model": model,
        "messages": [{"role": "user", "content": PROMPT}],
        "temperature": 0,
        "max_tokens": 192,
    }
    start = time.perf_counter()
    response = post_json(f"{base_url}/v1/chat/completions", payload)
    total_s = time.perf_counter() - start
    usage = response.get("usage", {})
    completion_tokens = int(usage.get("completion_tokens") or 0)
    tokps = (completion_tokens / total_s) if total_s > 0 and completion_tokens > 0 else 0.0
    return {"tps": tokps, "total_s": total_s, "completion_tokens": completion_tokens}


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--dotllm-repo", required=True, help=r"Path to dotLLM repo root, e.g. E:\Development\dotLLM")
    parser.add_argument("--model-path", required=True, help="Path to the GGUF file to serve")
    parser.add_argument("--model-id", required=True, help="Model id string returned by /v1/models (used in requests)")
    parser.add_argument("--port", type=int, default=8090, help="Port used for each sweep instance (reused sequentially)")
    parser.add_argument("--runs", type=int, default=2)
    parser.add_argument("--startup-timeout", type=int, default=180)
    parser.add_argument("--output")
    parser.add_argument("--log-dir", default="../results/sweep-logs")
    args = parser.parse_args()

    run_started_at = datetime.datetime.now(datetime.timezone.utc)
    if not args.output:
        args.output = os.path.join(".", "results", f"sweep-dotllm-{run_started_at.strftime('%Y%m%d-%H%M%S')}.json")
    os.makedirs(args.log_dir, exist_ok=True)

    base_url = f"http://127.0.0.1:{args.port}"
    results = []

    for name, extra_args in DEFAULT_SWEEP:
        print(f"\n=== variant: {name} ({' '.join(extra_args)}) ===")
        log_path = os.path.join(args.log_dir, f"sweep-{name}.log")
        proc = start_server(args.dotllm_repo, args.model_path, args.port, extra_args, log_path)
        row: dict[str, Any] = {"perm": name, "opts": {"cli_args": extra_args}}
        try:
            ready = wait_for_ready(base_url, args.startup_timeout)
            if not ready:
                row["status"] = "request_failed"
                row["errors"] = [f"server did not become ready within {args.startup_timeout}s (see {log_path})"]
                results.append(row)
                continue

            # Warm-up call (excluded from timing)
            try:
                run_once(base_url, args.model_id)
            except Exception as exc:
                row["status"] = "request_failed"
                row["errors"] = [f"warmup failed: {exc}"]
                results.append(row)
                continue

            runs = []
            errors = []
            for _ in range(args.runs):
                try:
                    runs.append(run_once(base_url, args.model_id))
                except (urllib.error.HTTPError, urllib.error.URLError) as exc:
                    errors.append(str(exc))
                except Exception as exc:
                    errors.append(str(exc))

            row["status"] = "ok" if not errors else ("partial_failed" if runs else "request_failed")
            if runs:
                row.update({
                    "tps_avg": round(statistics.mean(r["tps"] for r in runs), 2),
                    "tps_min": round(min(r["tps"] for r in runs), 2),
                    "tps_max": round(max(r["tps"] for r in runs), 2),
                    "completion_tokens_avg": round(statistics.mean(r["completion_tokens"] for r in runs), 1),
                    "total_s_avg": round(statistics.mean(r["total_s"] for r in runs), 3),
                })
            if errors:
                row["errors"] = errors
            results.append(row)
            print(f"  {row.get('status')}: tps_avg={row.get('tps_avg')}")
        finally:
            stop_server(proc)
            time.sleep(2)  # let the port/GPU fully release before the next variant

    payload_obj = {
        "benchmark": "sweep_dotllm",
        "run_started_at": run_started_at.isoformat(),
        "run_finished_at": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "output_path": os.path.abspath(args.output),
        "host_details": build_host_info(),
        "model": args.model_id,
        "model_path": args.model_path,
        "runs": args.runs,
        "note": (
            "dotLLM has no per-request analog of Ollama's num_thread/num_batch/num_gpu options; "
            "threading/GPU-layer config is set at server startup via CLI flags. Each variant below "
            "is a separate `dotllm serve` invocation with different startup flags, benchmarked via "
            "the same single-shot chat-completion request used by benchmark_throughput_openai.py."
        ),
        "variants": results,
    }
    os.makedirs(os.path.dirname(args.output) or ".", exist_ok=True)
    with open(args.output, "w", encoding="utf-8") as handle:
        handle.write(json.dumps(payload_obj, indent=2) + "\n")
    print("\n" + json.dumps(payload_obj, indent=2))


if __name__ == "__main__":
    main()
