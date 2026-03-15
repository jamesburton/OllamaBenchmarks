# 2026-03-15 Model Additions Summary

Added two Hugging Face sourced models to the benchmark inventory:

| Model | Source quant | Backend used for benchmarks | tok/s | RAM peak (GB) | GPU mem peak (GB) | Quick quality |
| --- | --- | --- | ---: | ---: | ---: | ---: |
| `omnicoder:9b-q4_k_m` | `Tesslate/OmniCoder-9B-GGUF:Q4_K_M` | `llama-server` | 1.48 | 6.17 | 4.8 | 5/5 |
| `qwen3.5:27b-claude-4.6-opus-reasoning-distilled-q3_k_m` | `Jackrong/Qwen3.5-27B-Claude-4.6-Opus-Reasoning-Distilled-GGUF:Q3_K_M` | `llama-server` | 0.48 | 14.73 | 4.8 | 1/5 |

Notes:

- `omnicoder:9b-q4_k_m` imports into Ollama, but the local Ollama runner fails to allocate memory cleanly on this host. The `llama-server` path runs successfully and passes the current coding, tool, and agent quick checks.
- `qwen3.5:27b-claude-4.6-opus-reasoning-distilled-q3_k_m` imports into Ollama, but the local Ollama runner still reports an unsupported `qwen35` architecture. The `llama-server` path runs successfully, but the current quick checks only score `1/5`.

Primary artifacts:

- `results/throughput-resource-omnicoder_9b-q4_k_m.json`
- `results/quality-omnicoder_9b-q4_k_m.json`
- `results/throughput-resource-qwen3.5_27b-claude-4.6-opus-reasoning-distilled-q3_k_m.json`
- `results/quality-qwen3.5_27b-claude-4.6-opus-reasoning-distilled-q3_k_m.json`
