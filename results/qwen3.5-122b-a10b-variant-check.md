# Variant Check: qwen3.5:122b-a10b

Checked on 2026-03-11 alongside the existing `qwen3.5:122b` result because the `a10b` tag may differ in definition or runtime behavior.

## What matched

- `ollama show qwen3.5:122b-a10b` and `ollama show qwen3.5:122b` report the same architecture (`qwen35moe`), parameter count (`125.1B`), quantization (`Q4_K_M`), context length (`262144`), and Ollama requirement (`0.17.1`).
- Both tags passed the quick tool checks and failed the quick coding checks (`2/4` total score).

## What changed

| Tag | tok/s | Quick quality | Notes |
|---|---:|---:|---|
| `qwen3.5:122b-a10b` | 5.45 | 2/4 | Slightly faster than the older tag on the main throughput test |
| `qwen3.5:122b` | 4.94 | 2/4 | Previously benchmarked baseline |

## Additional checks for qwen3.5:122b-a10b

- Backend comparison artifact: [backend-comparison-qwen3.5-122b-a10b.json](C:/Development/OllamaBenchmarks/results/backend-comparison-qwen3.5-122b-a10b.json)
  After pinning `num_ctx=8192` and running the backend check on its own, all isolated backend runs completed.
  `auto` reached 5.49 tok/s, `rocm` reached 5.12 tok/s, and `vulkan` was dramatically slower at 0.73 tok/s.
- Sweep artifact: [optimization-sweep-qwen3.5-122b-a10b.json](C:/Development/OllamaBenchmarks/results/optimization-sweep-qwen3.5-122b-a10b.json)
  `baseline`, `threads_8`, `threads_16`, and `batch_1024` completed with `num_ctx=8192`.
  `force_gpu_99` failed with `memory layout cannot be allocated with num_gpu = 99`.

## Interpretation

This still looks like the same underlying model family, but not a perfectly identical runtime profile. The `a10b` tag is modestly faster on the base throughput prompt. The earlier isolated-backend failures were reproducible only when the model was tested with a huge implicit context or while a concurrent sweep was competing for memory; after constraining context length and running the backend check in isolation, the variant behaved normally on `auto` and `rocm`.
