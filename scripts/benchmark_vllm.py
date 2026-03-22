"""Benchmark models via AMD ROCm vLLM Docker container.

Manages the vLLM container lifecycle and runs quality + throughput benchmarks
using the existing OpenAI-compatible benchmark scripts.

Usage examples:

  # Benchmark a single HuggingFace model (auto-detect GPU runtime):
  python scripts/benchmark_vllm.py --hf-model nvidia/Nemotron-3-Nano-4B-A1B-Instruct

  # Benchmark a GGUF quant with explicit tokenizer:
  python scripts/benchmark_vllm.py \
    --hf-model unsloth/nemotron-3-nano-4b-v1-GGUF:Q4_K_M \
    --tokenizer nvidia/Nemotron-3-Nano-4B-A1B-Instruct

  # Use a specific Docker image tag:
  python scripts/benchmark_vllm.py --hf-model Qwen/Qwen3-8B-Instruct \
    --docker-image rocm/vllm:latest

  # Specify tool-call parser for tool/agentic benchmarks:
  python scripts/benchmark_vllm.py --hf-model Qwen/Qwen3-8B-Instruct \
    --tool-parser hermes

  # Skip quality benchmarks (throughput only):
  python scripts/benchmark_vllm.py --hf-model Qwen/Qwen3-8B-Instruct \
    --throughput-only

  # Connect to an already-running vLLM server:
  python scripts/benchmark_vllm.py --hf-model Qwen/Qwen3-8B-Instruct \
    --base-url http://localhost:8000 --no-docker

Requires: Docker with either ROCm (AMD) or NVIDIA runtime.
"""

import argparse
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

# Map model families to vLLM tool-call-parser values.
TOOL_PARSER_MAP = {
    "qwen": "hermes",
    "nemotron": "hermes",
    "glm": "hermes",
    "granite": "granite",
    "llama": "llama3_json",
    "mistral": "mistral",
    "devstral": "mistral",
    "deepseek": "deepseek_v3",
    "phi": "hermes",
}

DEFAULT_DOCKER_IMAGE = "rocm/vllm:latest"
CONTAINER_NAME = "vllm-benchmark"
DEFAULT_PORT = 8000


def model_slug(model: str) -> str:
    model = re.sub(r":latest$", "", model)
    return re.sub(r"[^\w\.-]", "_", model.replace(":", "_").replace("/", "_"))


def detect_gpu_runtime() -> str | None:
    """Detect available Docker GPU runtime."""
    try:
        output = subprocess.run(
            ["docker", "info", "--format", "{{json .Runtimes}}"],
            capture_output=True, text=True, timeout=15,
        )
        runtimes = output.stdout.strip()
        if "nvidia" in runtimes:
            return "nvidia"
    except Exception:
        pass

    # Check for ROCm devices (Linux)
    if os.path.exists("/dev/kfd") and os.path.exists("/dev/dri"):
        return "rocm"

    return None


def guess_tool_parser(model: str) -> str:
    """Guess the vLLM tool-call-parser from the model name."""
    lower = model.lower()
    for prefix, parser in TOOL_PARSER_MAP.items():
        if prefix in lower:
            return parser
    return "hermes"


def build_docker_cmd(
    image: str,
    hf_model: str,
    port: int,
    runtime: str,
    tokenizer: str | None = None,
    tool_parser: str | None = None,
    max_model_len: int | None = None,
    gpu_memory_utilization: float = 0.9,
    extra_args: list[str] | None = None,
) -> list[str]:
    """Build the docker run command."""
    hf_cache = os.path.expanduser("~/.cache/huggingface")
    os.makedirs(hf_cache, exist_ok=True)

    cmd = ["docker", "run", "--rm", "--name", CONTAINER_NAME]

    if runtime == "nvidia":
        cmd += ["--gpus", "all"]
    elif runtime == "rocm":
        cmd += [
            "--device=/dev/kfd",
            "--device=/dev/dri",
            "--group-add", "video",
            "--cap-add=SYS_PTRACE",
            "--security-opt", "seccomp=unconfined",
            "--ipc=host",
            "-e", "HSA_OVERRIDE_GFX_VERSION=11.0.0",
        ]

    cmd += [
        "-p", f"{port}:8000",
        "-v", f"{hf_cache}:/root/.cache/huggingface",
        image,
        "vllm", "serve", hf_model,
        "--host", "0.0.0.0",
        "--port", "8000",
        "--gpu-memory-utilization", str(gpu_memory_utilization),
        "--dtype", "auto",
    ]

    if tokenizer:
        cmd += ["--tokenizer", tokenizer]

    if max_model_len:
        cmd += ["--max-model-len", str(max_model_len)]

    if tool_parser:
        cmd += [
            "--enable-auto-tool-choice",
            "--tool-call-parser", tool_parser,
        ]

    if extra_args:
        cmd += extra_args

    return cmd


