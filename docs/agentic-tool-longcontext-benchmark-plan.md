# Layer 4 — Agentic / Tool-Use / Long-Context benchmark (design & test plan)

Status: **DESIGN — not yet implemented.** Authored 2026-06-23. Successor work to the
Layer 1–3 coding suite (quality / throughput / L2 HumanEval-CS / L3 .NET).

## Why this exists

The existing layers measure **single-shot code generation** (L2 = fill-a-method, L3 =
one-shot .NET task). They do **not** measure the capabilities we actually want for
agentic coding workflows:

- **Tool use** — correct tool selection, correct arguments, multi-tool, multi-step chains, error recovery, incorporating tool results.
- **Agentic loops** — plan → act → observe → adapt over many turns until a goal (build+tests pass) is met.
- **Long context** — reasoning/retrieval over large inputs (whole repos, long specs), and crucially the **VRAM/throughput cost of long context** (does the arch have KV/attention optimisations, or does context blow up memory?).

Trigger: `empero-ai/Qwythos-9B-Claude-Mythos-5-1M` (Qwen3.5-9B fine-tune) scores
*worse* than its base on one-shot C# coding (L2-chat 62 vs base 67), but it is
explicitly built for **reasoning / function-calling / agentic / 1M-context / tool-use**.
Layers 1–3 cannot see that value. Layer 4 is designed to. The goal is to find **what
this model class should achieve**, push it to its **limits**, and run the same suite
across other models for comparison.

## What "good" looks like (targets to validate, not assume)

These are hypotheses to measure, refine after the first run, then hold other models to:

| Dimension | Target (provisional) | How measured |
|---|---|---|
| Single-tool correctness | ≥95% correct tool + args | T1 below |
| Multi-tool selection (distractors) | ≥90% | T1 |
| Sequential dependent tool chain | ≥75% complete | T2 |
| Error recovery (tool returns error/retry) | ≥60% recover | T2 |
| Agentic C# (feature → build+tests green) | ≥40% within N=10 steps | T3 |
| Long-context retrieval (needle) | ≥90% up to the *effective* ctx limit | T4 |
| Effective context limit | find where needle acc < 80% | T4 |
| KV VRAM cost | GB per 10k tokens; max ctx on 12GB & 128GB | T5 |
| Long-context decode throughput | tok/s vs ctx length curve | T5 |

## Test categories

### T1 — Tool-use correctness (extends the quality-suite TOOL_TASKS)
Drive `/api/chat` with the `tools` parameter; assert the emitted `tool_calls`.
Cases: single tool; multi-tool with distractors (must pick the right one); nested/object
args; **parallel** tool calls (model emits ≥2 in one turn); **sequential dependent**
(result of tool A feeds tool B); malformed-arg rejection. Score = exact tool name +
schema-valid args match. Pure generation → runs cross-machine via `OLLAMA_HOST` (no dotnet).

### T2 — Agentic loop (general, language-agnostic)
A real ReAct driver: expose a small tool registry (e.g. `calculator`, `kv_store_get/set`,
`search_corpus`, `finish`), give a goal needing 3–8 tool steps, loop
prompt→tool_call→execute→feed-result until `finish` or step cap. Score = goal achieved +
step efficiency + did it recover from an injected tool error. Builds on the existing
`PLAN_AGENT_TASKS` (create_plan / request_subagent / finalize_result) pattern in
`benchmark_quality.py` — generalise that into a reusable tool-loop harness.

### T3 — Agentic C# coding (the headline test)
The C#-specific agentic workflow. Sandbox a small .NET project; expose tools:
`read_file`, `list_files`, `write_file`, `run_build`, `run_tests`. Give a feature/bug
task with failing tests. Model must iterate (read → edit → build → test → fix) until
tests pass or step cap (N=10). Score = tests green (binary) + steps used + build-fails
survived. **This is the real "agentic code generation with tools" metric.** Generation
on the GPU host (`OLLAMA_HOST`), build/test on the Framework dotnet harness — reuse the
configurable timeouts (`L3_BUILD_TIMEOUT_S` / `L3_TEST_TIMEOUT_S`).

### T4 — Long-context comprehension & retrieval
- **Needle-in-a-haystack**: inject a unique fact at a known depth in filler of length
  L ∈ {4k, 16k, 32k, 64k, 128k, 256k, 512k, 1M} tokens; ask for it. Sweep depth × length.
  Accuracy-vs-length curve → the **effective** context limit (where it collapses; the
  "1M" claim is a marketing number until measured).
- **Long C# comprehension**: synthesize a repo of growing size (N files); ask cross-file
  questions / request a change requiring facts from files far apart. Tests usable long
  context, not just retrieval.

### T5 — Long-context VRAM & throughput scaling (the KV-efficiency question)
The user's key question: *does this support long context without massive VRAM?* Measure it.
- For ctx ∈ {4k, 32k, 128k, 256k, …}: set `num_ctx`, fill the KV cache to that many tokens,
  sample `nvidia-smi` VRAM + decode tok/s.
- Derive **GB-per-10k-tokens of KV** and the **max context that fits** on Framework
  (12 GB RTX 3060) vs Strix (128 GB unified).
- Check arch levers: GQA group size, sliding-window attention, and whether Ollama exposes
  **KV-cache quantisation** (`OLLAMA_KV_CACHE_TYPE=q8_0/q4_0`) and flash-attention for this
  arch — these are what make 1M context viable on small VRAM. Report which apply.

## Harness components to build (next session)

