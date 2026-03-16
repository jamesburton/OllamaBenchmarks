# Current Best Summary

Generated from the latest per-model throughput and quality artifacts present in `results/`.

## Fastest Full-Pass Models

| Model | tok/s | Score | RAM peak (GB) | GPU mem peak (GB) | Throughput artifact | Quality artifact |
| --- | ---: | ---: | ---: | ---: | --- | --- |
| `nemotron-3-nano:latest` | 58.99 | 5/5 | 2.04 | 26.84 | `results/throughput-resource-nemotron-3-nano_latest.json` | `results/quality-nemotron-3-nano_latest.json` |
| `glm-4.7-flash:latest` | 43.16 | 5/5 | 2.39 | 38.76 | `results/throughput-resource-glm-4.7-flash_latest.json` | `results/quality-glm-4.7-flash_latest.json` |
| `qwen3.5:latest` | 37.71 | 5/5 | 0.05 | 0.0 | `results/throughput-resource-qwen3.5_latest.json` | `results/quality-qwen3.5_latest.json` |
| `nemotron-3-nano:30b-a3b-q8_0` | 36.89 | 5/5 | 8.19 | 62.89 | `results/throughput-resource-nemotron-3-nano_30b-a3b-q8_0.json` | `results/quality-nemotron-3-nano_30b-a3b-q8_0.json` |
| `qwen3-coder-next:latest` | 32.55 | 5/5 | 1.6 | 57.85 | `results/throughput-resource-qwen3-coder-next_latest.json` | `results/quality-qwen3-coder-next_latest.json` |
| `ministral-3:14b` | 32.14 | 5/5 | 0.05 | 0.0 | `results/throughput-resource-ministral-3_14b.json` | `results/quality-ministral-3_14b.json` |
| `cogito:14b` | 28.88 | 5/5 | 0.09 | 0.0 | `results/throughput-resource-requested-20260313.json` | `results/quality-requested-20260313.json` |
| `granite4:32b-a9b-h` | 26.05 | 5/5 | 19.7 | 23.64 | `results/throughput-resource-granite4_32b-a9b-h.json` | `results/quality-granite4_32b-a9b-h.json` |
| `gpt-oss:120b` | 17.64 | 5/5 | 6.88 | 61.96 | `results/throughput-resource-gpt-oss_120b.json` | `results/quality-gpt-oss_120b.json` |
| `nemotron-3-super:latest` | 9.45 | 5/5 | 29.46 | 62.94 | `results/throughput-resource-nemotron-3-super_latest.json` | `results/quality-nemotron-3-super_latest.json` |

## Best Overall By Quick Score Then Speed

| Model | tok/s | Score | Coding | Tool | Agentic |
| --- | ---: | ---: | ---: | ---: | ---: |
| `nemotron-3-nano:latest` | 58.99 | 5/5 | 2/2 | 2/2 | 1/1 |
| `glm-4.7-flash:latest` | 43.16 | 5/5 | 2/2 | 2/2 | 1/1 |
| `qwen3.5:latest` | 37.71 | 5/5 | 2/2 | 2/2 | 1/1 |
| `nemotron-3-nano:30b-a3b-q8_0` | 36.89 | 5/5 | 2/2 | 2/2 | 1/1 |
| `qwen3-coder-next:latest` | 32.55 | 5/5 | 2/2 | 2/2 | 1/1 |
| `ministral-3:14b` | 32.14 | 5/5 | 2/2 | 2/2 | 1/1 |
| `cogito:14b` | 28.88 | 5/5 | 2/2 | 2/2 | 1/1 |
| `granite4:32b-a9b-h` | 26.05 | 5/5 | 2/2 | 2/2 | 1/1 |
| `gpt-oss:120b` | 17.64 | 5/5 | 2/2 | 2/2 | 1/1 |
| `nemotron-3-super:latest` | 9.45 | 5/5 | 2/2 | 2/2 | 1/1 |
| `qwen3-coder-next:q8_0` | 7.34 | 5/5 | 2/2 | 2/2 | 1/1 |
| `qwen3.5:122b` | 7.17 | 5/5 | 2/2 | 2/2 | 1/1 |
| `qwen3.5:122b-a10b` | 6.93 | 5/5 | 2/2 | 2/2 | 1/1 |
| `devstral-small-2:24b-instruct-2512-q8_0` | 6.43 | 5/5 | 2/2 | 2/2 | 1/1 |
| `granite4:7b-a1b-h` | 5.86 | 5/5 | 2/2 | 2/2 | 1/1 |

## Notes

- Fastest full-pass model in the current artifact set: `nemotron-3-nano:latest` at `58.99` tok/s.
- `omnicoder:9b-q4_k_m`: `5/5` at `1.48` tok/s.
- `qwen3.5:35b-a3b-q2_k_l`: `5/5` at `3.28` tok/s.
- `granite4:7b-a1b-h`: `5/5` at `5.86` tok/s.
- `qwen3.5:27b-claude-4.6-opus-reasoning-distilled-q3_k_m`: `1/5` at `0.48` tok/s.
- `qwen3.5:27b-claude-4.6-opus-reasoning-distilled-q2_k`: `1/5` at `0.63` tok/s.
