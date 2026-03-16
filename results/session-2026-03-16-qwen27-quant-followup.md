# 2026-03-16 Jackrong Qwen3.5-27B Quant Follow-Up

Tested a smaller follow-up quant for `Jackrong/Qwen3.5-27B-Claude-4.6-Opus-Reasoning-Distilled` using the existing `llama-server` benchmark path.

| Model | Source quant | tok/s | RAM peak (GB) | GPU mem peak (GB) | Quick quality | Notes |
| --- | --- | ---: | ---: | ---: | ---: | --- |
| `qwen3.5:27b-claude-4.6-opus-reasoning-distilled-q3_k_m` | `Q3_K_M` | 0.48 | 14.73 | 4.8 | 1/5 | First tested quant |
| `qwen3.5:27b-claude-4.6-opus-reasoning-distilled-q2_k` | `Q2_K` | 0.63 | 11.9 | 5.55 | 1/5 | Faster and lighter, but no quality improvement |

Takeaway:

- `Q2_K` is the more practical local quant on this host if the goal is simply to fit and respond.
- It does not improve the quick coding/tool/agent benchmark score relative to `Q3_K_M`.
- Based on the current suite, this model family still looks like a weak benchmark candidate compared with `omnicoder:9b-q4_k_m`, `granite4:7b-a1b-h`, or `qwen3.5:35b-a3b-q2_k_l`.

Primary artifacts:

- `results/throughput-resource-qwen3.5_27b-claude-4.6-opus-reasoning-distilled-q3_k_m.json`
- `results/quality-qwen3.5_27b-claude-4.6-opus-reasoning-distilled-q3_k_m.json`
- `results/throughput-resource-qwen3.5_27b-claude-4.6-opus-reasoning-distilled-q2_k.json`
- `results/quality-qwen3.5_27b-claude-4.6-opus-reasoning-distilled-q2_k.json`
