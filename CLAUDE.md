# Claude / agent guide for OllamaBenchmarks

Notes for AI coding agents (Claude Code, Codex CLI, etc.) working in this repo. Read [README.md](README.md) first for the harness overview and entry-point scripts; this file documents the conventions an agent needs to follow to avoid producing misleading results.

## Mandatory reading before reporting any benchmark score

- [MODEL_QUIRKS.md](MODEL_QUIRKS.md) — per-model nuances. If a score looks anomalously low, check this file before blaming the model. The most common false negatives are Layer 2 raw-mode on chat-tuned models (gemma4 family, gpt-oss, LFM2, glm-4.7-flash-reap) scoring 1–10% when the model is actually capable, and `think:true` runs on models that reject the flag returning 0/50 in under 5 seconds.
- [PLATFORM_QUIRKS.md](PLATFORM_QUIRKS.md) — host-level diagnostics. If a model fails to load on Strix, check `OLLAMA_GPU_MEMORY` and the BIOS UMA setting before declaring the model incompatible.

Both files end with `*Extend and refine these notes as insights are proven*`. When you discover a new gotcha during a session, append it there with the date and the verified fix.

## Benchmark output conventions

- Slug rule: strip `:latest` then replace `[:/\\]` with `_`. The same slug is used for `quality-{slug}.json`, `throughput-resource-{slug}.json`, and `coding-{slug}.json`. The slug convention is documented at the top of `benchmark-models.json`.
- Layer 3 think variants land in `coding-{slug}-think.json` when the runner is invoked with `CODING_BENCH_THINK=true`. The default still writes `coding-{slug}.json` so the no-think baseline is preserved.
- Layer 2 chat-mode results land in `coding-{slug}-chat.json` (from `scripts/benchmark_coding_layer2_chat.py`). The raw-mode baseline at `coding-{slug}.json` is preserved.
- Per-task generated C# code lands in `results/coding-generated/<slug>/<task>.cs`. The L3 directory is overwritten on every run for that slug — make a copy before the next run if you need to diff outputs.

## Cross-machine runs & build-timeout knobs (env)

- **`OLLAMA_HOST`** (e.g. `http://t5500:11434`): generation targets a remote Ollama while this box does the dotnet build/test. Honored by `benchmark_quality.py`, `benchmark_coding_layer2.py` (raw), `benchmark_coding_layer2_chat.py`, and `coding_tasks/task_runner.py` (L3). The standard split is **gen on the GPU host (T5500/Strix), build/test on Framework**; the recorded `ollama_host` in the output confirms where gen ran.
- Throughput (`benchmark_throughput_resource.ps1`) measures *local* GPU/CPU, so it must run **on the GPU host**, not cross-machine. (A quick tok/s can be read from the API `eval_count/eval_duration` instead.)
- **Build-timeout overrides** for loaded/disk-starved hosts (cold `dotnet` builds in fresh temp dirs can exceed defaults and mis-score valid runs as timeouts): `L2_RUN_TIMEOUT_S` (default 30; L2 raw + chat), `L3_BUILD_TIMEOUT_S` / `L3_TEST_TIMEOUT_S` (default 60). Set all to ~150 when the host is busy. Symptom of too-tight a budget: a flood of uniform "timed out" fails — re-run with the knobs before trusting the score.
- **Generation deadline (L2 raw):** `L2_GEN_TIMEOUT_S` (default 150) bounds each completion request in `benchmark_coding_layer2.py`. The raw harness once hung mid-run when a generation request orphaned and blew past the urllib *socket* timeout while Ollama stayed healthy — so the call now runs in a module-level `ThreadPoolExecutor` with a hard wall-clock backstop that abandons the worker and fails just that task. The raw harness also **checkpoints `coding-{slug}.json` after every task** (was only at the very end), so a stall loses one task, not the whole run, and progress is observable mid-run.
- **L2/L3 share `coding-{slug}.json` and both merge:** `benchmark_coding_layer3.py` now reads-then-updates the per-model file (preserving `layer2_*`) instead of overwriting — mirroring the L2 harness, which preserves `layer3_*`. Run order no longer matters; neither layer clobbers the other.

## Layer 4 (agentic / tool-use / long-context) — planned

