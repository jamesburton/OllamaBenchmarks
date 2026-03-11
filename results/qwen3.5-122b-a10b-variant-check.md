# Variant Check: qwen3.5:122b-a10b

Checked on 2026-03-11 alongside the existing `qwen3.5:122b` result because the `a10b` tag may differ in definition or runtime behavior.

## What matched

- `ollama show qwen3.5:122b-a10b` and `ollama show qwen3.5:122b` report the same architecture (`qwen35moe`), parameter count (`125.1B`), quantization (`Q4_K_M`), context length (`262144`), and Ollama requirement (`0.17.1`).
- Both tags passed the quick tool checks and failed the quick coding checks (`2/4` total score).

## What changed

| Tag | tok/s | Quick quality | Notes |
|---|---:|---:|---|
| `qwen3.5:122b-a10b` | 5.21 | 2/4 | Essentially tied with the older tag in the full refresh rerun |
| `qwen3.5:122b` | 5.22 | 2/4 | Refreshed baseline from the same run cycle |

## Additional checks for qwen3.5:122b-a10b

- Backend comparison artifact: [backend-comparison-qwen3.5-122b-a10b.json](C:/Development/OllamaBenchmarks/results/backend-comparison-qwen3.5-122b-a10b.json)
  In the refreshed rerun, `auto` reached 4.89 tok/s and `rocm` reached 5.34 tok/s.
  `vulkan` failed with a server-side `500`, so this tag still shows backend-specific variation even after the context fix.
- Sweep artifact: [optimization-sweep-qwen3.5-122b-a10b.json](C:/Development/OllamaBenchmarks/results/optimization-sweep-qwen3.5-122b-a10b.json)
  `baseline`, `threads_8`, `threads_16`, and `batch_1024` completed with `num_ctx=8192`.
  `force_gpu_99` failed with `memory layout cannot be allocated with num_gpu = 99`.

## Interpretation

This still looks like the same underlying model family, but not an identical runtime profile. After fixing the context handling, the base throughput of the two tags is effectively the same, but the `a10b` tag still shows backend-specific behavior: `auto` and `rocm` work, while `vulkan` remains unstable on this machine.
