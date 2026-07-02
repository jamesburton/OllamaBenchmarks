# Handoff

## Goal
Build and run a **Layer 4 benchmark** (agentic / tool-use / long-context) that measures what
Layers 1–3 cannot — the real strengths of Qwen3.5/Qwythos-class models. Also maintain a clean,
uncontaminated L3 record across the full model suite.

## Current State (2026-07-02, HEAD `000ee07`)

Working tree has uncommitted changes — see Recent Changes below.

### What is complete

| Step | Status | Commit |
|---|---|---|
| L3 contamination sweep (7 models) | ✓ done | `d44f27c`/`10e24c0` |
| Layer 4 T1/T2 tool-use harnesses | ✓ done | prior session |
| Layer 4 T3 agentic C# — 4-model cohort | ✓ done | `c5971e5` |
| Abliterated Qwythos — full build benchmarks (L2+L3) | ✓ done | `000ee07` |

### T3 agentic C# results

All 4 models scored 3/3 — the tasks are too easy to differentiate by pass/fail; step count is the only signal:

| Model | T3 | Avg steps |
|---|---|---|
| qwythos-9b-abliterated | 3/3 | 6.0 (leanest, no explicit build calls) |
| qwen3.5:9b | 3/3 | 6.3 |
| qwen3:8b | 3/3 | 7.0 |
| gemma4:e4b | 3/3 | 7.7 (most iterative) |

### Abliterated Qwythos build results (from `000ee07`)

Gen on T5500 RTX 3060, build/test on Framework (same setup as base Qwythos → apples-to-apples):

| Benchmark | Score |
|---|---|
| L2 raw | 51/158 (0.323) |
| L2 chat | 63/158 (0.399) |
| L3 nothink | 27/50 (0.545) |
| L3 think | 25/50 (0.509) |

**Key finding**: abliteration PRESERVES thinking — think barely hurts (27→25) vs base Qwythos
where think was devastating (25→15). All coding scores match or slightly beat base Qwythos.

### New: Ornith-1.0-9B-MTP (protoLabsAI) — partial results

User pulled and ran quality + coding benchmarks for two quants. Quality completed, coding did NOT:

| Variant | Quality | L2 raw | L3 |
|---|---|---|---|
| Q6_K | **11/11** (5/5 coding, 4/4 tool, 2/2 agentic) | INCOMPLETE | INCOMPLETE |
| Q8_0 | **10/11** (4/5 coding, 4/4 tool, 2/2 agentic) | INCOMPLETE | INCOMPLETE |

Files exist untracked: `results/quality-hf.co_protoLabsAI_Ornith-*.json` (complete, trustworthy).
`results/coding-hf.co_protoLabsAI_Ornith-*.json` — empty results, runs didn't finish.

### Uncommitted in working tree

- `PLATFORM_QUIRKS.md` — modified (unknown content, check before committing)
- `benchmark-models.json` — T3 backend_notes updates + abliterated L2/L3 entries
- `results/coding-layer3-results.json` — abliterated entries merged in
- 4 Ornith result files (untracked; coding ones are incomplete shells)

Repo is 2 commits ahead of `origin/main` (not yet pushed).

## Key Decisions Made

- **T3 tasks are too easy** — all 4 capable models scored 3/3. Future harder tasks should be added before T3 can differentiate models.
- **Abliteration doesn't change coding ability** — abliterated Qwythos matches base Qwythos on L2/L3; PRESERVES thinking (think hurts much less than base).
- **vibethinker-3b-cs:50k-q4 skipped for T3** — locally created model that was deleted; not in the Ollama registry, so cannot be repulled. Its T1/T2 was poor (1/6, 0/2) so T3 skip is low-loss.
- **Cross-machine split**: gen on GPU host (T5500/Strix) via `OLLAMA_HOST`, dotnet build/test on Framework. Throughput must run ON the GPU host.
- **NuGet contamination fixed**: `nuget.config` (`<clear/>` + nuget.org only) in all 4 templates. Working.
- **Thinking hurts .NET L3** remains cross-model (qwen3.5:9b 11→5, base Qwythos 25→15), except abliterated Qwythos (27→25) and gemma4:12b (31→32) as exceptions.

## What Worked

- Running T3 cohort on Framework locally (all models were already installed); no OLLAMA_HOST needed.
- Parallel workflow: user ran abliterated L2/L3 on T5500 concurrently while T3 ran on Framework.
- `benchmark_layer4_agentic_csharp.py` with `--model` (singular) — one invocation per model.

## What Didn't Work

- **Background task IDs expire** across sessions — use output file path directly to check progress.
- **T3 first attempt for abliterated** killed mid-run (session interruption); re-run succeeded.
- **OLLAMA_HOST without http:// scheme** — always use full `http://hostname:11434`.
- **glm-4.7-flash think@4096** — emits 2000+ reasoning tokens; some tasks hit budget. Still real.

