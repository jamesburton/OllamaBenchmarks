# Llama-Server Session Summary (2026-03-13)

## Summary

This session added an OpenAI-compatible benchmark path using `llama-server` so GGUF models that are blocked in Ollama can still be measured with the repo's compact throughput and quality checks.

## Models tested

| Model | Backend | tok/s | RAM peak (GB) | GPU mem peak (GB) | Quick quality | Notes |
| --- | --- | ---: | ---: | ---: | ---: | --- |
| `qwen3.5:35b-a3b-q2_k_l` | `llama-server` | 3.28 | 12.8 | 3.71 | 5/5 | Imported Bartowski `Q2_K_L` GGUF; full pass on coding, tool use, and agentic checks |
| `lfm2:24b` | `llama-server` | 4.36 | 14.1 | 4.11 | 1/5 | Faster than Qwen on this path, but collapsed on tool and agentic tasks |
| `granite4:7b-a1b-h` | `llama-server` | 6.48 | 4.54 | 4.13 | 5/5 | Fastest of the three while still keeping a full quick-pass |

## Interpretation

- `qwen3.5:35b-a3b-q2_k_l` is viable on this machine when served through `llama-server`.
- The same quant remains blocked in Ollama because local Ollama builds currently fail to load `qwen35moe`.
- `granite4:7b-a1b-h` is the strongest value result in the direct `llama.cpp` path so far.
- `lfm2:24b` does not currently transfer well to this backend for tool-heavy tasks.

## Artifacts

- Qwen throughput: `results/throughput-openai-20260313-171042.json`
- Qwen quality: `results/quality-openai-20260313-171156.json`
- LFM2 throughput: `results/throughput-openai-20260313-203028.json`
- LFM2 quality: `results/quality-openai-20260313-203109.json`
- Granite throughput: `results/throughput-openai-20260313-203359.json`
- Granite quality: `results/quality-openai-20260313-203436.json`

## Backend note

- `qwen3.5:35b-a3b-q2_k_l` should currently be treated as `llama-server`-only on this host.
- `ministral-3:14b` was not included in the comparison table because the Ollama-exported blob did not load cleanly in direct `llama-server`.
