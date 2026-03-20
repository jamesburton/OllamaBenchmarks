"""Compare model performance across backends: Ollama vs llama-server (Vulkan) vs vLLM.

Runs the same quality + throughput benchmarks against different serving backends
to identify the fastest inference path for each model.

Usage:
  # Compare top model across Ollama and llama-server:
  python scripts/benchmark_backend_compare.py --model nemotron-3-nano:latest

  # Compare specific models:
  python scripts/benchmark_backend_compare.py --model qwen3-coder:30b qwen3:8b

  # Skip quality (throughput only):
  python scripts/benchmark_backend_compare.py --model qwen3:8b --throughput-only

  # Use a running vLLM server too:
  python scripts/benchmark_backend_compare.py --model qwen3:8b --vllm-url http://localhost:8000

Requires: Ollama running, llama-server.exe on PATH or specified via --llama-server-path.
"""

import argparse
import datetime
import json
import os
import re
import signal
import subprocess
import sys
import time
import urllib.error
import urllib.request
from typing import Any

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
REPO_ROOT = os.path.dirname(SCRIPT_DIR)
RESULTS_DIR = os.path.join(REPO_ROOT, "results")

DEFAULT_LLAMA_SERVER = os.path.expandvars(
    r"%USERPROFILE%\.docker\bin\inference\llama-server.exe"
)
LLAMA_SERVER_PORT = 8081


def model_slug(model: str) -> str:
    return re.sub(r"[^\w\.-]", "_", model.replace(":", "_").replace("/", "_"))


def write_json(path: str, data: Any) -> None:
    os.makedirs(os.path.dirname(path) or ".", exist_ok=True)
    with open(path, "w", encoding="utf-8") as f:
        f.write(json.dumps(data, indent=2) + "\n")


def get_gguf_blob_path(model: str) -> str | None:
    """Resolve the GGUF blob path from Ollama's model store."""
    try:
        result = subprocess.run(
            ["ollama", "show", model, "--modelfile"],
            capture_output=True, text=True, timeout=15,
        )
        for line in result.stdout.splitlines():
            if line.startswith("FROM ") and (":\\" in line or ":/" in line):
                return line[5:].strip()
    except Exception:
        pass
    return None


def wait_for_health(url: str, timeout: int = 300) -> bool:
    """Wait for a server health endpoint to respond."""
    deadline = time.time() + timeout
    while time.time() < deadline:
        try:
            req = urllib.request.Request(f"{url}/health")
            with urllib.request.urlopen(req, timeout=5):
                return True
        except Exception:
            pass
        time.sleep(3)
    return False


def wait_for_openai(url: str, timeout: int = 300) -> bool:
    """Wait for an OpenAI-compatible /v1/models endpoint."""
    deadline = time.time() + timeout
    while time.time() < deadline:
        try:
            req = urllib.request.Request(f"{url}/v1/models")
            with urllib.request.urlopen(req, timeout=5) as resp:
                data = json.loads(resp.read().decode())
                if data.get("data"):
                    return True
        except Exception:
            pass
        time.sleep(3)
    return False


def run_quality_openai(base_url: str, model_name: str, output_path: str) -> dict | None:
    """Run quality benchmark via OpenAI-compatible API."""
    cmd = [
        sys.executable,
        os.path.join(SCRIPT_DIR, "benchmark_quality_openai.py"),
        "--models", model_name,
        "--base-url", base_url,
        "--output", output_path,
    ]
    result = subprocess.run(cmd, capture_output=True, text=True, timeout=1800)
    if result.returncode != 0:
        print(f"  Quality error: {result.stderr[:300]}", file=sys.stderr)
        return None
    try:
        return json.loads(result.stdout)
    except json.JSONDecodeError:
        return None


def run_throughput_openai(base_url: str, model_name: str, output_path: str, process_name: str = "llama-server") -> dict | None:
    """Run throughput benchmark via OpenAI-compatible API."""
    cmd = [
        sys.executable,
        os.path.join(SCRIPT_DIR, "benchmark_throughput_openai.py"),
        "--model", model_name,
        "--base-url", base_url,
        "--output", output_path,
        "--process-name", process_name,
    ]
    result = subprocess.run(cmd, capture_output=True, text=True, timeout=1800)
    if result.returncode != 0:
        print(f"  Throughput error: {result.stderr[:300]}", file=sys.stderr)
        return None
    try:
        return json.loads(result.stdout)
    except json.JSONDecodeError:
        return None


