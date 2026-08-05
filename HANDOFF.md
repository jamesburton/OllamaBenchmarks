# HANDOFF — Gemma 4 12B benchmarking on Framework

**Last updated:** 2026-06-05 — ✅ **COMPLETE**
**Machine:** Framework (Intel Core Ultra 7 155H + **RTX 3060 12GB eGPU over Thunderbolt 4**)

## ✅ FINAL VERDICT
**Keep installed: official `gemma4:12b` (q4_K_M, 7.6GB).** It's the only quant that meets the gate on this
12GB GPU (q5+/q8/mxfp8/nvfp4 offload → Strix Halo candidates). The Unsloth **UD-Q4_K_XL** dynamic quant was
benchmarked as the only same-size-class alternative and is a **dead tie** (identical 11/11 quick-quality &
95/158 L2; ~19.6 vs ~18.6 tok/s, within variance) → official tag wins (no custom-Modelfile dependency); UD
model removed, its Modelfile (`models/gemma4-12b-ud-q4kxl.Modelfile`) + cached GGUF kept for re-creation.

**What gemma4:12b is:** a gate-passing 128K generalist + **strong broad coder** (L2 60.1%, quick-quality
11/11) but a **weak reasoning-heavy .NET coder on a 13-tps box** (L3 ~6/50 — runaway verbose reasoning, a
genuine limit confirmed by the 0/10 8192-ceiling check, not budget). For .NET coding on Framework, the faster
perfect-quality models (glm-4.7-flash, qwen3-coder-next) remain the better pick. Committed.

**Resume note:** if revisiting, the open follow-up is a Strix Halo run of the higher quants (q5/q6/q8/bf16)
and a larger-budget standardized think:true L3 for cross-machine comparison.

---

## Goal

Add **Gemma 4 12B** to the benchmark suite, find the quant(s) that run well *on this machine*, and
benchmark them. Selection rule from the user:

1. **Hard gate:** `>10 tok/s` **AND** `128k+ context`. On Framework this is **binary on GPU fit** —
   the model + 128k KV cache must fit fully in the **12GB RTX 3060** (no partial offload; TB4 partial
   offload is *slower* than CPU — see `memory/feedback_egpu_offload.md`). Resident ≈ 30–50 tps; spilled ≈ 3–5 tps.
2. Among gate-passers, **weight coding quality highest**, with a **speed tiebreak**: a model that is
   "almost as good and much faster" can edge ahead of a slightly-better-but-slower one.