def wait_for_server(base_url: str, timeout: int = 300) -> bool:
    """Wait for vLLM server to be ready."""
    url = f"{base_url.rstrip('/')}/v1/models"
    deadline = time.time() + timeout
    while time.time() < deadline:
        try:
            req = urllib.request.Request(url)
            with urllib.request.urlopen(req, timeout=5) as resp:
                data = json.loads(resp.read().decode())
                if data.get("data"):
                    return True
        except Exception:
            pass
        time.sleep(3)
    return False


def get_served_model_name(base_url: str) -> str | None:
    """Get the model name as reported by the vLLM server."""
    try:
        url = f"{base_url.rstrip('/')}/v1/models"
        req = urllib.request.Request(url)
        with urllib.request.urlopen(req, timeout=10) as resp:
            data = json.loads(resp.read().decode())
            models = data.get("data", [])
            if models:
                return models[0].get("id")
    except Exception:
        pass
    return None


def stop_container() -> None:
    """Stop and remove the vLLM container."""
    subprocess.run(
        ["docker", "stop", CONTAINER_NAME],
        capture_output=True, timeout=30,
    )
    subprocess.run(
        ["docker", "rm", "-f", CONTAINER_NAME],
        capture_output=True, timeout=10,
    )


def run_quality_benchmark(base_url: str, model_name: str, output_path: str, api_key: str | None = None) -> dict[str, Any] | None:
    """Run quality benchmark using existing OpenAI script."""
    cmd = [
        sys.executable,
        os.path.join(SCRIPT_DIR, "benchmark_quality_openai.py"),
        "--models", model_name,
        "--base-url", base_url,
        "--output", output_path,
    ]
    if api_key:
        cmd += ["--api-key", api_key]
    result = subprocess.run(cmd, capture_output=True, text=True, timeout=1800)
    if result.returncode != 0:
        print(f"Quality benchmark error: {result.stderr[:500]}", file=sys.stderr)
        return None
    try:
        return json.loads(result.stdout)
    except json.JSONDecodeError:
        return None


