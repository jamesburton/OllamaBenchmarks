# OllamaBenchmarks

Local benchmark harness and captured results for evaluating Ollama models across multiple Windows hosts.

## What is in this repo

- `results/` contains the current benchmark outputs plus archived system snapshots.
- `benchmark-models.json` tracks the benchmark-suite models, locally installed models, and the combined effective benchmark list for the current machine.
- `scripts/benchmark_throughput_resource.ps1` measures generation speed and samples CPU, RAM, GPU utilization, and GPU dedicated memory.
- `scripts/benchmark_quality.py` runs a compact coding, tool-use, and agent-orchestration quality suite.
- `scripts/benchmark_backend.ps1` compares `auto`, `vulkan`, and `rocm` backend selection by launching isolated Ollama servers on alternate ports, and now preflights desktop-managed models so those temporary servers can resolve them reliably.
- `scripts/benchmark_sweep.py` runs decode-side option sweeps for a single model.
- `scripts/benchmark_throughput_openai.py` measures throughput and host-side resource usage against an OpenAI-compatible backend such as `llama-server`.
- `scripts/benchmark_quality_openai.py` runs the compact coding, tool-use, and agent-orchestration quality suite against an OpenAI-compatible backend such as `llama-server`.
- Multi-model throughput and quality runs now checkpoint one JSON artifact per model and keep the combined `*-current.json` refreshed as they progress.
- `scripts/collect_host_info.py` captures machine metadata for archived benchmark snapshots.
- `scripts/archive_system_benchmarks.py` snapshots the current benchmark artifacts under `results/systems/<host>-<label>/`.
- `scripts/build_cross_system_summary.py` rebuilds the comparison table across all archived systems.
- All benchmark scripts now auto-write a structured JSON artifact under `results/` even if you do not pass an output path.
- The reusable agent workflow for this repo lives in [skills/ollama-benchmark-matrix/SKILL.md](skills/ollama-benchmark-matrix/SKILL.md).
- `plans/` contains the next-step benchmark expansion work.

## Current headline findings (Framework, 2026-03-22)

**21 models** score 5/5 quality. Top performers on Framework (Core Ultra + RTX 3060 12GB):

| Model | tok/s | Quality | Role |
|-------|------:|--------:|------|
| `glm-4.7-flash` | 43.16 | 5/5 | **Best balanced** — fastest perfect-quality model |
| `qwen3.5:35b-a3b` | 39.90 | 5/5 | MoE, excellent speed for size |
| `nemotron-3-nano:30b-a3b-q8_0` | 36.89 | 5/5 | MoE, strong throughput |
| `qwen3-coder-next` | 32.55 | 5/5 | **Best coding model** |
| `nemotron-cascade-2` | 27.83 | 5/5 | Cascade architecture |
| `granite4:7b-a1b-h` | 26.91 | 5/5 | Tiny MoE, great value |

49 throughput / 55 quality results total across the Framework benchmark capture.

See [results/cross-system-summary.md](results/cross-system-summary.md) for the rolling matrix across archived hosts.
See [results/current-best-summary.md](results/current-best-summary.md) for the current best quality/speed rollup.

## Quick start

Prerequisites:

- Windows PowerShell
- Python 3 on `PATH`
- Ollama running locally on `http://127.0.0.1:11434`
- Models already pulled with `ollama pull <model>`
- For `llama-server` runs, a compatible OpenAI-style endpoint such as `http://127.0.0.1:8081/v1/chat/completions`

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

### OpenAI-compatible backend benchmark

Use this path for models that run in `llama-server` but not in Ollama yet. The OpenAI-compatible scripts target `/v1/chat/completions` and write the same style of JSON artifacts under `results/`.

Throughput example:

```powershell
python .\scripts\benchmark_throughput_openai.py `
  --model qwen3.5:35b-a3b-q2_k_l `
  --base-url http://127.0.0.1:8081
```

Quality example:

```powershell
python .\scripts\benchmark_quality_openai.py `
  --models qwen3.5:35b-a3b-q2_k_l `
  --base-url http://127.0.0.1:8081
```

