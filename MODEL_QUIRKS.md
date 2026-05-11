# Model Quirks

Per-model gotchas observed while benchmarking Ollama models, with the underlying reason and a recommended workaround. Sorted roughly by impact on benchmark interpretation. Keep this list current — a misleading score in `benchmark-models.json` is worse than missing data.

## The `think` flag matrix

Ollama exposes a `think` parameter on `/api/chat` and `/api/generate`, but model families react inconsistently.

| Behaviour | Models | What to do |
| --- | --- | --- |
| Honors `think:true`/`think:false` cleanly | gemma4 family (e2b/26b/31b), qwen3.6, qwen3.6:27b, qwen3:14b, qwen3.5 (latest/4b/9b), glm-4.7-flash, nemotron-3-nano family, nemotron-nano-9b-v2-toolfix | Run paired variants. `CODING_BENCH_THINK=true` produces `coding-{slug}-think.json`; default produces `coding-{slug}.json`. |
| Ignores `think:false` (still thinks) | gpt-oss:20b, gpt-oss:120b, lfm2.5-thinking (both sizes), glm-4.7-flash-reap-toolfix | The "no-think" baseline is effectively a think run. A `-think` variant adds little. For gpt-oss, set `reasoning_effort: low\|medium\|high` instead of toggling `think`. |
| Rejects `think:true` with `does not support thinking` | granite4:7b-a1b-h, granite4:32b-a9b-h, qwen3-coder-next | Run only the no-think baseline. Delete any `-think` checkpoint that produces 0/50 in <5 s — it's a 400 error not a model failure. |

Probe script: `scripts/probe_default_think.py`. Re-run after Ollama upgrades.

## Layer 2 raw-mode vs chat-mode

The default Layer 2 runner (`scripts/benchmark_coding_layer2.py`) calls `/api/generate` with `raw: true`. That bypasses every chat template, RENDERER, and PARSER and feeds the C# function signature to the model as a raw completion prefix.

- Completion-friendly models (granite4, OmniCoder, qwen-coder, gemma3, shenwen-coderV2, qwen3 base) score 25–50% on raw L2.
- Chat-tuned and thinking-default models (gemma4 family, gpt-oss, glm-4.7-flash-reap, lfm2.5-thinking, LFM2 family, phi4-mini, mistral-small in chat mode) score 1–10% because they cannot continue from a raw prefix.

The raw score is a real signal of pure FIM ability and should be kept. But it is **not** a fair "coding ability" headline for chat-tuned models. For those, also run the chat-mode variant:

```
python scripts/benchmark_coding_layer2_chat.py \
    --models <model> \
    --dataset-path scripts/coding_tasks/datasets/data/humaneval-cs-reworded.json
```

That uses `/api/chat` (template applied), `num_predict=4096`, `num_ctx=8192`, and `reasoning_effort: low` for the gpt-oss family. It writes to `coding-{slug}-chat.json` so the raw baseline at `coding-{slug}.json` is preserved.

The `scripts/run_l2_chat_for_weak_raw.py` runner queues a batch of chat-tuned/thinking models.

## gemma4 family: `TEMPLATE {{ .Prompt }}` + RENDERER/PARSER

gemma4 modelfiles use Ollama 0.20+ `RENDERER gemma4` / `PARSER gemma4` and have `TEMPLATE {{ .Prompt }}`. With `raw: true` the renderer is bypassed and the model receives an unwrapped completion prompt it cannot follow. Always use `/api/chat` (raw=false) for gemma4.

gemma3 by contrast has a real TEMPLATE block (`<start_of_turn>user…<end_of_turn>`) which Ollama applies even on `/api/generate` when `raw: false` (the default). That's why gemma3:4b scored 34.8% on L2 raw but gemma4 only 3.8%.

## glm-4.7-flash and glm-4.7-flash-reap-toolfix

- `glm-4.7-flash` (base): honors `think:false` per the probe, but on long coding prompts still produces reasoning-style content that the extractor strips to empty. L3 with `think:false` was 0/50; `think:true` was 34/50 (0.673). Force `CODING_BENCH_THINK=true` for any meaningful coding score.
- `glm-4.7-flash-reap-toolfix` (Cerebras REAP-pruned + tool-template fix): also ignores `think:false`. L2 raw 2/158 is an artifact — extraction fails because reasoning is mixed into `content`. Chat-mode L2 re-run is the only meaningful score.

## Carnice-V2 27b and GLM-4.5-Air-REAP

Both wrap completions in `<think>...</think>` that fills the small (256 tok) quick-quality budget before code arrives, so they score 0–3/11 on the quick suite even though their coding L3/L2 are healthy. Use 1024+ token budgets or read L3/L2 numbers when judging these models.

## qwen3-coder-next

Strong on Layer 3 (.NET): 41/50 (0.818). But Ollama rejects `think:true` for this model — Qwen3-coder variants are tuned for code generation with no thinking branch. The L3 number above is the only one to cite; do not look for a `-think` variant.

## qwen3.6:27b dense vs qwen3.6 MoE on Strix

- qwen3.6 (35B MoE, ~3B active): 44.61 tok/s, L3 40/50, L2 77/158.
- qwen3.6:27b (dense): 10.01 tok/s, L3 40/50, L2 68/158.

Qwen claims the dense 27B beats the MoE 397B on SWE-bench, but on this hardware the MoE 35B wins by every measured axis except pure-dense determinism. Pick MoE for Strix iGPU work; the dense 27B is interesting as a comparison anchor only.

## qwen35moe arch and pre-0.20 Ollama

qwen35moe loads fine in Ollama 0.20.0-rc1+. On Ollama 0.20.7 and earlier (e.g. T5500), 35B variants panic at `ggml.go:276 (index out of range [0] with length 0)`. Small 4b/9b variants of the same arch load fine on those older versions. The fix is to upgrade Ollama on the affected host; do not attempt llama-server-based workarounds for this arch.

## Sharded GGUF and Ollama

Ollama does not support sharded GGUFs (GitHub issue #5245). When pulling REAP-pruned or large quants from HF, look for a single-file quant variant. GLM-4.5-Air-REAP Q4_K_M is sharded → use IQ4_XS instead (47 GB single file).

## Step-3.5-Flash arch

Ollama 0.20.2 does not yet support the Step-3.5-Flash architecture; the model downloads but returns 500 on load. Use llama-server (post-PR llama.cpp) until Ollama merges the support. GitHub: ollama/ollama#14043.

## Stale Layer 2/3 template cache

If a Layer 2 or Layer 3 run scores 0/N or near-zero with **all** failures at the build stage, suspect a stale `scripts/coding_tasks/templates/.cache/` directory before suspecting the model. The cached `obj/project.assets.json` pins exact transitive package versions; if those versions are no longer in the local NuGet cache, `dotnet build --no-restore` fails with NETSDK1064 for every task. Delete the cache and rerun:

```
rm -rf scripts/coding_tasks/templates/.cache
```

The runner's `setup_template_cache()` will rebuild on next invocation.

## Quick quality suite token budget

The `benchmark_quality.py` quick quality suite uses a 256-token cap for coding answers. Any model that wraps reasoning in `<think>…</think>` (Carnice-V2, GLM-4.5-Air-REAP, several distilled variants) will exhaust the budget before producing code and score 0/5 on coding. That score does not reflect the model's actual coding ability — read L3/L2 numbers for those.

---

*Extend and refine these notes as insights are proven*
