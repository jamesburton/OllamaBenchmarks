---
name: ollama-docker-agent-deploy
description: Use when deploying Ollama models as Docker containers for use with Docker Agent, when selecting the best local models from benchmark results, or when creating docker-agent YAML configurations. Covers the full pipeline from benchmarking through containerised deployment.
---

# Ollama Docker Agent Deploy

End-to-end workflow: benchmark local models, select the best performers, deploy Ollama in Docker, and run Docker Agent configurations against them.

## When to Use

- Setting up a new machine for local LLM inference via Docker
- Selecting which Ollama models to deploy based on quality and speed
- Creating or updating Docker Agent YAML configs
- Deploying Ollama as a container with shared host model storage

## Prerequisites

- Docker Desktop (4.63+ for Docker Agent plugin)
- Ollama installed locally (for benchmarking and model storage)
- NVIDIA GPU drivers + NVIDIA Container Toolkit (for GPU passthrough)
- Python 3 on PATH, Windows PowerShell

## Step 1: Benchmark the Host

Run the benchmark suite against locally-installed models. Use the companion `ollama-benchmark-matrix` skill for detailed benchmark workflow, or run the essentials:

```powershell
# Pull models to benchmark
ollama pull glm-4.7-flash
ollama pull qwen3-coder-next
ollama pull qwen3.5:35b-a3b
# ... (see benchmark-models.json for full suite)

# Throughput + resource sampling
./scripts/benchmark_throughput_resource.ps1 `
  -Models @('glm-4.7-flash','qwen3-coder-next','qwen3.5:35b-a3b') `
  -CheckpointDir results

# Quality suite (coding, tool-use, agentic tasks — scored out of 5)
cd scripts && python benchmark_quality.py `
  --models "glm-4.7-flash" "qwen3-coder-next" "qwen3.5:35b-a3b" `
  --output ../results/quality-current.json --checkpoint-dir ../results

# Rebuild aggregates from per-model checkpoints
python rebuild_benchmark_aggregate.py `
  --benchmark throughput_resource `
  --output ../results/throughput-resource-current.json `
  --models "glm-4.7-flash" "qwen3-coder-next" "qwen3.5:35b-a3b"
```

## Step 2: Select Models

Filter for **5/5 quality** models, then rank by `toks_per_s`. Recommended roles:

| Role | Selection Criteria |
|------|--------------------|
| Fast general | Highest tok/s at 5/5 quality |
| Coding | Purpose-built coder at 5/5, or fastest 5/5 |
| Reasoning | MoE or large model with good speed at 5/5 |
| Small/fallback | Smallest 5/5 model (low VRAM for concurrent use) |

Example picks from Framework benchmarks:

| Role | Model | tok/s | Quality |
|------|-------|------:|--------:|
| Fast general | glm-4.7-flash | 43.16 | 5/5 |
| Coding | qwen3-coder-next | 32.55 | 5/5 |
| Reasoning | qwen3.5:35b-a3b | 39.90 | 5/5 |
| Small/fallback | granite4:7b-a1b-h | 26.91 | 5/5 |

Models scoring below 4/5 should not be deployed for agent use. Models with tok/s below ~5 are likely CPU-bound (exceed GPU VRAM) and will feel sluggish interactively.

## Step 3: Deploy Ollama Container

The docker-compose shares host model storage so pulled models are instantly available:

```yaml
# docker-agent/docker-compose.yaml
services:
  ollama:
    image: ollama/ollama:latest
    ports:
      - "11434:11434"
    volumes:
      # Maps host Ollama model cache into container — no re-download
      - ${USERPROFILE}/.ollama:/root/.ollama    # Windows
      # - ~/.ollama:/root/.ollama               # Linux/Mac
    deploy:
      resources:
        reservations:
          devices:
            - driver: nvidia
              count: all
              capabilities: [gpu]
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:11434/api/tags"]
      interval: 10s
      timeout: 5s
      retries: 3
```

```bash
cd docker-agent
docker compose up -d
# Verify: curl http://localhost:11434/api/tags
```

## Step 4: Create Docker Agent Config

Ollama is a **custom provider** using the OpenAI-compatible API:

```yaml
# agent.yaml
providers:
  ollama:
    api_type: openai_chatcompletions
    base_url: http://localhost:11434/v1
    # No token_key needed for local Ollama

models:
  default:
    provider: ollama
    model: glm-4.7-flash          # Your best benchmark pick
    max_tokens: 8192
    temperature: 0

agents:
  root:
    model: default
    description: Assistant using locally-benchmarked Ollama models
    instruction: |
      You are a helpful assistant.
    toolsets:
      - type: filesystem
      - type: shell
      - type: think
    add_date: true
    add_environment_info: true
```

For multi-agent setups, assign different benchmark-proven models to specialist roles via `sub_agents`. See `docker-agent/agent-multi.yaml` for a working example.

## Step 5: Run the Agent

```bash
docker agent run docker-agent/agent-coding.yaml
docker agent run docker-agent/agent-general.yaml
docker agent run docker-agent/agent-multi.yaml
```

## Pre-built Configs

This repo includes ready-to-use configs in `docker-agent/`:

| Config | Models | Use Case |
|--------|--------|----------|
| `agent-general.yaml` | glm-4.7-flash | Everyday tasks |
| `agent-coding.yaml` | qwen3-coder-next + qwen3.5:35b-a3b | Code + review |
| `agent-multi.yaml` | 4-model team | Complex multi-step work |
| `agent-small.yaml` | granite4:7b-a1b-h | Low-VRAM environments |

## Slug Convention

Model names use `:latest` as the implicit default tag. Omit it from slugs and config — `glm-4.7-flash` not `glm-4.7-flash:latest`. Non-default tags are kept: `qwen3.5:35b-a3b`, `nemotron-3-nano:30b-a3b-q8_0`.

## Common Issues

| Problem | Fix |
|---------|-----|
| Container can't find models | Check volume mount maps host `.ollama` dir correctly |
| GPU not used in container | Install NVIDIA Container Toolkit; check `docker run --gpus all nvidia-smi` |
| Agent timeout / very slow | Model exceeds VRAM — falls to CPU. Pick a smaller model |
| "Connection refused" from agent | Ensure Ollama container is running: `docker compose up -d` |
| Model not in Ollama | Run `ollama pull <model>` on host; container shares the storage |
