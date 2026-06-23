# Handoff

## Goal
Build a **Layer 4 benchmark** (agentic / tool-use / long-context) that measures what
Layers 1–3 cannot: agentic code-generation-with-tools and long-context capability — the
real strengths of Qwen3.5/Qwythos-class models. Layers 1–3 only test single-shot code
gen, where the `Qwythos-9B` fine-tune actually scored **below** its base `qwen3.5:9b`
(L2-chat 62 vs 67). The user wants to use these models in **agentic C# workflows with
tools and long context**, so we need tests that find what they *should* achieve, push to
their limits, and compare across models. Full design already written — implement it.

Secondary: finish the unfinished `qwen3.5:9b` base coding figures (fresh L2-raw + L3).

## Current State
- **Design doc DONE, implementation NOT started:** `docs/agentic-tool-longcontext-benchmark-plan.md` is the complete, actionable spec (T1 tool-use correctness, T2 agentic loop, T3 agentic C# read/write/build/test loop, T4 long-context needle + long-C#, T5 KV-VRAM-vs-context scaling). Build the harnesses it lists.
- **All prior work committed & pushed to `main`** (HEAD `ce583ae`). Working tree clean. The `feature/vibethinker-csharp-finetune` branch was merged and deleted.
- **qwen3.5:9b carry-over is INCOMPLETE:**
  - L2-chat: **67/158 (0.424)** — done, fresh, committed (`results/coding-qwen3.5_9b-chat.json`).
  - L2-raw: today's run **HUNG at task ~121/158** and never wrote; `results/coding-qwen3.5_9b.json` on disk is a **stale 2026-04-17** prior (61/158). Needs a fresh re-run.
  - L3 (no-think + think): **not run** (deferred so the GPU was freed for other work).
- `qwen3.5:9b` is pulled on T5500 (6.6 GB Q4_K_M). GPU is idle/free.

## Key Decisions Made
- **Q4_K_M for Qwythos** (not f16) so it matches the base model's quant → apples-to-apples; also dodged an Ollama f16-validation failure that was actually disk-space, not a bad GGUF.
- **Cross-machine split is the standard**: generation on the GPU host (T5500/Strix) via `OLLAMA_HOST`, dotnet build/test on Framework. Throughput must be measured *on* the GPU host (it reads local VRAM).
- **"Thinking hurts .NET L3"** is a confirmed cross-model finding (Qwythos 6→4, qwen3.6 40→35) — Layer 3 uses the **no-think baseline**; think variants are for the record only.
- **The Qwythos fine-tune did not help coding** — its base beats it. Its value is agentic/tool/long-context, which is *why* Layer 4 exists. Don't relitigate "is Qwythos a good coder" — it isn't, relative to its base.
- Committing directly to `main` is fine now (personal tracking repo; feature branch already merged).

## What Worked
- Robust remote launches on T5500: **Task Scheduler (`schtasks /ru james /it`) + redirect output to a remote FILE** survives ssh drops (held-ssh + `-u` dies on broken-pipe; see PLATFORM_QUIRKS). File-polling watchers via short ssh calls, not held connections.
- The configurable build-timeout knobs (`L2_RUN_TIMEOUT_S`, `L3_BUILD_TIMEOUT_S`, `L3_TEST_TIMEOUT_S` = 150) fixed false "timed out" floods on a loaded host.
- Reading per-task results by **grepping after the phase marker** (`sed -n '/[2\/4] L2-raw/,$p'`) — both L2-chat and L2-raw print `[N/158]`, so a naive `tail` mixes phases.

## What Didn't Work
- **L2-raw harness hangs on long unattended runs**: a generation request orphaned and blew past its urllib timeout while Ollama itself stayed healthy (confirmed via a fresh test gen). Investigate the read-timeout robustness in `scripts/benchmark_coding_layer2.py` before trusting long raw runs. The chained 4-phase suite (`scripts/lora/run_qwen35_9b_suite.ps1`) got stuck here.
- Estimating ETA by eyeball — real pace was ~78 s/task (L2 phase = ~3.4 h), ~4× my guesses. Measure from result JSON `*_run_started_at/finished_at`.
- `git merge -F -` (stdin) — not supported; use a message file.

## Recent Changes (this session)
- `docs/agentic-tool-longcontext-benchmark-plan.md` — NEW, the Layer 4 design/spec.
- `benchmark-models.json` — added `Qwythos-9B` (full results) + `vibethinker-3b-cs:50k-q4` (iter-2) to `benchmark_suite`+`backend_notes`; updated `qwen3.5:9b` note with L2 coding + base-vs-finetune finding.
- `scripts/benchmark_quality.py`, `scripts/benchmark_coding_layer2.py` — added `OLLAMA_HOST` support (were hardcoded 127.0.0.1).
- `scripts/benchmark_coding_layer2_chat.py`, `scripts/coding_tasks/task_runner.py` — env-configurable dotnet timeouts (+ `stdin=DEVNULL`).
- `MODEL_QUIRKS.md`, `CLAUDE.md` — Qwen-family think notes; OLLAMA_HOST + timeout-knob docs; Layer 4 pointer.
- `results/` — Qwythos quality/L2 raw+chat/L3 no-think+think; qwen3.5:9b L2-chat.
- `.gitignore` — excludes `scripts/lora/output/` (multi-GB adapters).

## Important Context
- **Machines**: Framework = local dev box (hostname `Framework`, dotnet 10 build host, eGPU dead). T5500 = remote Windows (RTX 3060 12 GB CUDA), `ssh t5500` passwordless over Tailscale, cmd.exe shell, `OLLAMA_MODELS=E:\OllamaModels\.ollama\models`, Ollama server already network-bound (reachable as `http://t5500:11434` from Framework). Strix = AMD 128 GB unified (for big models). GPU work on shared machines is coordinated via the user.
- T5500 disk is tight — keep large artifacts off C:; E: has more room. Watch for it filling.
- Slug rule: strip `:latest`, replace `[:/\\]` → `_`. Qwythos slug = `hf.co_empero-ai_Qwythos-9B-Claude-Mythos-5-1M-GGUF_Q4_K_M`.
- Open questions for Layer 4 (resolve first): Qwen3.5 attention details (GQA/sliding window?), whether Ollama supports KV-cache quant + flash-attn for `qwen3.5` arch (`OLLAMA_KV_CACHE_TYPE`), and whether Ollama returns structured `message.tool_calls` for these models (dictates T1/T2 parsing).
- Comparison cohort for Layer 4: Qwythos-9B (1M), qwen3.5:9b (base), qwen3.6 + qwen3.6:27b, gemma4 family, GLM-4.x, qwen3-coder-next.

## Next Steps
1. **Finish qwen3.5:9b base figures** (quick win, GPU free): from Framework, `OLLAMA_HOST=http://t5500:11434`, `L2_RUN_TIMEOUT_S=150 L3_BUILD_TIMEOUT_S=150 L3_TEST_TIMEOUT_S=150` — re-run L2-raw (`benchmark_coding_layer2.py`), then L3 no-think and L3 think (`benchmark_coding_layer3.py`, `--output results/coding-layer3-qwen3.5_9b[-think].json`). Watch the L2-raw hang; if it recurs, fix the timeout in the raw harness first. Then complete the base-vs-Qwythos table and update `qwen3.5:9b` backend_notes; commit.
2. **Resolve the Layer 4 open questions** (above) — read the Qwen3.5 model card/config, probe Ollama tool-call output format and KV-cache-quant support. Cheap, unblocks the harness design.
3. **Build T5 first (long-context VRAM/throughput sweep)** — it directly answers the user's headline question ("long context without massive VRAM?") and is the simplest harness (num_ctx sweep + nvidia-smi on the GPU host). `scripts/benchmark_longcontext.py`.
4. **Build T1/T2 (tool-use + agentic loop)** via a reusable `scripts/coding_tasks/tool_loop.py` driver (extends the quality-suite `TOOL_TASKS`/`PLAN_AGENT_TASKS`). Cross-machine OK.
5. **Build T3 (agentic C#)** — file-op + build/test tools on top of `coding_tasks/task_runner.py`'s .NET sandbox; loop until tests green.
6. Run the Layer 4 suite across the comparison cohort; add a cross-model chart (cf. `scripts/strix_summary_chart.py`).
