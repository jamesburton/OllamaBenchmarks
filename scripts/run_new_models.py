#!/usr/bin/env python3
"""Run full benchmark suite (quality + throughput + coding L3) for new models."""
import json
import os
import subprocess
import sys
import datetime

MODELS = [
    "hf.co/xhxlb/IQuest-Coder-V1-7B-Instruct-GGUF:Q4_K_M",
    "hf.co/xhxlb/IQuest-Coder-V1-14B-Instruct-GGUF:Q4_K_M",
    "hf.co/mradermacher/shenwen-coderV2-Instruct-GGUF:Q8_0",
]

RESULTS_DIR = "results"


def run_cmd(desc, cmd, timeout=7200):
    print(f"  [{desc}] ...", end="", flush=True)
    try:
        result = subprocess.run(cmd, capture_output=True, text=True, timeout=timeout)
        if result.returncode == 0:
            print(" done", flush=True)
        else:
            print(f" exit {result.returncode}", flush=True)
            if result.stderr:
                print(f"    {result.stderr[:200]}", flush=True)
        return result
    except subprocess.TimeoutExpired:
        print(" TIMEOUT", flush=True)
        return None
    except Exception as exc:
        print(f" ERROR: {exc}", flush=True)
        return None


def model_slug(model):
    import re
    model = re.sub(r":latest$", "", model)
    return re.sub(r"[^\w\.-]", "_", model.replace(":", "_").replace("/", "_").replace("\\", "_"))


def main():
    run_start = datetime.datetime.now(datetime.timezone.utc)
    print(f"=== Full benchmark suite for {len(MODELS)} new models ===")
    print(f"Started at {run_start.isoformat()}\n")

    for i, model in enumerate(MODELS, 1):
        slug = model_slug(model)
        print(f"[{i}/{len(MODELS)}] {model}", flush=True)

        # 1. Quality benchmark
        run_cmd("quality", [
            sys.executable, "scripts/benchmark_quality.py",
            "--models", model,
            "--checkpoint-dir", RESULTS_DIR,
        ], timeout=600)

        # 2. Throughput benchmark
        run_cmd("throughput", [
            "powershell", "-ExecutionPolicy", "Bypass",
            "-File", "scripts/benchmark_throughput_resource.ps1",
            "-Models", model,
            "-OutputPath", f"{RESULTS_DIR}/throughput-resource-{slug}.json",
        ], timeout=600)

        # 3. Coding Layer 3
        run_cmd("coding-L3", [
            sys.executable, "scripts/benchmark_coding.py",
            "--models", model,
            "--layers", "3",
            "--checkpoint-dir", RESULTS_DIR,
            "--output", f"{RESULTS_DIR}/coding-{slug}.json",
        ], timeout=7200)

        # Read and report results
        quality_file = f"{RESULTS_DIR}/quality-{slug}.json"
        throughput_file = f"{RESULTS_DIR}/throughput-resource-{slug}.json"
        coding_file = f"{RESULTS_DIR}/coding-{slug}.json"

        q_score = "?"
        if os.path.isfile(quality_file):
            try:
                d = json.load(open(quality_file))
                for r in d.get("results", []):
                    q_score = f"{r.get('score', '?')}/{r.get('score_max', '?')}"
            except Exception:
                pass

        t_speed = "?"
        if os.path.isfile(throughput_file):
            try:
                d = json.load(open(throughput_file))
                for r in d.get("results", []):
                    t_speed = f"{r.get('toks_per_s', '?')} tok/s"
            except Exception:
                pass

        c_score = "?"
        if os.path.isfile(coding_file):
            try:
                d = json.load(open(coding_file))
                if "layer3_weighted_score" in d:
                    total = d.get("layer3_total", 20)
                    passed = sum(1 for r in d.get("layer3_results", []) if r.get("passed"))
                    c_score = f"{passed}/{total}"
            except Exception:
                pass

        print(f"  => Quality: {q_score} | Throughput: {t_speed} | Coding L3: {c_score}")
        print(flush=True)

    run_end = datetime.datetime.now(datetime.timezone.utc)
    print(f"Total time: {run_end - run_start}")


if __name__ == "__main__":
    main()
