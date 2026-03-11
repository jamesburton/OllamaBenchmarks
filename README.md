# OllamaBenchmarks

Local benchmark harness and captured results for evaluating Ollama models on a Strix Halo 128 GB Windows system.

## What is in this repo

- `results/` contains reconstructed benchmark outputs from the March 11, 2026 session.
- `scripts/benchmark_throughput_resource.ps1` measures generation speed and samples CPU, RAM, GPU utilization, and GPU dedicated memory.
- `scripts/benchmark_quality.py` runs a compact coding and tool-calling quality suite.
- `scripts/benchmark_backend.ps1` compares `auto`, `vulkan`, and `rocm` backend selection by launching isolated Ollama servers on alternate ports.
- `scripts/benchmark_sweep.py` runs decode-side option sweeps for a single model.
- `plans/` contains the next-step benchmark expansion work.

## Current headline findings

- Best overall coding model from this session: `qwen3-coder-next:latest`
- Best value contender: `lfm2:24b`
- Best raw throughput: `glm-4.7-flash:latest`, but it underperformed on the quick quality checks used here
- For this machine, forcing `vulkan` or `rocm` was slower than leaving `OLLAMA_LLM_LIBRARY` unset (`auto`)

See [results/session-2026-03-11-summary.md](C:/Development/OllamaBenchmarks/results/session-2026-03-11-summary.md) for the consolidated table and interpretation.

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
  -NumPredict 192 `
  -OutputPath .\results\fresh-throughput.json
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
  --models qwen3-coder-next:latest lfm2:24b granite4:32b-a9b-h `
  --output .\results\fresh-quality.json
```

This suite currently checks:

- Small executable coding tasks
- Single-call tool invocation correctness

### Backend comparison

Example:

```powershell
./scripts/benchmark_backend.ps1 `
  -Model qwen3-coder-next:latest `
  -OutputPath .\results\fresh-backends.json
```

This launches temporary Ollama servers on alternate ports and compares:

- `auto`
- `vulkan`
- `rocm`

### Option sweep

Example:

```powershell
python .\scripts\benchmark_sweep.py `
  --model qwen3-coder-next:latest `
  --output .\results\fresh-sweep.json
```

Default sweep variants:

- baseline
- `num_thread=8`
- `num_thread=16`
- `num_batch=1024`
- `num_gpu=99`

## How to read the results

- Prefer `toks_per_s` for steady-state decode speed.
- Use end-to-end time when comparing user experience, because some settings increase total latency without helping real throughput.
- Treat the quick quality suite as a screening pass, not a final model quality verdict.
- Re-run the same prompt set before making model changes or backend changes.

## Missing or reconstructed artifacts

No previously saved benchmark artifacts from the parent `C:\Development` workspace were found for this session, so the files in `results/` were reconstructed directly from the measured values produced during the session.

## Next work

See [PLANS.md](C:/Development/OllamaBenchmarks/PLANS.md) and the individual files in [plans](C:/Development/OllamaBenchmarks/plans) for the next benchmark phases.

