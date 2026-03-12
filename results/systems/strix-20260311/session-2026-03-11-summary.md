# Session Summary: 2026-03-11

## Throughput and quick quality

| Model | tok/s | Quick quality (/5) | Notes |
|---|---:|---:|---|
| glm-4.7-flash:latest | 43.11 | 5 | Fastest model in the suite and now a full quick-pass on the expanded checks |
| qwen3-coder-next:latest | 33.92 | 5 | Strongest coding-oriented option with a full quick-pass |
| lfm2:24b | 31.78 | 4 | Still a strong lower-footprint option, missed the new agentic check |
| granite4:32b-a9b-h | 26.12 | 4 | One coding miss in this rerun |
| ministral-3:14b | 21.94 | 5 | Smallest of the top full-pass group |
| gpt-oss:120b | 11.12 | 5 | Passed the expanded suite, but remains expensive in memory and latency |
| qwen3-coder-next:q8_0 | 7.96 | 5 | Full quick-pass, much slower than `latest` |
| glm-4.7-flash:bf16 | 7.09 | 2 | Still weak relative to its `latest` tag |
| nemotron-3-super:latest | 5.84 | 5 | Full quick-pass, but slower than the leading models |
| qwen3.5:122b | 5.22 | 5 | Full quick-pass in the expanded suite, but not competitive on speed |
| qwen3.5:122b-a10b | 5.21 | 5 | Essentially tied with `qwen3.5:122b`; see the variant-check artifact for backend details |
| devstral-small-2:24b-instruct-2512-q8_0 | 3.98 | 5 | Full quick-pass, speed still poor here |
| MichelRosselli/GLM-4.5-Air:latest | 1.27 | 2 | Still not competitive here |

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
2. If raw speed matters most and the expanded quick suite is enough, `glm-4.7-flash:latest` is now the best all-around throughput result.
3. Keep `lfm2:24b` as the best lower-footprint alternative, noting that it missed the new agentic orchestration check.
4. Leave `OLLAMA_LLM_LIBRARY` on `auto`; `rocm` stayed close, but `auto` remained the best default and `vulkan` was far behind.