1. `scripts/coding_tasks/tool_loop.py` — generic tool-calling driver: registry, `/api/chat`
   `tools` round-trip, execute → feed-back loop, step cap, transcript capture. Honors `OLLAMA_HOST`.
2. `scripts/benchmark_layer4_tools.py` — T1 + T2 (pure generation; cross-machine).
3. `scripts/benchmark_layer4_agentic_csharp.py` — T3, on top of the existing .NET sandbox
   harness in `coding_tasks/task_runner.py` (add file-op + build/test tools).
4. `scripts/benchmark_longcontext.py` — T4 (needle + long-C#) and T5 (VRAM/throughput sweep).
   T5 must run **on the GPU host** (Strix/T5500) to read local VRAM; T1–T4 can run cross-machine.
5. Result files: `layer4-tools-{slug}.json`, `layer4-agentic-{slug}.json`,
   `longcontext-{slug}.json`; one `backend_notes` line per model as usual.

## Comparison cohort (run the same suite across these)

Qwythos-9B (1M ctx), its base qwen3.5:9b, qwen3.6 (262K, 35B-A3B MoE) + qwen3.6:27b,
gemma4 family (long ctx + vision), GLM-4.x, and the strongest coders on hand
(qwen3-coder-next, gemma4:12b). The point is a **cross-model agentic/tool/long-context
chart** the way `strix_summary_chart.py` does for L1–L3 today.

## Open questions — RESOLVED 2026-06-24

Probed against `qwen3.5:9b` on T5500 (Ollama 0.30.10) + the `Qwen/Qwen3.5-9B` HF `config.json`.

- **Qwen3.5 attention (Q1):** `model_type qwen3_5`, arch `Qwen3_5ForConditionalGeneration` (multimodal). **32 layers, 16 query heads, 4 KV heads → GQA group size 4. head_dim 256** (note: *double* the usual 128 → heavy KV). hidden 4096, rope_theta 1e7, max_position 262144. **No sliding window** (full attention; `sliding_window`/`use_sliding_window`/`max_window_layers` all absent). `ollama show` reports base **context length 262144 (262K)**, capabilities completion/vision/tools/thinking — the "1M" is Qwythos's rope-extended claim, base is 262K.
  - **KV-cache math** (per token, f16): `2·layers·kv_heads·head_dim·2B = 2·32·4·256·2 = 131072 B = 128 KiB/token` ⇒ **~1.28 GiB per 10k tokens**. Full 262K ≈ **~32 GiB f16** — does NOT fit Framework's 12 GB 3060 even with q4_0 KV (~8 GiB) + 6.6 GB weights. Long ctx ⇒ needs KV quant *and* a big-VRAM host (Strix 128 GB), or heavy ctx truncation. T5 measures the real curve.
- **Flash-attn / KV-quant (Q2):** Ollama 0.30.10 exposes the server-level `OLLAMA_FLASH_ATTENTION` and `OLLAMA_KV_CACHE_TYPE` (`q8_0`/`q4_0`) knobs; **neither is currently set on T5500** (defaults: f16 KV). Testing KV-quant requires **restarting the shared Ollama** with the env var → coordinate via the user. Open empirical sub-question for T5: does the bundled llama.cpp FA kernel support **head_dim 256** for `qwen35` (some FA impls cap/specialise head_dim) — confirm by toggling the flag and watching load logs + VRAM.
- **Tool-call format (Q3):** Ollama returns **structured** `message.tool_calls` for qwen3.5 — `[{id, function:{name, arguments:{…}}}]` with `arguments` already parsed to an object (not a JSON string), `content` empty on a tool turn, `done_reason: stop`. T1/T2 parse `message.tool_calls[].function.{name,arguments}` directly; no inline-text scraping.
- **Token accounting (Q4):** `prompt_eval_count` (prompt tokens) and `eval_count` (generated tokens) are present on every `/api/generate` + `/api/chat` response — use `prompt_eval_count` to hit exact target lengths in the T4/T5 sweeps.
- **T5 measurement primitive (probed 3 loads on T5500, qwen3.5:9b):** `/api/ps` `size_vram` **includes the KV cache** and KV is **pre-allocated by `num_ctx`**, NOT grown by actual tokens — confirmed: num_ctx 8k→32k moved `size_vram` 5.88→6.74 GB, while a 4032-token prompt at num_ctx=8k left it unchanged at 5.88 GB (== the 2-token load). **Implications for the harness:** measure the whole VRAM curve over HTTP with **tiny prompts** at each `num_ctx` (no large prefill, no CPU-offload-prefill risk, `nvidia-smi` optional); use `size_vram < size` as the fit/offload detector; only send a real ~L-token prompt for the *throughput* metric, and only where it fits fully. **Empirical KV cost ≈ 35 KiB/token** (0.86 GB / 24576 tokens of Δnum_ctx) — far below the 128 KiB/token f16 hand-calc above (likely default KV layout/flash-attn or a head_dim nuance; report the *measured* number). So the 12 GB fit boundary is ~180k tokens, not ~34k — 128k context plausibly fits f16 on a 3060. T5 confirms the exact curve.

## Immediate carry-over (unfinished from the 2026-06-23 session)
- `qwen3.5:9b` (base) needs a **fresh L2-raw re-run** (today's hung at ~121/158; on-disk file is a stale 2026-04-17 prior) **+ L3 no-think + L3 think** to complete the base-vs-Qwythos coding comparison. Model is pulled on T5500.
- Known L2-raw hang: a generation request orphaned past its timeout while Ollama stayed healthy — investigate the urllib read-timeout robustness in `benchmark_coding_layer2.py` before long unattended raw runs.
