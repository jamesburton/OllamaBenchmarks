# Handoff

## Goal
Build and run a **Layer 4 benchmark** (agentic / tool-use / long-context) that measures what
Layers 1–3 cannot — the real strengths of Qwen3.5/Qwythos-class models. Also maintain a clean,
uncontaminated L3 record across the full model suite.

## Current State (2026-06-25, HEAD `fc7e831`)

Working tree is **clean**. All contaminated L3 results have been corrected.

### Contamination sweep — COMPLETE
All 7 originally scheduled contaminated models (+ Qwythos and qwen3.5:9b from prior session) have been
re-run with the NuGet-fix in place. Key corrected scores:

| Model | Old (contaminated) | Corrected |
|---|---|---|
| qwen3.5:9b | 3/50 | **11/50** (0.218) nothink |
| Qwythos-9B | 6/50 | **25/50** (0.500) nothink |
| glm-4.7-flash | 0/50 | **35/50** (0.691) nothink ← biggest surprise |
| gemma4:12b | 5/50 | **31/50** (0.600) nothink — verdict reversed |
| gemma4:12b think | 6/50 | **32/50** (0.618) |
| cogito:14b | — | **27/50** (0.527) |
| qwen3:8b | ~0/50 | **20/50** (0.400) |
| minicpm-v:8b | ~0/50 | **6/50** (0.120) |
| VibeThinker-3B-GGUF | ~0/50 | **0/50** (genuine gap confirmed) |
| trinity-mini | ~0/50 | **0/50** (genuine gap confirmed) |
| vibethinker-3b-cs:50k-q4 | ~0/50 | **1/50** |

**Key insight from the sweep:** glm-4.7-flash's prior "reasoning-extraction failure" explanation was
WRONG — its 0/50 was pure NuGet contamination; nothink mode works fine (35/50). gemma4:12b's "POOR
fit for .NET" verdict was also wrong (31/50 corrected). Both MODEL_QUIRKS.md and backend_notes updated.

### Layer 4 harnesses — BUILT, partial cohort run
- `scripts/benchmark_longcontext.py` (T5) — DONE. Run: qwen3.5:9b on T5500 (128K fits 9.93GB), abliterated Qwythos partial (Framework 3060 only got to 64K fit).
- `scripts/benchmark_layer4_tools.py` (T1/T2) — DONE. Run for initial cohort: gemma4:e4b 6/6 T1 + 2/2 T2, abliterated got 0/6 T1 (OLLAMA_HOST scheme bug, now fixed).
- `scripts/benchmark_layer4_agentic_csharp.py` (T3) — DONE. Built; **cohort NOT yet run**.
- `scripts/coding_tasks/tool_loop.py` — reusable tool-loop driver used by T1/T2/T3.

### Abliterated Qwythos — no-build benchmarks only
`richardyoung/qwythos-9b-abliterated:Q4_K_M` is in `benchmark_suite`. No-build results committed:
quality 8/11, T1 6/6, T2 2/2, T5 max_fit=64K (Framework 3060 limit). **Build benchmarks (L2
raw+chat, L3 nothink+think, T3) still pending** — need T5500 for clean 128K fit + L3 builds.

## Key Decisions Made
- **NuGet contamination fix**: added `nuget.config` (`<clear/>` + nuget.org only) to all 4 templates
  (`test_project`, `blazor_project`, `layer2_project`, `agentic_csharp`). Confirmed working.
- **Contamination tell**: test_project built 0 while Blazor built some = contaminated. Check
  `build_success` per task before trusting any L3 number.
- **Cross-machine split**: gen on GPU host (T5500/Strix) via `OLLAMA_HOST`, dotnet build/test on
  Framework. Throughput must run ON the GPU host.
- **glm-4.7-flash quirk CORRECTED**: prior note said "nothink emits reasoning that extractor strips"
  — this was the contamination masking model quality. Both nothink (35) and think (34) work fine.
- **gemma4:12b L3 CORRECTED**: was labelled "POOR fit for .NET" — actually 31/50 solid coder.
- **Thinking hurts .NET L3** remains a cross-model finding (qwen3.5 11→5, Qwythos 25→15), but
  gemma4:12b is the exception where think very slightly helps (31→32).
- **Abliterated Qwythos T5**: Framework's 3060 only fits 64K (7.1GB model leaves <5GB for KV).
  T5500 should fit 128K cleanly. The 1M context story needs Strix.
- **L3 harness does NOT checkpoint per-task** — only at the end of all 50. Long runs produce no
  output until complete. This is expected, not a hang.

## What Worked
- **rerun_contaminated_l3.sh** + inline pull scripts for batch contamination re-runs. Works well
  for sequential model queuing. `MODE=all` does pull → run → delete per model.
- **Parallel GPU split**: Framework 3060 for build-only benchmarks (T1/T2, T5 at ≤12GB), T5500 or
  Strix for generation. No build contention between them.
