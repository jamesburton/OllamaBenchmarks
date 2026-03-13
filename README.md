# OllamaBenchmarks

Local benchmark harness and captured results for evaluating Ollama models across multiple Windows hosts.

## What is in this repo

- `results/` contains the current benchmark outputs plus archived system snapshots.
- `benchmark-models.json` tracks the benchmark-suite models, locally installed models, and the combined effective benchmark list for the current machine.
- `scripts/benchmark_throughput_resource.ps1` measures generation speed and samples CPU, RAM, GPU utilization, and GPU dedicated memory.
- `scripts/benchmark_quality.py` runs a compact coding, tool-use, and agent-orchestration quality suite.
- `scripts/benchmark_backend.ps1` compares `auto`, `vulkan`, and `rocm` backend selection by launching isolated Ollama servers on alternate ports, and now preflights desktop-managed models so those temporary servers can resolve them reliably.
- `scripts/benchmark_sweep.py` runs decode-side option sweeps for a single model.
- Multi-model throughput and quality runs now checkpoint one JSON artifact per model and keep the combined `*-current.json` refreshed as they progress.
- `scripts/collect_host_info.py` captures machine metadata for archived benchmark snapshots.
- `scripts/archive_system_benchmarks.py` snapshots the current benchmark artifacts under `results/systems/<host>-<label>/`.
- `scripts/build_cross_system_summary.py` rebuilds the comparison table across all archived systems.
- All benchmark scripts now auto-write a structured JSON artifact under `results/` even if you do not pass an output path.
- The reusable agent workflow for this repo lives in [skills/ollama-benchmark-matrix/SKILL.md](C:/Development/OllamaBenchmarks/skills/ollama-benchmark-matrix/SKILL.md).
- `plans/` contains the next-step benchmark expansion work.

## Current headline findings

- Latest Framework capture (`2026-03-12`): `qwen3.5:latest` is the best overall local coding model at `37.71 tok/s` with a `5/5` quality pass.
- `ministral-3:14b` also full-passed at `32.14 tok/s`; `rnj-1:8b` is faster at `46.60 tok/s` but missed the agentic task.
- `lfm2.5-thinking:1.2b` is the local raw-throughput outlier at `205.66 tok/s`, but it failed the compact quality suite.
- On the Framework machine, backend comparison on `qwen3.5:latest` now favors `auto` over both `vulkan` and `rocm`.
- Across archived systems, the current strongest fully-tested coding captures remain `qwen3-coder-next:latest` on Strix Halo and `qwen3.5:latest` on the Framework machine.

See [results/session-2026-03-12-summary.md](C:/Development/OllamaBenchmarks/results/session-2026-03-12-summary.md) for the latest Framework session summary.
See [results/cross-system-summary.md](C:/Development/OllamaBenchmarks/results/cross-system-summary.md) for the rolling matrix across archived hosts.

## Quick start

Prerequisites:

- Windows PowerShell
- Python 3 on `PATH`
- Ollama running locally on `http://127.0.0.1:11434`
- Models already pulled with `ollama pull <model>`

### Throughput and resource benchmark

Example:

```powershell
./scripts/benchmark_throughput_resource.ps1 `
  -Models @('qwen3-coder-next:latest','lfm2:24b') `
  -NumPredict 192
```

Output fields:

- `toks_per_s`: decode tokens per second using `eval_count / eval_duration`
- `cpu_avg_pct`, `cpu_peak_pct`: sampled Ollama process CPU usage
- `ram_peak_gb`: peak process working set
- `gpu_util_avg`, `gpu_util_peak`: sampled GPU engine utilization counters
- `gpu_mem_peak_gb`: peak dedicated GPU memory seen for Ollama processes
- `ollama_ps`: the concurrent `ollama ps` line for the model

### Quick quality benchmark

Example:

```powershell
python .\scripts\benchmark_quality.py `
  --models qwen3-coder-next:latest lfm2:24b granite4:32b-a9b-h
```

This suite currently checks:

- Small executable coding tasks
- Single-call tool invocation correctness
- Plan creation plus a follow-up sub-agent request/finalization sequence

### Backend comparison

Example:

```powershell
./scripts/benchmark_backend.ps1 `
  -Model qwen3-coder-next:latest
```

This launches temporary Ollama servers on alternate ports and compares:

- `auto`
- `vulkan`
- `rocm`

### Option sweep

Example:

```powershell
python .\scripts\benchmark_sweep.py `
  --model qwen3-coder-next:latest
```

Default sweep variants:

- baseline
- `num_thread=8`
- `num_thread=16`
- `num_batch=1024`
- `num_gpu=99`

Each script still accepts an explicit output path if you want a deterministic filename. The generated JSON now includes run metadata such as timestamps, model list, benchmark type, and the resolved artifact path.
When throughput or quality runs cover multiple models, they also write per-model checkpoint files such as `results/throughput-resource-<model>.json` and `results/quality-<model>.json`, then roll those into the combined current artifact.

## Cross-system workflow

After refreshing the `*-current.json` artifacts for a machine, archive and summarize it with:

```powershell
python .\scripts\archive_system_benchmarks.py --label 20260311
python .\scripts\build_cross_system_summary.py
```

This creates a per-system snapshot under `results/systems/` with `host-info.json` and `manifest.json`, then rebuilds the shared comparison table.

## How to read the results

- Prefer `toks_per_s` for steady-state decode speed.
- Use end-to-end time when comparing user experience, because some settings increase total latency without helping real throughput.
- Treat the quick quality suite as a screening pass, not a final model quality verdict.
- The current quick quality score is out of `5`: two coding tasks, two simple tool tasks, and one agentic planning/delegation task.
- Re-run the same prompt set before making model changes or backend changes.

## Missing or reconstructed artifacts

No previously saved benchmark artifacts from the parent `C:\Development` workspace were found for this session, so the files in `results/` were reconstructed directly from the measured values produced during the session.

## Next work

See [PLANS.md](C:/Development/OllamaBenchmarks/PLANS.md) and the individual files in [plans](C:/Development/OllamaBenchmarks/plans) for the next benchmark phases.
