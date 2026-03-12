# Session Summary: 2026-03-11 (T5500 Xeon + RTX 3060)

## Throughput and quick quality

| Model | tok/s | Quick quality (/4) | Notes |
|---|---:|---:|---|
| lfm2.5-thinking:latest | 220.75 | 0 | Fastest raw throughput, failed the quick quality checks |
| ministral-3:14b | 29.68 | 4 | Best overall balance in this run |
| granite4:7b-a1b-h | 20.46 | 4 | Solid quality at moderate throughput |
| lfm2:24b | 11.67 | 4 | Stable quality, slower on this system |

## Backend comparison on granite4:7b-a1b-h

| Backend | tok/s | end-to-end tok/s |
|---|---:|---:|
| auto | 20.09 | 19.33 |
| rocm | 7.57 | 6.55 |
| vulkan | 7.19 | 6.24 |

## Option sweep on granite4:7b-a1b-h

| Variant | avg tok/s | avg total seconds | Notes |
|---|---:|---:|---|
| baseline | 20.28 | 9.805 | Best default end-to-end profile |
| threads_8 | 20.21 | 14.455 | Similar decode speed, slower total time |
| threads_16 | 20.83 | 14.24 | Similar decode speed, slower total time |
| batch_1024 | 20.6 | 14.155 | Similar decode speed, slower total time |
| force_gpu_99 | 19.64 | 15.175 | Worse total time |

## Not run (N/A due to RAM/VRAM constraints)

- devstral-small-2:24b-instruct-2512-q8_0
- glm-4.7-flash:bf16
- glm-4.7-flash:latest
- gpt-oss:120b
- granite4:32b-a9b-h
- MichelRosselli/GLM-4.5-Air:latest
- nemotron-3-super:latest
- qwen3.5:122b
- qwen3.5:122b-a10b
- qwen3-coder-next:latest
- qwen3-coder-next:q8_0

## Recommendation

1. Use `ministral-3:14b` as the primary coding model on this system.
2. Keep `granite4:7b-a1b-h` as the next best quality option.
3. Treat `lfm2.5-thinking:latest` as a speed-only option (quality failures in this quick suite).
4. Leave `OLLAMA_LLM_LIBRARY` on `auto`; it was markedly faster than `rocm` and `vulkan` here.