- **Task Scheduler on T5500** for long unattended remote runs; Strix via inline bash+SSH for pulls.
- Framework Ollama needs occasional restart: `Stop-Process "ollama app"`, then relaunch the app,
  wait 10s for server to bind port 11434.

## What Didn't Work
- **Background task IDs expire** across sessions — `TaskOutput` returns "No task found" after a
  context compaction. Use output file path directly (`Read` on the `.output` file) to check progress.
- **OLLAMA_HOST without http:// scheme** — causes `URLError: unknown url type`. Always use full
  `http://hostname:11434`.
- **glm-4.7-flash think@4096** — emits 2000+ reasoning tokens; some tasks hit token budget and
  return empty code. This is still real but the 32/50 think score shows it's survivable.

## Recent Changes (this session)
- `results/coding-layer3-{7 models}.json` — corrected contaminated L3 scores
- `results/coding-generated/{7 models}/` — regenerated code from corrected runs
- `benchmark-models.json` — backend_notes updated for all corrected models; gemma4:12b verdict
  reversed; glm-4.7-flash quirk corrected; cogito:14b/qwen3:8b/minicpm-v:8b/trinity-mini entries added
- `MODEL_QUIRKS.md` — glm-4.7-flash section corrected (was "nothink extraction fails" → now accurate)
- `scripts/rerun_contaminated_l3.sh` — batch re-run script for contaminated models

## Important Context
- **Machines**: Framework = local dev box (RTX 3060 eGPU, 12 GB, dotnet 10 build host). T5500 = remote
  Windows (`ssh t5500`, RTX 3060 12 GB CUDA, `OLLAMA_MODELS=E:\OllamaModels\.ollama\models`). Strix =
  AMD 128 GB unified (`ssh strix`, for models >12 GB). GPU work on T5500/Strix is user-coordinated.
- **Framework GPU**: Ollama uses CUDA RTX 3060 by default. If Ollama server is down, kill the app
  and restart: `Stop-Process "ollama app"; Start-Process "...\ollama app.exe"; sleep 10`.
- **T5500 disk**: tight on C:. E: has more room. Pull large models to E: via `OLLAMA_MODELS`.
- **Slug rule**: strip `:latest`, replace `[:/\\]` → `_`.
- **L3 output locations**: `results/coding-layer3-{slug}.json` (new-format file per model) +
  merged into `results/coding-{slug}.json`. The `-think` suffix applies when `CODING_BENCH_THINK=true`.
- **Strix Ollama**: tested and working (21 models installed). glm-4.7-flash was pulled fresh this
  session and can be deleted post-run if disk is tight.
- **Framework installed models** (relevant): gemma4:12b (7GB), minicpm-v:8b (5.1GB), qwen3:8b
  (4.9GB), VibeThinker-3B-GGUF (1.8GB), richardyoung/qwythos-9b-abliterated (5.2GB).

## Next Steps

1. **Run T3 cohort (agentic C# loop)** on Framework using `scripts/benchmark_layer4_agentic_csharp.py`:
   - Models to run: Qwythos-9B, gemma4:e4b, qwen3:4b (or qwen3:8b), vibethinker-3b-cs:50k-q4
   - Template is `agentic_csharp` (nuget.org-only, xunit only — no MassTransit dependency)
   - Gen via `OLLAMA_HOST` pointing to whichever GPU host has the model; builds on Framework
   - Output: `results/layer4-agentic-{slug}.json`

2. **Abliterated Qwythos build benchmarks** on T5500 (coordinate GPU first):
   - L2 raw: `python scripts/benchmark_coding_layer2.py --models richardyoung/qwythos-9b-abliterated:Q4_K_M`
   - L2 chat: `python scripts/benchmark_coding_layer2_chat.py --models richardyoung/qwythos-9b-abliterated:Q4_K_M`
   - L3 nothink: `python scripts/benchmark_coding_layer3.py --models richardyoung/qwythos-9b-abliterated:Q4_K_M`
   - L3 think: `CODING_BENCH_THINK=true python scripts/benchmark_coding_layer3.py ...`
   - T3: `python scripts/benchmark_layer4_agentic_csharp.py --models richardyoung/qwythos-9b-abliterated:Q4_K_M`
   - T5 on T5500 for clean 128K curve (Framework only got 64K due to less headroom)

3. **T5 on Strix** for 256K+/1M context and KV-quant sweep:
   - `OLLAMA_KV_CACHE_TYPE=q8_0` and `q4_0` variants (needs Ollama server restart on Strix → coordinate)
   - Models: Qwythos-9B (1M native), qwen3.5:9b, glm-4.7-flash (19GB, already on Strix)

4. **Cross-model Layer 4 summary chart** — once T3 cohort + abliterated data are in:
   - Extend `scripts/strix_summary_chart.py` or create `scripts/layer4_summary_chart.py`
   - Columns: model, T1, T2, T3, T5 (max_fit_ctx, kv_gb_per_10k, decode_tok_s)

5. **Final commit/push** of all accumulated results.