3. **Keep only the top performer installed**; `ollama rm` the others after benchmarking (result JSONs in
   `results/` persist after removal, so scores aren't lost).
4. Give a full summary per model; progress updates at most every 30 min on long runs.

## Model facts (researched this session)

- **Gemma 4 12B** released **2026-06-03**. Dense decoder-only, multimodal (text/image/video/audio in),
  **256K (262,144) native context**. Ollama: `gemma4:12b`.
- Ollama quant tags & sizes: `12b`(=`12b-it-q4_K_M`) **7.6GB** · `12b-it-q8_0` 13GB · `12b-it-bf16` 24GB ·
  `12b-mlx`/`mlx-bf16` (Apple-only) · `12b-mxfp8` 12GB · `12b-nvfp4` 10GB.
- Unsloth recommends dynamic **`UD-Q4_K_XL`** (HF: `unsloth/gemma-4-12B-it-GGUF`).

## Scope decision (user-confirmed)

On **Framework**, only the quants that fit the 12GB gate get benched here; the rest go to **Strix Halo**
(128GB unified memory) where they run >10 tps:

| Quant | Size | Framework plan |
|---|---|---|
| **q4_K_M** (default `gemma4:12b`) | 7.6GB | **Primary candidate** — benchmark fully. |
| **Unsloth UD-Q4_K_XL** | ~7.8GB | A/B vs q4_K_M (same size class, better quality, no speed cost). Pull from HF; if it lacks gemma4 RENDERER/PARSER, build a Modelfile copying template/params from official `gemma4:12b` (`ollama show --modelfile`). |
| mxfp8 (12GB), nvfp4 (10GB) | — | **Skip on Framework** — no FP-accel on Ampere / leaves no room for 128k KV. |
| q5 / q6 / q8 / bf16 | 8.8–24GB | **Strix Halo only** — would offload on Framework (<10 tps). Queue as Strix candidates. |

## Current progress

- [x] Verified hostname = Framework; RTX 3060 eGPU present (12288 MiB, ~11.5GB free, driver 591.86).
- [x] Researched Gemma 4 12B (exists, specs above).
- [x] Started Ollama server (was not listening on :11434).
- [x] ~~BLOCKER: Ollama 0.24.0 too old (`412`)~~ → **RESOLVED: updated to v0.30.5** (user ran the tray
      updater; v0.30.5 notes explicitly *"Fix gemma4:12b floating point exception crash"*). Server confirmed up.
- [~] Downloads in progress (parallel, background): (1) `ollama pull gemma4:12b` (q4_K_M, 7.6GB);
      (2) HF GGUF `gemma-4-12b-it-UD-Q4_K_XL.gguf` (single file, 7.37GB) → `C:\Users\james\.cache\gemma4-gguf\`.
      UD route = **option (b)** per user: build a Modelfile `FROM` that GGUF, copying TEMPLATE/RENDERER/PARSER
      from `ollama show --modelfile gemma4:12b`.
- [x] **GATE TEST — PASSED for q4_K_M.** Loaded at `num_ctx=131072`: `ollama ps` = `8.2 GB, 100% GPU,
      CONTEXT 131072`; `nvidia-smi` = 10.8GB on the **RTX 3060** via `llama-server.exe` (CUDA, NOT Arc iGPU —
      env worry resolved). Fits 128k fully with ~1.3GB headroom. No KV-cache-type override needed (kept f16).
      - Throughput (via `/api/chat`, the correct gemma4 path; `/api/generate` returns empty due to RENDERER bypass):
        **~13 tok/s decode, context-independent** (12.19 @ 8k, 12.94 @ 128k). `think=false` → clean code.
      - ⚠️ ~13 tps is ~1/3 of gemma3:12b's 36 tps on this same box — likely the heavier unified-multimodal
        gemma4 arch. Still clears the >10 gate. Will confirm with the official throughput script.
- [x] UD-Q4_K_XL Modelfile built: `models/gemma4-12b-ud-q4kxl.Modelfile` (option b — FROM HF GGUF + official
      gemma4 TEMPLATE/RENDERER/PARSER). Create with `ollama create gemma4-12b-ud-q4kxl -f <file>` when ready.
- [ ] Benchmark q4_K_M, then UD-Q4_K_XL.
- [ ] Update `benchmark-models.json` (`benchmark_suite`, `local_installed`, `backend_notes`), commit.

## ⚠️ KEY FINDING: pin num_ctx to 131072 (the 256K default offloads)

gemma4:12b's **default context is 256K**, whose KV cache overflows 12GB → **partial offload → 5.58 tok/s
(FAILS gate)**. Pinned at **128k it's 100% GPU → 13.63 tok/s (PASSES)**. Decode speed is context-independent
once resident (the 256K number is slow purely from offload, not compute). Consequences:
- Throughput must be measured pinned: added a backward-compatible **`-NumCtx`** param to
  `scripts/benchmark_throughput_resource.ps1` (default 0 = old behavior). Ran with `-NumCtx 131072`.
- Coding suites are already safe: L3 (`task_runner.call_ollama` num_ctx=12288) and L2-chat (num_ctx=8192)
  both fit fully → ~13 tps, no offload. No change needed.
- gemma4:12b is ~13 tps vs gemma3:12b's 36 on this box — heavier unified-multimodal arch, not a config bug.

## Confirmed numbers — gemma4:12b q4_K_M (Framework eGPU)
- Throughput @128k: **13.63 tok/s**, 100% GPU, 7.63GB VRAM. (@256k default: 5.58, partial offload.)
- Gate: **PASS** (>10 tps AND 128k @ 100% GPU).
- Quick quality: **11/11** (coding 5/5, tool 4/4, agentic 2/2).
- **L3 .NET think:FALSE: 5/50 (0.091)** — REAL but UNREPRESENTATIVE. Generated code is complete & extraction
  is clean (no artifact), but the model makes genuine C# syntax slips without reasoning (e.g. `with {...}`
  missing its target, statement-`switch` instead of `x switch {...}`). The **entire gemma4 family is
  benchmarked think:TRUE** (e2b 28, 26b 32, 31b 45) — think:false is the weak baseline. No-think generated
  code preserved at `results/coding-generated/gemma4_12b-nothink/`.
- **L3 .NET think:TRUE @4096: 6/50 (0.109)** → `coding-gemma4_12b-think.json`. All 6 passes are Blazor;
  **10 tasks empty-extracted** (budget starvation). So think:true ≈ think:false at standard budget — the
  4096 cap cancels the reasoning gain. Honest headline = **6/50** (a poor fit for reasoning-heavy coding on
  a 13-tps box, NOT the model's intrinsic ceiling — see 8192 spot-check).
- 10 starved tasks (ceiling-check set): aspnet_validation_endpoint, masstransit_statemachine,
  xunit_v3_theory_tests, vertical_blazor_crud, linq_set_operations, linq_todictionary_tolookup,
  async_whenall_parallel, async_semaphore_throttle, async_valuetask_cache, masstransit_request_response.
- **L2-chat (think:false, HumanEval-CS): 95/158 (0.601)** → `coding-gemma4_12b-chat.json`. STRONG — beats
  mistral-small (0.38), GLM-4.5-Air-REAP (0.52), qwen3.6 (0.49). Confirms gemma4:12b is a capable BROAD coder
  on simple single-function tasks even without thinking; its weakness is narrowly the complex reasoning-heavy
  .NET L3 tasks at budget-constrained settings.
- **Ceiling @8192 (10 hardest L3 tasks, think:true): 0/10 (8 still empty-extract).** CONCLUSIVE: the weak
  complex-.NET score is a **genuine capability / runaway-reasoning limit, NOT budget starvation** — even at
  double budget the model thinks past 8192 without converging to code. True L3 think:true ceiling ≈ 6/50.

## q4_K_M FINAL SCORECARD (Framework RTX 3060 eGPU, Ollama 0.30.5)
| Metric | Result | Note |
|---|---|---|
| 128k gate | **PASS** | 100% GPU, 7.63GB, 13.63 tok/s |
| Throughput @128k | 13.63 tok/s | @256k default: 5.58 (offload) |
| Quick quality | **11/11** | coding 5/5, tool 4/4, agentic 2/2 |
| L2-chat (HumanEval-CS, think:false) | **95/158 (60.1%)** | STRONG broad coder |
| L3 .NET think:false | 5/50 (9.1%) | real syntax slips |
| L3 .NET think:true @4096 | **6/50 (10.9%)** | headline; 10 empty-extract |
| L3 .NET ceiling (10 hardest @8192) | 0/10 | confirms real limit |

**Verdict:** strong *broad* coder + perfect quick-quality, but a *weak reasoning-heavy .NET* coder on this
box (runaway verbose reasoning at 13 tps). Effective time-to-code on hard tasks is poor.

UD comparison (lean): throughput@128k + quick-quality + L2-chat. Skip UD L3 (would be ~6/50 noise).

## UD-Q4_K_XL (option b: Unsloth dynamic 4-bit + gemma4 renderer) — in progress
Model created: `gemma4-12b-ud-q4kxl` (FROM HF GGUF, 6.86GB). Smoke test: 100% GPU @128k (8.1GB), clean code
via chat path (renderer/parser work). Gate: **PASS**.
| Metric | UD-Q4_K_XL | q4_K_M |
|---|---|---|
| Throughput @128k | **19.61 tok/s** (?) | 13.63 tok/s |
| Quick quality | **11/11** | 11/11 |
| L2-chat | RUNNING | 95/158 (60.1%) |
NOTE: UD's 19.61 vs q4's 13.63 is a surprise (leaner quant, or GPU-clock/thermal drift). **Must verify with a
clean back-to-back throughput A/B** before crediting UD a speed win. If real + L2 ties → UD could edge q4 on
the speed tiebreak (but loses the "official tag, no custom Modelfile" convenience — note for the decision).

**Methodology note for this model:** headline coding quality = **think:true L3 + chat-mode L2**. Compare
q4_K_M vs UD-Q4_K_XL on the SAME (think:true) setting.

## CONTROLLED EXPERIMENT — think:true fixes the syntax (proven, hardware-independent)
Replayed `patterns_record_with` (a think:false FAIL) at **think:true, budget 8192**:
- think:false output: `with { Status="Shipped" }` and statement-`switch` → **broken** (real syntax errors).
- think:true output: `order with { Status="Shipped" }` and `order.Total switch {…}` → **correct**.
- BUT thinking alone ≈ **2000 tokens** (thinking_len 7904 chars, eval_count 2281). Hard tasks exceed the
  harness's **4096** `max_tokens` → model emits only thinking, **"Empty code after extraction"**.

**Conclusion:** gemma4:12b genuinely needs reasoning for correct C#. think:false=5/50 is a real FLOOR. The
think:true@4096 run is partly budget-limited — and on a 13-tps box that over-thinking (~2.5 min before code)
is a **real cost**, so per advisor we KEEP the standard-4096 think:true result as the honest headline rather
than rescue it with a bigger budget.

## DECISION PLAN (advisor-aligned) — DO NOT budget-spiral
- **Headline** = think:true L3 @ **standard 4096**, run to completion, failures kept as honest signal. (Running.)
- **Floor** = think:false L3 = **5/50**. **Ceiling** = think:true @ **8192** spot-check on just the hardest
  (empty-extraction) tasks — documents intrinsic capability vs budget limit. Use `CODING_BENCH_MAX_TOKENS=8192`
  (new backward-compat env override added to `task_runner.run_task`, default 4096).
- **Do NOT** chase the family's L3 numbers (e2b 25 / 26b 32 / 31b 45 are **Strix / Ollama 0.22**, not
  comparable to Framework / 0.30.5). Only q4-vs-UD must be clean (identical settings).
- **L2-chat** already forces `think:False` (raw-completion measure, no thinking → no starvation) → run as-is.
- **Right-size UD-Q4_K_XL**: quick-quality + one think:true L3 @4096. If within ~3 pts of q4_K_M → call it a
  tie and **keep the official `gemma4:12b` q4_K_M** (no custom-Modelfile dependency); skip UD's L2.
- **Final summary**: report **effective time-to-code** (thinking overhead), not just 13 tok/s.
- Commit both harness edits (`-NumCtx`, `CODING_BENCH_MAX_TOKENS`) with one-line methodology notes.

## Benchmark sequence (per fitting quant)

Slug rule: strip `:latest`, replace `[:/\\]` → `_`. gemma4:12b → `gemma4_12b`.

1. **Fit + throughput AT 128k** — `./scripts/benchmark_throughput_resource.ps1 -Models gemma4:12b`
   (NOTE: script caps num_ctx ≤32k per PLATFORM_QUIRKS — a 32k pass does NOT prove the 128k gate; prove 128k
   separately via the load test above).
2. **Quick quality** — `python scripts/benchmark_quality.py --models gemma4:12b --output results/quality-gemma4_12b.json`
3. **Coding L3 (.NET, 50 tasks)** no-think — `python scripts/benchmark_coding_layer3.py --models gemma4:12b ...`
4. **Coding L2 CHAT mode** — `python scripts/benchmark_coding_layer2_chat.py --models gemma4:12b ...`
   ⚠️ gemma4 is chat-tuned: **raw-mode L2 is an ARTIFACT** (template bypass — see MODEL_QUIRKS). Headline L2 = chat.
5. **L3 think variant** (gemma4 honors `think:true`) — only on the *final winner* (doubles L3 time):
   `$env:CODING_BENCH_THINK='true'; python scripts/benchmark_coding_layer3.py ...`

Efficiency: same-model quants run at ~equal tps when resident, so do the expensive L2/L3 only on the top
1–2 contenders, not every quant. Coding quality is the real discriminator between quants.

Hold constant across all quants: flash-attn (already `OLLAMA_FLASH_ATTENTION=1`), KV-cache-type. Document any change.

## What worked
- `ollama serve` from `C:/Users/james/AppData/Local/Programs/Ollama/ollama.exe` brought the API up on :11434.
- nvidia-smi confirms eGPU live.

## What didn't work
- `ollama list` / `ollama pull` via CLI initially failed because no server was listening (only the tray
  "ollama app" process existed). Fixed by starting `ollama serve`.
- Pulling gemma4:12b on 0.24.0 → 412 version error. Must update first.

## Next steps
1. Finish Ollama update to 0.30.5; restart server; confirm `api/version`.
2. Pull `gemma4:12b`; run the **128k gate load test**.
3. If gate passes → run benchmark sequence on q4_K_M.
4. Pull/build **UD-Q4_K_XL**; gate test; benchmark if it passes.
5. Pick winner per criteria; `ollama rm` the loser; update JSON + commit.
6. Queue q5/q6/q8/bf16 as **Strix Halo** candidates in `benchmark_suite` with `backend_notes`.
