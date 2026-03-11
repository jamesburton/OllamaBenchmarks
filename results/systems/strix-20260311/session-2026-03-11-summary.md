# Session Summary: 2026-03-11

## Throughput and quick quality

| Model | tok/s | Quick quality (/4) | Notes |
|---|---:|---:|---|
| glm-4.7-flash:latest | 43.94 | 2 | Fastest raw throughput, still weak on coding tasks |
| lfm2:24b | 33.54 | 4 | Best speed/quality balance in this run |
| qwen3-coder-next:latest | 32.87 | 4 | Nearly tied with `lfm2:24b`, still the safest coding pick |
| granite4:32b-a9b-h | 26.13 | 4 | Strong coding/tool performance with lower throughput |
| ministral-3:14b | 21.97 | 4 | Smaller model with clean quality pass |
| gpt-oss:120b | 10.81 | 3 | One coding miss, high memory demand |
| glm-4.7-flash:bf16 | 7.17 | 2 | Tool use okay, coding still weak |
| qwen3-coder-next:q8_0 | 6.66 | 4 | Full quality pass, much slower than `latest` |
| qwen3.5:122b | 4.94 | 2 | Large footprint without competitive throughput |
| devstral-small-2:24b-instruct-2512-q8_0 | 3.50 | 4 | Quality solid, speed poor on this machine |
| MichelRosselli/GLM-4.5-Air:latest | 1.27 | 0 | Not competitive here |

## Backend comparison on qwen3-coder-next:latest

| Backend | tok/s | end-to-end tok/s |
|---|---:|---:|
| auto | 32.59 | 29.10 |
| rocm | 32.13 | 28.71 |
| vulkan | 6.55 | 6.15 |

## Option sweep on qwen3-coder-next:latest

| Variant | avg tok/s | avg total seconds | Notes |
|---|---:|---:|---|
| baseline | 32.76 | 75.468 | Best default end-to-end profile |
| threads_8 | 32.17 | 78.639 | Slightly slower than baseline |
| threads_16 | 29.91 | 101.759 | Clearly worse |
| batch_1024 | 32.83 | 163.951 | Similar decode speed, much worse total time |
| force_gpu_99 | 32.56 | 215.549 | No gain, worst total time |

## Recommendation

1. Use `qwen3-coder-next:latest` as the primary coding model.
2. Keep `lfm2:24b` as the highest-value alternative and best speed/quality tradeoff.
3. Use `granite4:32b-a9b-h` when you want another full-pass coding model.
4. Leave `OLLAMA_LLM_LIBRARY` on `auto` unless you specifically want to test `rocm`; both were close here and far ahead of `vulkan`.