def run_quality_ollama(model: str, output_path: str) -> dict | None:
    """Run quality benchmark via Ollama native API."""
    cmd = [
        sys.executable,
        os.path.join(SCRIPT_DIR, "benchmark_quality.py"),
        "--models", model,
        "--output", output_path,
    ]
    result = subprocess.run(cmd, capture_output=True, text=True, timeout=1800)
    if result.returncode != 0:
        print(f"  Quality error: {result.stderr[:300]}", file=sys.stderr)
        return None
    try:
        return json.loads(result.stdout)
    except json.JSONDecodeError:
        return None


def run_throughput_ollama(model: str, output_path: str) -> dict | None:
    """Run throughput benchmark via Ollama (PowerShell script)."""
    cmd = [
        "powershell", "-ExecutionPolicy", "Bypass",
        "-File", os.path.join(SCRIPT_DIR, "benchmark_throughput_resource.ps1"),
        "-Models", model,
        "-OutputPath", output_path,
    ]
    result = subprocess.run(cmd, capture_output=True, text=True, timeout=1800)
    if result.returncode != 0:
        print(f"  Throughput error: {result.stderr[:300]}", file=sys.stderr)
        return None
    try:
        return json.load(open(output_path))
    except Exception:
        return None


def benchmark_ollama(model: str, slug: str, throughput_only: bool) -> dict:
    """Run benchmarks against Ollama."""
    row: dict[str, Any] = {"backend": "ollama", "model": model}

    tp_path = os.path.join(RESULTS_DIR, f"compare-ollama-tp-{slug}.json")
    print(f"  Throughput...")
    tp = run_throughput_ollama(model, tp_path)
    if tp and tp.get("results"):
        row["toks_per_s"] = tp["results"][0].get("toks_per_s", 0)
        row["ram_peak_gb"] = tp["results"][0].get("ram_peak_gb", 0)
        row["gpu_mem_peak_gb"] = tp["results"][0].get("gpu_mem_peak_gb", 0)
    else:
        row["toks_per_s"] = 0
        row["error_throughput"] = "failed"

    if not throughput_only:
        q_path = os.path.join(RESULTS_DIR, f"compare-ollama-q-{slug}.json")
        print(f"  Quality...")
        q = run_quality_ollama(model, q_path)
        if q and q.get("results"):
            r = q["results"][0]
            row["score"] = r.get("score", 0)
            row["score_max"] = r.get("score_max", 0)
        else:
            row["score"] = 0
            row["score_max"] = 5
            row["error_quality"] = "failed"

    return row


