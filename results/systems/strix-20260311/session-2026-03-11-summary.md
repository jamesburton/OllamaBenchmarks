# Session Summary: 2026-03-11

## Throughput and quick quality

| Model | tok/s | Quick quality (/4) | Notes |
|---|---:|---:|---|
| glm-4.7-flash:latest | 43.11 | 2 | Fastest raw throughput, still weak on coding tasks |
| qwen3-coder-next:latest | 33.92 | 4 | Best overall result in this refresh |
| lfm2:24b | 31.78 | 4 | Slightly slower than `qwen3-coder-next:latest`, still a strong lower-footprint option |
| granite4:32b-a9b-h | 26.12 | 3 | Lost one coding pass in this rerun |
| ministral-3:14b | 21.94 | 4 | Smaller model with a clean quality pass |
| gpt-oss:120b | 11.12 | 3 | One coding miss, high memory demand |
| qwen3-coder-next:q8_0 | 7.96 | 4 | Good quality, much slower than `latest` |
| glm-4.7-flash:bf16 | 7.09 | 2 | Tool use okay, coding still weak |
| nemotron-3-super:latest | 5.84 | 3 | Large model, better than the `qwen3.5:122b` tags here but not competitive with the top coding options |
| qwen3.5:122b | 5.22 | 2 | Very large footprint without competitive quality |
| qwen3.5:122b-a10b | 5.21 | 2 | Essentially tied with `qwen3.5:122b`; see the variant-check artifact for backend details |
| devstral-small-2:24b-instruct-2512-q8_0 | 3.98 | 4 | Quality solid, speed still poor here |
| MichelRosselli/GLM-4.5-Air:latest | 1.27 | 0 | Not competitive here |

## Backend comparison on qwen3-coder-next:latest

| Backend | tok/s | end-to-end tok/s |
|---|---:|---:|
| auto | 33.89 | 30.06 |
| rocm | 33.10 | 28.96 |
| vulkan | 8.04 | 7.61 |

## Option sweep on qwen3-coder-next:latest

| Variant | avg tok/s | avg total seconds | Notes |
|---|---:|---:|---|
| baseline | 33.49 | 5.710 | Best end-to-end profile in this refreshed run |
| threads_8 | 33.55 | 29.896 | No throughput gain worth the latency cost |
| threads_16 | 33.44 | 29.249 | Similar decode speed, worse end-to-end |
| batch_1024 | 33.94 | 28.161 | Highest decode speed, much worse total time |
| force_gpu_99 | 33.87 | 29.533 | No practical gain over baseline |

## Recommendation

1. Use `qwen3-coder-next:latest` as the primary coding model.
2. Keep `lfm2:24b` as the best lower-footprint alternative.
3. Use `ministral-3:14b` or `qwen3-coder-next:q8_0` if you want another full-pass option from this refreshed run.
4. Leave `OLLAMA_LLM_LIBRARY` on `auto`; `rocm` stayed close, but `auto` remained the best default and `vulkan` was far behind.