def run_throughput_benchmark(
    base_url: str, model_name: str, output_path: str,
    api_key: str | None = None, process_name: str = "python3",
) -> dict[str, Any] | None:
    """Run throughput benchmark using existing OpenAI script."""
    cmd = [
        sys.executable,
        os.path.join(SCRIPT_DIR, "benchmark_throughput_openai.py"),
        "--model", model_name,
        "--base-url", base_url,
        "--output", output_path,
        "--process-name", process_name,
    ]
    if api_key:
        cmd += ["--api-key", api_key]
    result = subprocess.run(cmd, capture_output=True, text=True, timeout=1800)
    if result.returncode != 0:
        print(f"Throughput benchmark error: {result.stderr[:500]}", file=sys.stderr)
        return None
    try:
        return json.loads(result.stdout)
    except json.JSONDecodeError:
        return None


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Benchmark models via vLLM Docker container",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    parser.add_argument("--hf-model", required=True,
                        help="HuggingFace model ID (e.g. Qwen/Qwen3-8B-Instruct or repo:quant for GGUF)")
    parser.add_argument("--tokenizer",
                        help="Explicit tokenizer for GGUF models (e.g. Qwen/Qwen3-8B)")
    parser.add_argument("--docker-image", default=DEFAULT_DOCKER_IMAGE,
                        help=f"Docker image (default: {DEFAULT_DOCKER_IMAGE})")
    parser.add_argument("--port", type=int, default=DEFAULT_PORT,
                        help=f"Host port for vLLM server (default: {DEFAULT_PORT})")
    parser.add_argument("--tool-parser",
                        help="vLLM tool-call-parser (auto-detected from model name if omitted)")
    parser.add_argument("--max-model-len", type=int,
                        help="Max context length (reduces VRAM usage)")
    parser.add_argument("--gpu-memory-utilization", type=float, default=0.9,
                        help="Fraction of GPU memory to use (default: 0.9)")
    parser.add_argument("--gpu-runtime",
                        choices=["nvidia", "rocm", "auto"], default="auto",
                        help="Docker GPU runtime (default: auto-detect)")
    parser.add_argument("--base-url",
                        help="Connect to existing vLLM server instead of starting Docker")
    parser.add_argument("--no-docker", action="store_true",
                        help="Skip Docker management (use with --base-url)")
    parser.add_argument("--api-key", help="API key for vLLM server")
    parser.add_argument("--throughput-only", action="store_true",
                        help="Skip quality benchmarks")
    parser.add_argument("--quality-only", action="store_true",
                        help="Skip throughput benchmarks")
    parser.add_argument("--output-dir", default=os.path.join(REPO_ROOT, "results"),
                        help="Output directory for results")
    parser.add_argument("--label",
                        help="Label for output files (default: derived from model name)")
    parser.add_argument("--startup-timeout", type=int, default=300,
                        help="Seconds to wait for vLLM server startup (default: 300)")
    parser.add_argument("--extra-vllm-args", nargs="*", default=[],
                        help="Extra arguments to pass to vllm serve")
    args = parser.parse_args()

    label = args.label or f"vllm-{model_slug(args.hf_model)}"
    base_url = args.base_url or f"http://localhost:{args.port}"
    docker_proc = None
    managed_docker = not args.no_docker and not args.base_url

    try:
        if managed_docker:
            # Detect GPU runtime
            if args.gpu_runtime == "auto":
                runtime = detect_gpu_runtime()
                if not runtime:
                    print("ERROR: No GPU runtime detected (need ROCm or NVIDIA Docker runtime).", file=sys.stderr)
                    print("\nFor AMD ROCm: install ROCm and ensure /dev/kfd and /dev/dri exist.", file=sys.stderr)
                    print("For NVIDIA: install nvidia-container-toolkit.", file=sys.stderr)
                    print("\nTo benchmark an already-running vLLM server, use --base-url and --no-docker.", file=sys.stderr)
                    sys.exit(1)
            else:
                runtime = args.gpu_runtime

            print(f"GPU runtime: {runtime}")
            print(f"Docker image: {args.docker_image}")
            print(f"Model: {args.hf_model}")

            # Stop any existing container
            stop_container()

            # Build tool parser
            tool_parser = args.tool_parser or guess_tool_parser(args.hf_model)
            print(f"Tool-call parser: {tool_parser}")

            # Build and start Docker command
            docker_cmd = build_docker_cmd(
                image=args.docker_image,
                hf_model=args.hf_model,
                port=args.port,
                runtime=runtime,
                tokenizer=args.tokenizer,
                tool_parser=tool_parser,
                max_model_len=args.max_model_len,
                gpu_memory_utilization=args.gpu_memory_utilization,
                extra_args=args.extra_vllm_args or None,
            )

            print(f"Starting vLLM container...")
            print(f"  {' '.join(docker_cmd[:8])}...")
            docker_proc = subprocess.Popen(
                docker_cmd,
                stdout=subprocess.PIPE,
                stderr=subprocess.STDOUT,
            )

            # Wait for server readiness
            print(f"Waiting for vLLM server (timeout {args.startup_timeout}s)...")
            if not wait_for_server(base_url, timeout=args.startup_timeout):
                print("ERROR: vLLM server did not start in time.", file=sys.stderr)
                # Show last Docker output for debugging
                if docker_proc.poll() is not None:
                    output = docker_proc.stdout.read().decode(errors="replace")
                    print(f"Container exited with code {docker_proc.returncode}.", file=sys.stderr)
                    print(f"Last output:\n{output[-2000:]}", file=sys.stderr)
                sys.exit(1)

            print("vLLM server ready.")

        # Get the actual model name from the server
        served_name = get_served_model_name(base_url) or args.hf_model
        print(f"Served model name: {served_name}")

        os.makedirs(args.output_dir, exist_ok=True)
        results_summary: dict[str, Any] = {
            "backend": "vllm",
            "hf_model": args.hf_model,
            "served_model_name": served_name,
            "docker_image": args.docker_image if managed_docker else None,
            "base_url": base_url,
        }

        # Run throughput benchmark
        if not args.quality_only:
            tp_output = os.path.join(args.output_dir, f"throughput-vllm-{label}.json")
            print(f"\nRunning throughput benchmark...")
            tp_result = run_throughput_benchmark(base_url, served_name, tp_output, api_key=args.api_key)
            if tp_result:
                tp_data = tp_result.get("results", [{}])
                if tp_data:
                    tps = tp_data[0].get("toks_per_s", 0)
                    print(f"  Throughput: {tps} tok/s")
                    results_summary["throughput"] = tp_data[0]
            else:
                print("  Throughput benchmark failed.")

        # Run quality benchmark
        if not args.throughput_only:
            qual_output = os.path.join(args.output_dir, f"quality-vllm-{label}.json")
            print(f"\nRunning quality benchmark...")
            qual_result = run_quality_benchmark(base_url, served_name, qual_output, api_key=args.api_key)
            if qual_result:
                for r in qual_result.get("results", []):
                    print(f"  Quality: {r.get('score', '?')}/{r.get('score_max', '?')} "
                          f"(coding={r.get('coding_pass')}/{r.get('coding_total')} "
                          f"tool={r.get('tool_pass')}/{r.get('tool_total')} "
                          f"agentic={r.get('agentic_pass')}/{r.get('agentic_total')})")
                    results_summary["quality"] = r
            else:
                print("  Quality benchmark failed.")

        # Print comparison summary
        print(f"\n{'='*60}")
        print(f"vLLM Benchmark Summary: {args.hf_model}")
        print(f"{'='*60}")
        print(json.dumps(results_summary, indent=2))

    finally:
        if managed_docker:
            print("\nStopping vLLM container...")
            stop_container()
            if docker_proc and docker_proc.poll() is None:
                docker_proc.terminate()
                try:
                    docker_proc.wait(timeout=10)
                except subprocess.TimeoutExpired:
                    docker_proc.kill()


if __name__ == "__main__":
    main()