def benchmark_llama_server(
    model: str, slug: str, llama_server_path: str,
    port: int, throughput_only: bool,
) -> dict:
    """Run benchmarks against llama-server (Vulkan)."""
    row: dict[str, Any] = {"backend": "llama-server-vulkan", "model": model}

    blob_path = get_gguf_blob_path(model)
    if not blob_path:
        row["error"] = f"Could not resolve GGUF blob for {model}"
        return row

    if not os.path.isfile(llama_server_path):
        row["error"] = f"llama-server not found at {llama_server_path}"
        return row

    base_url = f"http://127.0.0.1:{port}"

    # Kill any existing llama-server
    subprocess.run(
        ["powershell", "-NoProfile", "-Command",
         "Get-Process llama-server -ErrorAction SilentlyContinue | Stop-Process -Force"],
        capture_output=True, timeout=10,
    )
    time.sleep(1)

    # Start llama-server with Vulkan
    server_args = [
        llama_server_path,
        "-m", blob_path,
        "--host", "127.0.0.1",
        "--port", str(port),
        "--alias", model,
        "--ctx-size", "32768",
        "--n-gpu-layers", "99",
        "--jinja",
        "--reasoning", "off",
    ]

    print(f"  Starting llama-server (Vulkan)...")
    proc = subprocess.Popen(
        server_args,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
    )

    try:
        if not wait_for_health(base_url, timeout=300):
            row["error"] = "llama-server did not start"
            return row

        print(f"  Throughput...")
        tp_path = os.path.join(RESULTS_DIR, f"compare-vulkan-tp-{slug}.json")
        tp = run_throughput_openai(base_url, model, tp_path, process_name="llama-server")
        if tp and tp.get("results"):
            row["toks_per_s"] = tp["results"][0].get("toks_per_s", 0)
            row["ram_peak_gb"] = tp["results"][0].get("ram_peak_gb", 0)
            row["gpu_mem_peak_gb"] = tp["results"][0].get("gpu_mem_peak_gb", 0)
        else:
            row["toks_per_s"] = 0
            row["error_throughput"] = "failed"

        if not throughput_only:
            print(f"  Quality...")
            q_path = os.path.join(RESULTS_DIR, f"compare-vulkan-q-{slug}.json")
            q = run_quality_openai(base_url, model, q_path)
            if q and q.get("results"):
                r = q["results"][0]
                row["score"] = r.get("score", 0)
                row["score_max"] = r.get("score_max", 0)
            else:
                row["score"] = 0
                row["score_max"] = 5
                row["error_quality"] = "failed"
    finally:
        proc.terminate()
        try:
            proc.wait(timeout=10)
        except subprocess.TimeoutExpired:
            proc.kill()
        time.sleep(2)

    return row


def benchmark_vllm(model: str, slug: str, vllm_url: str, throughput_only: bool) -> dict:
    """Run benchmarks against an already-running vLLM server."""
    row: dict[str, Any] = {"backend": "vllm", "model": model}

    # Get served model name
    try:
        req = urllib.request.Request(f"{vllm_url}/v1/models")
        with urllib.request.urlopen(req, timeout=10) as resp:
            data = json.loads(resp.read().decode())
            served_name = data.get("data", [{}])[0].get("id", model)
    except Exception:
        row["error"] = f"vLLM server not reachable at {vllm_url}"
        return row

    print(f"  Throughput (vLLM model: {served_name})...")
    tp_path = os.path.join(RESULTS_DIR, f"compare-vllm-tp-{slug}.json")
    tp = run_throughput_openai(vllm_url, served_name, tp_path, process_name="python3")
    if tp and tp.get("results"):
        row["toks_per_s"] = tp["results"][0].get("toks_per_s", 0)
        row["ram_peak_gb"] = tp["results"][0].get("ram_peak_gb", 0)
        row["gpu_mem_peak_gb"] = tp["results"][0].get("gpu_mem_peak_gb", 0)
    else:
        row["toks_per_s"] = 0
        row["error_throughput"] = "failed"

    if not throughput_only:
        print(f"  Quality...")
        q_path = os.path.join(RESULTS_DIR, f"compare-vllm-q-{slug}.json")
        q = run_quality_openai(vllm_url, served_name, q_path)
        if q and q.get("results"):
            r = q["results"][0]
            row["score"] = r.get("score", 0)
            row["score_max"] = r.get("score_max", 0)
        else:
            row["score"] = 0
            row["score_max"] = 5
            row["error_quality"] = "failed"

    return row