## Recent Changes (this session)

- `results/layer4-agentic-richardyoung_qwythos-9b-abliterated_Q4_K_M.json` — T3 result (3/3)
- `results/layer4-agentic-gemma4_e4b.json` — T3 result (3/3)
- `results/layer4-agentic-qwen3_8b.json` — T3 result (3/3)
- `benchmark-models.json` — T3 backend_notes appended for abliterated, gemma4:e4b, qwen3:8b
- `results/coding-richardyoung_qwythos-9b-abliterated_Q4_K_M*.json` — L2+L3 results (user, `000ee07`)
- `results/coding-generated/richardyoung_qwythos-9b-abliterated_Q4_K_M/` — L3 generated code (user)

## Important Context

- **Machines**: Framework = local dev box (RTX 3060 eGPU, 12 GB, dotnet 10 build host). T5500 = remote
  Windows (`ssh t5500`, RTX 3060 12 GB CUDA, `OLLAMA_MODELS=E:\OllamaModels\.ollama\models`). Strix =
  AMD 128 GB unified (`ssh strix`, for models >12 GB). GPU work on T5500/Strix is user-coordinated.
- **Framework locally installed models** (relevant): gemma4:e4b (9.6GB), gemma4:12b (7.6GB),
  qwen3:8b (5.2GB), cogito:14b (9.0GB), minicpm-v:8b (5.5GB),
  richardyoung/qwythos-9b-abliterated:Q4_K_M (5.6GB), hf.co/prithivMLmods/VibeThinker-3B-GGUF:Q4_K_M (1.9GB)
- **Slug rule**: strip `:latest`, replace `[:/\\]` → `_`.
- **L3 output**: `results/coding-layer3-{slug}.json` per model + merged into `results/coding-{slug}.json`.
- **T5 long-context**: uses `scripts/benchmark_longcontext.py`. Results in `results/longcontext-{slug}.json`.
- **Strix Ollama**: 21 models installed, working. Needs `OLLAMA_HOST=http://strix:11434`.
- **T5500 disk**: tight on C:. Large models go to `E:` via `OLLAMA_MODELS`.
- **Ornith model**: `hf.co/protoLabsAI/Ornith-1.0-9B-MTP-GGUF` — a 9B model with MTP (multi-token
  prediction). Q6_K and Q8_0 quants benchmarked for quality; coding runs (L2/L3) incomplete.
  It's an MTP model so it may have quirks with Ollama's standard API — check MODEL_QUIRKS.md
  before re-running if the coding runs keep failing silently.

## Next Steps

1. **Commit the working-tree changes** before starting anything new:
   ```
   git add PLATFORM_QUIRKS.md benchmark-models.json results/coding-layer3-results.json
   git add results/quality-hf.co_protoLabsAI_Ornith-1.0-9B-MTP-GGUF_ornith-9b-mtp-kl-Q6_K.gguf.json
   git add results/quality-hf.co_protoLabsAI_Ornith-1.0-9B-MTP-GGUF_ornith-9b-mtp-kl-Q8_0.gguf.json
   # Do NOT add the coding-Ornith files (empty/incomplete)
   git commit -m "feat: abliterated Qwythos backend_notes + Ornith quality results"
   ```

2. **T5 on Strix** for 256K+/1M context and KV-quant sweep (coordinate GPU first):
   ```powershell
   $env:OLLAMA_HOST = "http://strix:11434"
   python scripts/benchmark_longcontext.py --models hf.co/empero-ai/Qwythos-9B-Claude-Mythos-5-1M-GGUF:Q4_K_M
   python scripts/benchmark_longcontext.py --models richardyoung/qwythos-9b-abliterated:Q4_K_M
   python scripts/benchmark_longcontext.py --models qwen3.5:9b
   # For KV-quant variants, restart Strix Ollama server with OLLAMA_KV_CACHE_TYPE=q8_0 / q4_0
   ```
   Models must be installed on Strix. Verify with `ssh strix "ollama list"` first.

3. **Ornith L2/L3 coding benchmarks** (once T5 is done, or in parallel on Framework):
   - Investigate why the coding runs produced empty results — check if MTP causes Ollama API issues
   - If Ornith is still installed: `python scripts/benchmark_coding_layer2.py --models hf.co/protoLabsAI/Ornith-1.0-9B-MTP-GGUF:ornith-9b-mtp-kl-Q6_K.gguf`
   - Add Ornith to `benchmark_suite` and write `backend_notes` entry once coding results are in

4. **Layer 4 summary chart** — `scripts/layer4_summary_chart.py` (create or extend `strix_summary_chart.py`):
   - Columns: model | T1 | T2 | T3 | T5 (max_ctx, KV_GB/10k, decode_tok/s)
   - Include qwen3.5:9b, Qwythos base, abliterated Qwythos, gemma4:e4b, qwen3:8b

5. **Final push**: `git push` once all results and the summary chart are committed.
