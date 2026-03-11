# Session Summary: 2026-03-11

## Throughput and quick quality

| Model | tok/s | Quick quality (/2) | Notes |
|---|---:|---:|---|
| glm-4.7-flash:latest | 44.99 | 0 | Fastest raw throughput, weak coding/tool sanity |
| qwen3-coder-next:latest | 32.78 | 2 | Best balanced performer |
| granite4:32b-a9b-h | 26.12 | 2 | Strong general coding/tool performance |
| lfm2:24b | 24.98 | 2 | Strong quality and efficient footprint |
| ministral-3:14b | 22.09 | 2 | Good speed and small size |
| gpt-oss:120b | 10.66 | 1 | Tool pass, coding output unstable in quick test |
| qwen3-coder-next:q8_0 | 8.73 | 2 | Good quality, much slower than latest |
| glm-4.7-flash:bf16 | 6.87 | 1 | Tool pass, coding fail in quick pass |
| qwen3.5:122b | 5.38 | 0 | Slow on this system |
| devstral-small-2:24b-instruct-2512-q8_0 | 3.71 | 2 | Quality good, speed poor here |
| MichelRosselli/GLM-4.5-Air:latest | 1.49 | 0 | CPU-heavy and not competitive here |

## Expanded quality on top three

| Model | Score |
|---|---:|
| qwen3-coder-next:latest | 5/5 |
| lfm2:24b | 5/5 |
| granite4:32b-a9b-h | 4/5 |

## Backend comparison on qwen3-coder-next:latest

| Backend | tok/s | end-to-end tok/s |
|---|---:|---:|
| auto | 33.69 | 30.49 |
| vulkan | 8.38 | 7.97 |
| rocm | 7.03 | 6.81 |

## Recommendation

1. Use `qwen3-coder-next:latest` as the primary coding model.
2. Keep `lfm2:24b` as the best value and fallback option.
3. Use `granite4:32b-a9b-h` as the third-ranked alternative.
4. Leave `OLLAMA_LLM_LIBRARY` unset and let Ollama choose `auto`.

