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
- **qwen3.5:9b carry-over is now COMPLETE (2026-06-24, cross-machine: gen on T5500, build on Framework):**
  - L2-chat: **67/158 (0.424)** — committed (`results/coding-qwen3.5_9b-chat.json`).
  - L2-raw: **42/158 (0.266)** — fresh same-day re-run (prior 61/158 was a 2026-04-17 different-setup figure, not comparable; raw mode is noisy for this instruct model → L2-chat is the trusted number).
  - L3 no-think: **11/50 (0.218)**; L3 think: **5/50 (0.109)** — CORRECTED 2026-06-24 (earlier 3/50 & 1/50 were the test_project NuGet-restore contamination, now fixed). Think still hurts L3 (11→5).
  - base-vs-Qwythos (same cross-machine setup): base wins L2-chat (67 vs 62); fine-tune wins L2-raw (42 vs 50) and DECISIVELY wins L3 — no-think **25 vs 11**, think **15 vs 5** (~2.3×). This REVERSES the prior "base beats finetune" read (which rested on contaminated L3 + L2-chat alone): on framework-level .NET the Qwythos fine-tune is the stronger coder.
  - Harness fixes this session: L2-raw now has a wall-clock gen backstop (`L2_GEN_TIMEOUT_S`, default 150) + per-task incremental checkpoint; L3 now merges into `coding-{slug}.json` instead of clobbering `layer2_*`.
- `qwen3.5:9b` is pulled on T5500 (6.6 GB Q4_K_M). GPU coordination: no lock file exists (T5500 CLAUDE.md = "user-mediated until we build a lock file"); monitor `nvidia-smi` idle + user go-ahead.

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
- **L2-raw harness hangs on long unattended runs** — FIXED (2026-06-24). A generation request orphaned and blew past its urllib *socket* timeout while Ollama stayed healthy. Fix: each gen call now runs in a module-level `ThreadPoolExecutor` with a hard wall-clock backstop (`L2_GEN_TIMEOUT_S`, default 150) that abandons the worker and fails just that task; plus per-task incremental checkpointing so a stall can't lose the whole run. The 2026-06-24 full re-run completed all 158 with 0 timeouts. (Still avoid the chained suite `scripts/lora/run_qwen35_9b_suite.ps1`; run L2/L3 as separate invocations.)
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
1. ~~Finish qwen3.5:9b base figures~~ **DONE (2026-06-24)** — L2-raw 42/158, L3 no-think 11/50, L3 think 5/50 (corrected after the test_project NuGet contamination fix); base-vs-Qwythos table + backend_notes updated; L2-raw hang fixed. See Current State.
2. ~~Resolve the Layer 4 open questions~~ **DONE (2026-06-24)** — all four answered + a bonus measurement-primitive probe, recorded in `docs/agentic-tool-longcontext-benchmark-plan.md` ("Open questions — RESOLVED"). Headlines: structured `message.tool_calls` (clean T1/T2 parsing); arch GQA-4 / head_dim-256 / no sliding window / 262K base ctx; `/api/ps size_vram` includes KV and KV is pre-allocated by `num_ctx` (so VRAM is measurable over HTTP with tiny prompts); empirical KV ≈34 KiB/token.
3. ~~Build T5~~ **DONE (2026-06-24)** — `scripts/benchmark_longcontext.py` built, validated, and run for `qwen3.5:9b` on T5500 (`results/longcontext-qwen3.5_9b.json`). Result: **128K fits 100% on-GPU at 9.93GB; 256K spills (16.3GB, 65% GPU)**; decode 43→26.7 tok/s across 4K→128K. This is the **12GB-host curve only** — the 256K+/1M + KV-quant (`OLLAMA_KV_CACHE_TYPE`, needs an Ollama restart → coordinate) story belongs on **Strix (128GB)**, and **Qwythos-9B (1M)** + the rest of the cohort still need the same T5 run. T4 (needle + long-C#) is still a stub in that file — not implemented.
4. **Build T1/T2 (tool-use + agentic loop)** via a reusable `scripts/coding_tasks/tool_loop.py` driver (extends the quality-suite `TOOL_TASKS`/`PLAN_AGENT_TASKS`). Cross-machine OK. Tool-call parsing confirmed: read `message.tool_calls[].function.{name,arguments}` (arguments already an object).
5. **Build T3 (agentic C#)** — file-op + build/test tools on top of `coding_tasks/task_runner.py`'s .NET sandbox; loop until tests green.
6. Run the Layer 4 suite across the comparison cohort; add a cross-model chart (cf. `scripts/strix_summary_chart.py`).