def print_comparison(rows: list[dict], throughput_only: bool) -> str:
    """Format and print a comparison table."""
    lines = []
    if throughput_only:
        lines.append(f"{'Backend':<25} {'Model':<40} {'tok/s':>8} {'RAM GB':>8} {'VRAM GB':>9}")
        lines.append("-" * 93)
        for r in rows:
            err = r.get("error", "")
            if err:
                lines.append(f"{r['backend']:<25} {r['model']:<40} {'ERROR':>8}   {err}")
            else:
                lines.append(
                    f"{r['backend']:<25} {r['model']:<40} {r.get('toks_per_s',0):>8.1f} "
                    f"{r.get('ram_peak_gb',0):>8.1f} {r.get('gpu_mem_peak_gb',0):>9.1f}"
                )
    else:
        lines.append(f"{'Backend':<25} {'Model':<40} {'Score':>7} {'tok/s':>8} {'RAM GB':>8} {'VRAM GB':>9}")
        lines.append("-" * 100)
        for r in rows:
            err = r.get("error", "")
            if err:
                lines.append(f"{r['backend']:<25} {r['model']:<40} {'ERROR':>7}   {err}")
            else:
                score = f"{r.get('score','?')}/{r.get('score_max','?')}"
                lines.append(
                    f"{r['backend']:<25} {r['model']:<40} {score:>7} {r.get('toks_per_s',0):>8.1f} "
                    f"{r.get('ram_peak_gb',0):>8.1f} {r.get('gpu_mem_peak_gb',0):>9.1f}"
                )

    table = "\n".join(lines)
    print(f"\n{'='*60}")
    print("Backend Comparison Results")
    print(f"{'='*60}")
    print(table)
    return table


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Compare model performance across inference backends",
    )
    parser.add_argument("--model", nargs="+", required=True,
                        help="Ollama model name(s) to benchmark")
    parser.add_argument("--llama-server-path", default=DEFAULT_LLAMA_SERVER,
                        help=f"Path to llama-server.exe (default: {DEFAULT_LLAMA_SERVER})")
    parser.add_argument("--llama-server-port", type=int, default=LLAMA_SERVER_PORT,
                        help=f"Port for llama-server (default: {LLAMA_SERVER_PORT})")
    parser.add_argument("--vllm-url",
                        help="URL of running vLLM server (e.g. http://localhost:8000)")
    parser.add_argument("--throughput-only", action="store_true",
                        help="Skip quality benchmarks")
    parser.add_argument("--skip-ollama", action="store_true",
                        help="Skip Ollama backend")
    parser.add_argument("--skip-llama-server", action="store_true",
                        help="Skip llama-server backend")
    parser.add_argument("--output", default=os.path.join(RESULTS_DIR, "backend-comparison-current.json"),
                        help="Output path for comparison results")
    args = parser.parse_args()

    all_rows: list[dict] = []
    run_started = datetime.datetime.now(datetime.timezone.utc)

    for model in args.model:
        slug = model_slug(model)
        print(f"\n{'='*60}")
        print(f"Model: {model}")
        print(f"{'='*60}")

        if not args.skip_ollama:
            print(f"\n[Ollama]")
            row = benchmark_ollama(model, slug, args.throughput_only)
            all_rows.append(row)
            tps = row.get("toks_per_s", 0)
            print(f"  -> {tps:.1f} tok/s" + (f", {row.get('score','?')}/{row.get('score_max','?')}" if not args.throughput_only else ""))

        if not args.skip_llama_server:
            print(f"\n[llama-server (Vulkan)]")
            row = benchmark_llama_server(
                model, slug, args.llama_server_path,
                args.llama_server_port, args.throughput_only,
            )
            all_rows.append(row)
            if "error" in row:
                print(f"  -> ERROR: {row['error']}")
            else:
                tps = row.get("toks_per_s", 0)
                print(f"  -> {tps:.1f} tok/s" + (f", {row.get('score','?')}/{row.get('score_max','?')}" if not args.throughput_only else ""))

        if args.vllm_url:
            print(f"\n[vLLM]")
            row = benchmark_vllm(model, slug, args.vllm_url, args.throughput_only)
            all_rows.append(row)
            if "error" in row:
                print(f"  -> ERROR: {row['error']}")
            else:
                tps = row.get("toks_per_s", 0)
                print(f"  -> {tps:.1f} tok/s" + (f", {row.get('score','?')}/{row.get('score_max','?')}" if not args.throughput_only else ""))

    table = print_comparison(all_rows, args.throughput_only)

    # Save results
    payload = {
        "benchmark": "backend_comparison",
        "run_started_at": run_started.isoformat(),
        "run_finished_at": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "models": args.model,
        "backends": [r["backend"] for r in all_rows],
        "results": all_rows,
        "comparison_table": table,
    }
    write_json(args.output, payload)
    print(f"\nResults saved to {args.output}")


if __name__ == "__main__":
    main()
