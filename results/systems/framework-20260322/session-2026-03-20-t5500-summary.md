# T5500 Benchmark Summary (2026-03-20)

## System
- **Host:** Dell Precision WorkStation T5500
- **CPU:** Intel Xeon (24 logical cores)
- **RAM:** 36 GB
- **GPU:** NVIDIA GeForce RTX 3060 (12 GB VRAM)
- **Ollama:** 0.18.1
- **OS:** Windows 11 Pro 10.0.26200

## Top Models (5/5 Quality, ranked by PPL proxy)

| Rank | Model | tok/s | Quality | PPL Proxy | VRAM | Highlight |
|------|-------|------:|--------:|----------:|-----:|-----------|
| 1 | cogito:14b | 34.36 | 5/5 | 3.47 | 9.05 GB | Best text fidelity |
| 2 | qwen3.5:9b | 38.87 | 5/5 | 3.89 | 8.2 GB | Best speed+quality |
| 3 | qwen3:8b | 48.65 | 5/5 | 4.08 | ~5 GB | Fast + quality |
| 4 | qwen3.5:4b | 55.66 | 5/5 | 4.62 | 5.73 GB | Best for tight VRAM |

## All Working Models

| Model | tok/s | Quality | VRAM | PPL Proxy | Notes |
|-------|------:|--------:|-----:|----------:|-------|
| phi4-mini:latest | 93.05 | 1/5 | 3.06 GB | - | Fastest raw speed |
| cogito:8b | 59.93 | 3/5 | 5.09 GB | - | Fast, misses tools |
| qwen3.5:4b | 55.66 | 5/5 | 5.73 GB | 4.62 | Best value |
| qwen3:8b | 48.65 | 5/5 | ~5 GB | 4.08 | |
| qwen3.5:9b | 38.87 | 5/5 | 8.2 GB | 3.89 | Champion tier |
| cogito:14b | 34.36 | 5/5 | 9.05 GB | 3.47 | Best PPL |
| zac/phi4-tools | 31.86 | 2/5 | 9.38 GB | - | Partial tool support |
| ministral-3:14b | 29.68 | 4/5 | ~9 GB | - | |
| granite4:7b-a1b-h | ~20 | 4/5 | 4.2 GB | - | |
| lfm2:24b | 11.67 | 4/5 | ~14 GB | - | CPU offload needed |

## Models Removed (incompatible with T5500)

| Model | Reason |
|-------|--------|
| qwen3.5:27b-* (all 27B variants) | Ollama 0.18.1 unknown architecture error |
| qwen3.5:35b-a3b-q2_k_l | llama-server only (old build, no qwen35) |
| omnicoder:9b-q4_k_m | llama-server only (old build) |
| nemotron-3-nano:30b | Too large for 12GB VRAM + 36GB RAM |
| jacob-ebey/phi4-tools | 0/5 quality, duplicate blob |
| lfm2.5-thinking:latest | 0/5 quality |

## Key Findings

1. **Ollama 0.18.1 was critical** - fixed memory allocation bugs that blocked all Qwen 3/3.5 models on 0.18.0
2. **qwen3.5:4b is remarkable** - 5/5 quality at 55.66 tok/s using only 5.73 GB VRAM
3. **cogito:14b has best text fidelity** (PPL 3.47) but qwen3.5:9b offers better speed
4. **27B qwen35 models don't load** on Ollama 0.18.1 (architecture detection bug)
5. **Disk cleanup freed ~73 GB** by removing incompatible models
