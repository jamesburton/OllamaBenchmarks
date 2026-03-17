# Session Summary (2026-03-16)

## Installed-model sweep on OldFramework

Host:

- Framework Laptop
- Intel Core i7-1165G7
- 63.79 GB RAM
- Intel Iris Xe Graphics
- Ollama 0.18.0

All five shortlisted models completed both throughput and quality runs on this machine.

| Model | Throughput (tok/s) | Quality | Notes |
|---|---:|---:|---|
| `lfm2:24b` | 13.95 | 4/5 | Fastest model in this local sweep, but still missed the agentic task |
| `granite4:7b-a1b-h` | 9.18 | 5/5 | Best overall speed/quality balance on this machine |
| `rnj-1:8b` | 4.63 | 4/5 | Respectable speed, but missed the agentic task again |
| `ministral-3:14b` | 2.85 | 5/5 | Reliable full-pass option, but slower than Granite here |
| `qwen3.5:latest` | 2.56 | 5/5 | Full-pass coding model, but unexpectedly slow on this CPU-only Iris Xe setup |

## Backend comparison on `qwen3.5:latest`

| Backend | Avg tok/s | Avg end-to-end tok/s | Total s | Notes |
|---|---:|---:|---:|---|
| `auto` | 2.57 | 2.34 | 71.228 | Best result |
| `vulkan` | 2.54 | 2.22 | 75.057 | Close on decode, worse end-to-end |
| `rocm` | 2.27 | 2.09 | 79.833 | Slowest result |

## Option sweep on `qwen3.5:latest`

| Variant | Avg tok/s | Avg total s | Notes |
|---|---:|---:|---|
| `force_gpu_99` | 2.75 | 66.650 | Best result in this sweep |
| `batch_1024` | 2.69 | 72.366 | Slight win over baseline |
| `baseline` | 2.47 | 73.375 | Stable default |
| `threads_8` | 1.08 | 623.202 | Much worse than baseline |
| `threads_16` | request failed | request failed | Timed out twice |

## Recommendations

1. Use `granite4:7b-a1b-h` as the best local speed/quality balance on this machine.
2. Keep `ministral-3:14b` as the conservative full-pass alternative.
3. Use `qwen3.5:latest` when you specifically want that model family, but expect much lower speed than the stronger archived Framework box.
4. Treat `lfm2:24b` and `rnj-1:8b` as speed-oriented second-line options that still miss the agentic check.
5. Keep backend selection on `auto` for `qwen3.5:latest`.
6. If you optimize `qwen3.5:latest` on this host, try `num_gpu=99` before experimenting with higher thread counts.