One-command launcher example:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\run_llama_server_benchmarks.ps1 `
  -Models qwen3.5:35b-a3b-q2_k_l granite4:7b-a1b-h `
  -LlamaServerPath C:\path\to\llama-server.exe
```

Current backend note:

- `qwen3.5:35b-a3b-q2_k_l` is currently a `llama-server` model on this machine. It passes the compact tool/coding/agent checks there, but Ollama `0.17.8-rc4` still fails to load `qwen35moe`.

Recent `llama-server` comparison on this T5500 host:

| Model | Backend | tok/s | RAM peak (GB) | GPU mem peak (GB) | Quick quality | Notes |
| --- | --- | ---: | ---: | ---: | ---: | --- |
| `omnicoder:9b-q4_k_m` | `llama-server` | 1.48 | 6.17 | 4.8 | 5/5 | Strong coding/tool result from Tesslate's OmniCoder GGUF despite Ollama runner limits on this host |
| `qwen3.5:35b-a3b-q2_k_l` | `llama-server` | 3.28 | 12.8 | 3.71 | 5/5 | Strongest large-model result in this path so far |
| `lfm2:24b` | `llama-server` | 4.36 | 14.1 | 4.11 | 1/5 | Faster than Qwen here, but weak on tool and agent checks |
| `granite4:7b-a1b-h` | `llama-server` | 6.48 | 4.54 | 4.13 | 5/5 | Best speed/quality balance in the direct llama.cpp path |

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

## Deploying as Docker Agent with containerised Ollama

This repo includes ready-to-use Docker Agent configs that use the best benchmark-proven models.

### 1. Start the Ollama container

The compose file shares your host's Ollama model cache so there's no re-download:

```bash
cd docker-agent
docker compose up -d
```

This maps `~/.ollama` (or `%USERPROFILE%\.ollama` on Windows) into the container and passes through all NVIDIA GPUs.

### 2. Run a Docker Agent

```bash
docker agent run docker-agent/agent-general.yaml    # Fast general assistant (glm-4.7-flash)
docker agent run docker-agent/agent-coding.yaml     # Coding agent + reviewer
docker agent run docker-agent/agent-multi.yaml      # 4-agent specialist team
docker agent run docker-agent/agent-small.yaml      # Low-VRAM environments
```

All model selections are driven by benchmark results — only 5/5 quality models are used.

### Pre-built configurations

| Config | Models | Use Case |
|--------|--------|----------|
| `docker-agent/agent-general.yaml` | glm-4.7-flash (43 tok/s) | Everyday tasks |
| `docker-agent/agent-coding.yaml` | qwen3-coder-next + qwen3.5:35b-a3b | Code writing + review |
| `docker-agent/agent-multi.yaml` | 4-model team | Complex multi-step work |
| `docker-agent/agent-small.yaml` | granite4:7b-a1b-h (27 tok/s) | Constrained environments |

### How it works

Ollama exposes an OpenAI-compatible API. Docker Agent connects via custom provider:

```yaml
providers:
  ollama:
    api_type: openai_chatcompletions
    base_url: http://localhost:11434/v1
```

See `docker-agent/` for full configs and `docker-compose.yaml`.

## End-to-end workflow: benchmark, select, deploy

To set up a new machine from scratch:

1. **Pull models** — `ollama pull <model>` for each candidate
2. **Benchmark** — run throughput + quality scripts (see Quick Start above)
3. **Select** — filter for 5/5 quality, rank by tok/s
4. **Deploy** — start Ollama container with `docker compose up -d`
5. **Run agents** — `docker agent run <config>.yaml`

The `ollama-docker-agent-deploy` skill automates guidance for this pipeline.

## Skills

This repo includes project-local skills for Claude Code / Codex:

| Skill | Purpose |
|-------|---------|
| `skills/ollama-benchmark-matrix` | Run benchmarks, archive snapshots, maintain cross-system matrix |
| `skills/ollama-docker-agent-deploy` | Select best models, deploy Ollama container, create and run Docker Agent configs |

To copy a skill to your user-level skills (available across all projects) or to another project, use the `skills-guru` skill:

```
/skills-guru install skills/ollama-docker-agent-deploy --scope user
```

## Next work

See [PLANS.md](PLANS.md) and the individual files in [plans/](plans/) for the next benchmark phases.
