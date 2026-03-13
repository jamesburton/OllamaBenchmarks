# Session Summary (2026-03-12)

## Installed-model sweep

| Model | Throughput (tok/s) | Quality | Notes |
|---|---:|---:|---|
| lfm2.5-thinking:1.2b | 205.66 | 0/5 | Fastest by a wide margin, but failed the compact coding, tool-use, and agentic checks |
| rnj-1:8b | 46.60 | 4/5 | Strong speed/result mix, missed the agentic orchestration task |
| qwen3.5:latest | 37.71 | 5/5 | Best overall coding-oriented local model on this machine |
| ministral-3:14b | 32.14 | 5/5 | Full quality pass, but slower than qwen3.5:latest |
| codellama:70b | incomplete | incomplete | Did not complete under the current throughput or quality harness |

## Backend comparison on qwen3.5:latest

| Backend | Avg tok/s | Avg end-to-end tok/s | Total s | Notes |
|---|---:|---:|---:|---|
| auto | 2.97 | 2.81 | 59.526 | Best result on this machine |
| vulkan | 2.29 | 2.15 | 77.661 | Slower than auto |
| rocm | 2.33 | 2.16 | 77.335 | Slightly ahead of vulkan, but still behind auto |

## Option sweep on qwen3.5:latest

| Variant | Avg tok/s | Avg total s | Notes |
|---|---:|---:|---|
| baseline | 37.76 | 6.409 | Best overall baseline |
| threads_8 | 37.85 | 15.002 | Negligible decode gain, much worse end-to-end time |
| threads_16 | 37.65 | 14.782 | No improvement over baseline |
| batch_1024 | 37.62 | 14.356 | No material gain over baseline |
| force_gpu_99 | 37.42 | 15.074 | Slightly worse than baseline |

## Recommendations

1. Use `qwen3.5:latest` as the primary local coding model on this machine.
2. Keep `ministral-3:14b` as a second full-pass option when you want a different model family.
3. Use `rnj-1:8b` when speed matters more than the agentic benchmark.
4. Do not treat `lfm2.5-thinking:1.2b` as a coding benchmark winner despite its raw throughput.
5. Keep `OLLAMA_LLM_LIBRARY` unset (`auto`) on this machine unless a future Ollama release materially changes backend behavior.
