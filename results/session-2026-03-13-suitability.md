# Local Suitability Check (2026-03-13)

## Size-based fit on this machine

| Model | Ollama size | Suitability | Notes |
|---|---:|---|---|
| lfm2:24b | 14 GB | Suitable by disk/RAM | Attempted local pull; download completed but Ollama never registered the model after a prolonged digest-verification phase |
| granite4:32b-a9b-h | 19 GB | Suitable by disk/RAM | Deferred after the lfm2 install path failed |
| glm-4.7-flash:latest | 19 GB | Suitable by disk/RAM | Deferred after the lfm2 install path failed |
| glm-4.7-flash:bf16 | 60 GB | Not suitable | Too large for a safe local pull on current free space |
| qwen3-coder-next:latest | 52 GB | Not suitable | Too large for a safe local pull on current free space |
| qwen3-coder-next:q8_0 | 85 GB | Not suitable | Too large for a safe local pull on current free space |

## Local pull result

- `ollama pull lfm2:24b` transferred the full 14 GB payload, then remained stuck in `verifying sha256 digest` for an extended period.
- During that state, `ollama list` still did not expose `lfm2:24b`, no usable manifest appeared under `C:\Users\james\.ollama\models\manifests`, and free space rebounded after the interrupted run.
- I stopped the stuck verification instead of burning more time on the same broken install path.

## Outcome

- No additional missing benchmark-suite model completed a local benchmark run in this pass.
- The repo changes in this pass are the backend-comparison fix, summary/doc alignment, and this suitability record.