Layers 1–3 only measure single-shot code gen. For agentic/tool/long-context model classes
(e.g. Qwen3.5/Qwythos, qwen3.6, gemma4) see **`docs/agentic-tool-longcontext-benchmark-plan.md`**
— the design for tool-use correctness, agentic C# loops (read/write/build/test until green),
needle-in-haystack long-context, and KV-VRAM-vs-context scaling. Not yet implemented.

## `benchmark-models.json` discipline

- `backend_notes[model]` is the single source of truth for per-model commentary. Add a line for every Strix-tested model with throughput, quality, L3, and L2 numbers. When a number is an artifact (e.g. raw-mode L2 for a chat-tuned model), mark it `ARTIFACT` and point at the chat-mode re-run.
- `methodology_notes` at the top of the file explains repo-wide conventions (raw vs chat L2, think variants). Update it when the methodology changes.
- The `benchmark_suite`, `local_installed`, `effective_models`, and `missing_from_local` arrays are coverage indicators — keep them aligned with `ollama list`.

## Adding a new model

1. `ollama pull <model>` (or HF GGUF via `hf.co/<owner>/<repo>:<tag>`).
2. Quality: `python scripts/benchmark_quality.py --models <model> --output results/quality-<slug>.json`
3. Throughput: `./scripts/benchmark_throughput_resource.ps1 -Models <model> -OutputPath results/throughput-resource-<slug>.json`
4. Layer 3: `python scripts/benchmark_coding_layer3.py --models <model> --output results/coding-layer3-<slug>.json`
5. Layer 2 (raw): `python scripts/benchmark_coding_layer2.py --models <model> --dataset-path scripts/coding_tasks/datasets/data/humaneval-cs-reworded.json`
6. If chat-tuned or thinking-default, **also** Layer 2 (chat): `python scripts/benchmark_coding_layer2_chat.py --models <model> --dataset-path scripts/coding_tasks/datasets/data/humaneval-cs-reworded.json`
7. If thinking-capable, run paired Layer 3 think variant: `set CODING_BENCH_THINK=true && python scripts/benchmark_coding_layer3.py ...` (or PowerShell `$env:CODING_BENCH_THINK='true'; python ...`)
8. Append a `backend_notes` entry in `benchmark-models.json` summarising throughput, RAM/VRAM, quality, L3 (no-think and think), L2 (raw and chat). Cross-reference any quirk against MODEL_QUIRKS / PLATFORM_QUIRKS.
9. Commit with `feat: <model> Strix benchmarks — <one-line summary>`.

## Running multi-model chains

- `scripts/run_think_variants_l3.py` — sequential think:true L3 across a priority list.
- `scripts/run_l2_chat_for_weak_raw.py` — sequential chat-mode L2 re-run for chat-tuned models.
- `scripts/fill_l2_gaps.py` — sequential raw-mode L2 for installed models that lack L2 data.
- `scripts/run_overnight_strix.ps1` — wrapper that chains the above. Logs to `results/overnight-logs/`.

All four are sequential because the Strix iGPU can only host one model at a time. Don't parallelize.

## Investigation tools

- `scripts/probe_default_think.py` — confirm which models honor `think:false`.
- `scripts/check_think_support.py` — quick probe for `think:true` support per model.
- `scripts/check_benchmark_coverage.py` — print per-model coverage matrix (quality / throughput / L2 / L3).
- `scripts/think_vs_nothink_table.py` — print L3 think vs no-think comparison.
- `scripts/strix_summary_chart.py` — print the master Strix chart (tok/s desc, then L3 desc, then L2 desc, then quality).
- `scripts/inspect_l2_failures.py <slug>` — categorize L2 failure modes for a given model.
- `scripts/replay_l2_task.py <model> <task>` — replay a single L2 problem to see the raw model response.

## When in doubt

Read the matching memory file in `~/.claude/projects/C--Development-OllamaBenchmarks/memory/` — there are saved notes on the Strix unified memory unlock, the Ollama UMA bug, the Ollama think-flag quirks, the L2 raw-vs-chat issue, and the Layer 2/3 template cache trap. Cross-reference both MODEL_QUIRKS.md and the memory file before reporting a result that contradicts them.
