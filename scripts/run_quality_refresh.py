"""Re-run the quality benchmark (old 5-question suite -> current 11-question
suite) for installed/cloud models whose results/quality-{slug}.json still has
score_max <= 5. Waits for the main gap-fill queue (run_gap_fill_queue.py) to
finish before starting, since only one model can be loaded on the Strix iGPU
at a time. Checkpoints after every model (benchmark_quality.py already writes
results/quality-{slug}.json per model), and re-renders + republishes the
dashboard data isn't done here (no artifact-publish capability in a detached
process) -- the agent re-renders scripts/render_gap_fill_dashboard.py and
republishes the Artifact itself when it next checks in.
"""
import os
import re
import subprocess
import sys
import time
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent

MODELS = [
    "phi4-mini:latest",
    "lfm2.5-thinking:1.2b",
    "sam860/lfm2:2.6b",
    "LFM2-2.6b-tools:latest",
    "granite4:7b-a1b-h",
    "nemotron-nano-9b-v2-toolfix:latest",
    "hf.co/Jackrong/Qwopus3.5-9B-v3-GGUF:Q8_0",
    "lfm2:24b",
    "nemotron-3-nano:4b",
    "gemma4:31b",
    "gemma4:e2b",
    "qwen3-coder-next:latest",
]

WAIT_PID = int(sys.argv[1]) if len(sys.argv) > 1 else None


def pid_alive(pid: int) -> bool:
    out = subprocess.run(
        ["powershell", "-NoProfile", "-Command", f"(Get-Process -Id {pid} -ErrorAction SilentlyContinue) -ne $null"],
        capture_output=True, text=True,
    )
    return out.stdout.strip() == "True"


def main():
    log_path = REPO_ROOT / "results" / "overnight-logs" / "quality-refresh.log"
    log_path.parent.mkdir(parents=True, exist_ok=True)

    def log(msg):
        line = f"{time.strftime('%H:%M:%S')} {msg}\n"
        with open(log_path, "a", encoding="utf-8") as f:
            f.write(line)

    if WAIT_PID:
        log(f"waiting for gap-fill queue PID {WAIT_PID} to finish...")
        while pid_alive(WAIT_PID):
            time.sleep(60)
        log("gap-fill queue finished, starting quality refresh")

    env = os.environ.copy()
    if not re.match(r"^https?://", env.get("OLLAMA_HOST", "")):
        env["OLLAMA_HOST"] = "http://127.0.0.1:11434"

    for model in MODELS:
        log(f"--- refreshing quality: {model} ---")
        result = subprocess.run(
            [sys.executable, "scripts/benchmark_quality.py", "--models", model],
            cwd=REPO_ROOT, env=env, capture_output=True, text=True,
        )
        log(f"exit={result.returncode}")
        if result.returncode != 0:
            log(f"STDERR tail: {result.stderr[-2000:]}")

    log("=== quality refresh finished ===")


if __name__ == "__main__":
    main()
