# Model Quirks

Per-model gotchas observed while benchmarking Ollama models, with the underlying reason and a recommended workaround. Sorted roughly by impact on benchmark interpretation. Keep this list current — a misleading score in `benchmark-models.json` is worse than missing data.

## The `think` flag matrix

Ollama exposes a `think` parameter on `/api/chat` and `/api/generate`, but model families react inconsistently.

| Behaviour | Models | What to do |
| --- | --- | --- |
| Honors `think:true`/`think:false` cleanly | gemma4 family (e2b/26b/31b), qwen3.6, qwen3.6:27b, qwen3:14b, qwen3.5 (latest/4b/9b), glm-4.7-flash, nemotron-3-nano family, nemotron-nano-9b-v2-toolfix | Run paired variants. `CODING_BENCH_THINK=true` produces `coding-{slug}-think.json`; default produces `coding-{slug}.json`. |
| Ignores `think:false` (still thinks) | gpt-oss:20b, gpt-oss:120b, lfm2.5-thinking (both sizes), glm-4.7-flash-reap-toolfix, hf.co/prithivMLmods/VibeThinker-3B-GGUF:Q4_K_M | The "no-think" baseline is effectively a think run. A `-think` variant adds little. For gpt-oss, set `reasoning_effort: low\|medium\|high` instead of toggling `think`. VibeThinker-3B additionally returns **empty content** when `think:true` is sent — do NOT use `CODING_BENCH_THINK=true` with this model; it produces 0/50. |
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

- `glm-4.7-flash` (base): honors `think:false` cleanly. L3 corrected (2026-06-25): **35/50 (0.691) nothink** vs 34/50 (0.673) think — nothink marginally edges think. Prior 0/50 nothink was NuGet-restore contamination, not reasoning-content leakage. Both modes usable; nothink is the recommended baseline.
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

## gemma4:12b — 256K-default offload trap + think-dependent-but-runaway reasoning (2026-06-05)

Three distinct gotchas, all proven on Framework (RTX 3060 12GB eGPU, Ollama 0.30.5):

1. **Needs Ollama >= 0.30.5.** 0.24.0 returns `412: requires a newer version` on pull. 0.30.5 release notes
   also fix a gemma4:12b floating-point-exception crash. Update before benchmarking.

2. **Default 256K context overflows 12GB → silent partial offload.** `gemma4:12b` defaults to its full 256K
   context. On a 12GB GPU the 256K KV cache pushes it to partial offload: `ollama ps` shows `size=9.23GB
   vram=6.52GB` and throughput craters to **5.58 tok/s (FAILS the >10 gate)**. Pinned at `num_ctx=131072`
   it loads 100% GPU (`size=7.63GB vram=7.63GB`) at **~18.6 tok/s** (warm; ~13.6 when the eGPU is thermally
   loaded — throughput is thermal-dependent, re-measure warm). Decode speed is context-independent once
   resident; the 256K slowdown is pure offload. **Always pin `num_ctx<=131072` on a 12GB GPU.** The throughput
   script gained a `-NumCtx` param for this; coding suites already pin small contexts (L3 12288, L2 8192).

3. **Think-dependent for complex C#, but reasoning is runaway-verbose.** Controlled A/B on one task:
   think:false emits `with {…}` (missing target) and statement-`switch` → broken; think:true emits
   `order with {…}` and `order.Total switch {…}` → correct. So the model genuinely needs reasoning for
   correct C#. BUT it spends ~2000+ tokens thinking even for one-liners, so at the L3 harness's 4096
   `max_tokens` cap, ~10/50 hard tasks emit only thinking → **"Empty code after extraction"**. Scores:
   L3 think:false 5/50, think:true@4096 6/50 (both weak). A think:true **@8192** ceiling re-run of the 10
   starved tasks still scored **0/10** (8 empty) — so on the hardest .NET tasks the model runs away past
   8192 without converging: a **genuine capability limit, not just budget starvation**. Use
   `CODING_BENCH_MAX_TOKENS` (new env, default 4096) to raise the budget for verbose thinking models.
   Despite weak complex-.NET, gemma4:12b is a **strong broad coder**: L2-chat (think:false) **95/158 (0.601)**,
   quick-quality 11/11. Read L2 + quick-quality, not L3, when judging this model's general coding value.

4. **Unsloth UD-Q4_K_XL == official q4_K_M here.** The dynamic 4-bit quant (via a `FROM`-GGUF Modelfile that
   copies the official gemma4 RENDERER/PARSER) is a dead tie: 11/11 quick, 95/158 L2, ~19.6 vs ~18.6 tok/s
   (within variance). No reason to prefer it over the official tag on this hardware.

## VibeThinker-3B — think:true returns empty content (2026-06-20)

WeiboAI VibeThinker-3B (Qwen2.5-Coder-3B base, prithivMLmods Q4_K_M GGUF). Two think-flag quirks, both proven on Framework RTX 3060:

1. **Ignores `think:false`** — always emits `<think>...</think>` blocks inside the `content` field regardless of the `think` parameter. The `thinking` field in the response is always empty. The no-think L3 baseline is therefore an effective thinking run. L3/L2 extractors already strip `<think>` tags, so this is transparent to the benchmarks.

2. **`think:true` returns empty content** — sending `think:true` causes Ollama to return a response with empty `content` and empty `thinking`. The model produces nothing useful. **Do NOT run `CODING_BENCH_THINK=true` with this model** — it will produce 0/50 on L3 with no generated code, not a capability signal.

Consequence: no `-think` variant is possible or needed. The single L3 run (`coding-{slug}.json`) is the only meaningful coding score.

Capability note: L3 (.NET) 0/50 and L2 chat 25/158 (0.158) reflect a genuine training domain gap — the model was trained on LeetCode-style competitive programming (Python), math, and STEM, not enterprise .NET APIs. Quality coding score (2/5) is suppressed by the 256-token think budget. See `scripts/lora/` for the C# fine-tuning pipeline targeting this model.

## Qwen3.5 / Qwen3.6 family — clean `thinking` field, and "think hurts .NET L3" (2026-06-23)

`qwen3.5:9b`, `hf.co/empero-ai/Qwythos-9B-Claude-Mythos-5-1M-GGUF:Q4_K_M` (Qwen3.5-9B FT),
and `qwen3.6*` all handle the think flag **cleanly**, unlike VibeThinker:

- They **think by default**; the chain-of-thought goes to a **separate `thinking` field** in the `/api/chat` response, NOT as `<think>...</think>` inside `content`. `content` is therefore always clean — the coding extractors need no special handling.
- `think:false` **works** (suppresses thinking, returns a direct answer). `think:true` works. No empty-content trap (contrast VibeThinker, which ignores `think:false` and returns empty on `think:true`).
- The Qwen3.5 arch **loads fine in current Ollama** (no unknown-arch block) for the 9B sizes; large `qwen35moe`/dense 27B-35B variants still panic on some Ollama builds (see backend_notes).

**"Thinking HURTS Coding L3 (.NET)" — consistent across the family:** Qwythos-9B 6/50 no-think vs 4/50 think; qwen3.6 35B-A3B 40/50 vs 35/50; qwen3.6:27b similar. For these .NET framework tasks, use the **no-think baseline**; the paired think variant reliably underperforms (extra reasoning budget diverges rather than helps). Probe with `scripts/check_think_support.py` but expect the separate-field behaviour above.

Fine-tune-vs-base finding: the `Qwythos` "Mythos" fine-tune scores **below** its base `qwen3.5:9b` on C# coding (L2-chat 62 vs 67, L2-raw 50 vs 61) — the tune did not help coding. Its value is agentic/tool/long-context; see `docs/agentic-tool-longcontext-benchmark-plan.md` for the planned Layer 4 suite to measure those strengths.

## mistral-medium-3.5:iq3m — bool `think:true` is a SILENT no-op; use `think:"high"` (2026-07-04)

Dense 125B, 256k ctx, imported as **IQ3_M 59.5 GB** (bartowski imatrix, text-only) because the official 80 GB Q4 cannot load under the Strix 96 GB BIOS split (see PLATFORM_QUIRKS 2026-07-04). Import replicates the official registry packaging exactly: same TEMPLATE and SYSTEM blobs (digest-identical) plus `parser:"ministral"` patched into the local config (`gguf-cache/mm35-meta/patch_parser.py` — Modelfile has no PARSER directive). Sharded-GGUF note: Ollama can neither pull nor `create` from HF shards; merge with `llama-gguf-split --merge` first, and budget ~2× disk — `ollama create` also writes a full *patched* copy during its llama-quantize validation (and leaks `sha256-<10digits>` partial blobs on failure; delete them).

1. **Think flag (verified by probe):** the official template maps only `ThinkLevel=="high"` to `reasoning_effort:"high"`. Boolean `think:true` is accepted WITHOUT error but renders `"none"` — a silent no-op (4-token direct answer, empty thinking). `think:false`/absent = fast mode (correct no-think baseline). **Thinking runs must use the string `"high"`** (`CODING_BENCH_THINK=high` for L3). With `"high"`, reasoning lands cleanly in the separate `thinking` field (ministral parser), `content` stays clean.

2. **Fair sampling differs by mode (official Mistral guidance):** thinking → temp 0.7 / top_p 0.95 (task_runner.py has a `mistral-medium*` override keyed on CODING_BENCH_THINK); non-thinking → temp 0.0–0.7 / top_p 1.0, so the harness default temp 0 / top_p 1 is fair and comparable.

3. **Throughput (Strix, 100% GPU, 63 GB with 12k ctx):** decode ~3.0 tok/s (dense = full weight sweep per token; contrast MoE gpt-oss:120b), prefill ~71 tok/s, cold load ~63 s. Budget accordingly: raise `L2_GEN_TIMEOUT_S` (default 150 s barely fits ~300 raw tokens), and expect chat-mode calls to pay a ~525-token system-prompt prefill (~8 s) every request — the official Le Chat SYSTEM is baked in and auto-injected when no system message is sent (raw mode bypasses it).

4. **L2 raw will be an artifact** — chat-tuned instruct model; expect the usual 1–10% raw-mode score. Run `benchmark_coding_layer2_chat.py` for the honest number and mark raw ARTIFACT in backend_notes.

5. **`num_ctx:0` / missing num_ctx = 256k KV OOM (2026-07-04).** This is a 256k-context model: any request that resolves to "model max" context tries to allocate ~88 GB of KV (64 GiB GPU + 24 GB CPU) and 500s. `benchmark_throughput_resource.ps1` sends `num_ctx:0` by default — run it with `-NumCtx 8192`. The imported model now carries a `num_ctx 8192` params layer (matching this box's `OLLAMA_CONTEXT_LENGTH=8192` default that other models ran under), added by manifest surgery like the parser; per-request options still override (L3 uses 12288).

6. **Perf-lever sweep (2026-07-05): confirmed bandwidth-bound, only concurrency moves the needle.** Full test plan/scripts: `scripts/benchmark_sweep.py` (runtime options) + `scripts/perf_lever_service_sweep.ps1` (service-restart levers), results in `results/perf-levers/`.
   - **Roofline:** IQ3_M is ~57.8 GiB resident; at Strix's ~256 GB/s unified-memory bandwidth, a dense model's one-full-weight-sweep-per-token decode has a theoretical ceiling of **~4.1 tok/s**. The 2.93 tok/s originally recorded (07-04) is ~71% of that ceiling — already fairly bandwidth-efficient.
   - **`num_thread` (8/16), `num_batch` (1024), `num_gpu` (99 forced) — no effect**, all landed within noise of each other and of baseline. Expected: decode is a single-token GEMV that's already 100% GPU-offloaded; these levers affect prefill/host-orchestration, not the bandwidth-bound decode step. Do not bother re-tuning these for this model.
   - **`OLLAMA_KV_CACHE_TYPE=q8_0` / `q4_0` — no effect** on single-stream decode tok/s vs f16 default. Expected: at 8192 ctx the KV cache is a small fraction of the ~58 GB swept per token for a 125B dense model, so quantizing it barely moves the total bytes-per-token.
   - **`OLLAMA_NUM_PARALLEL=2` + 2 concurrent requests — real win: ~1.8x aggregate wall-clock throughput** (311.7s serialized wall-clock for 2×165 tokens at `NUM_PARALLEL=1` vs 171.8s at `NUM_PARALLEL=2`, despite each stream's own reported tok/s staying ~1.05-1.08 either way — Ollama batches the concurrent decodes, amortizing the weight-sweep across streams). This doesn't speed up one generation, but running the L2/L3 harnesses with concurrency ≥2 could cut suite wall-clock roughly in half. Caveat: only tested at concurrency=2 given tight system RAM (see PLATFORM_QUIRKS.md orphan-leak entry below) — each additional concurrent stream adds its own KV cache; do not push higher without more RAM headroom.
   - **Throughput decays within a session, even with a clean process (no leak).** The service sweep's `baseline_single` variant — 3 back-to-back identical calls right after a fresh restart — measured **2.72 → 1.24 → 1.11 tok/s**, decaying inside of ~5 minutes with zero config change. All subsequent variants (KV cache, concurrency) then sat around ~1.05-1.1 tok/s regardless of lever. This means the original 2.93 tok/s is a **cold/best-case number**, not what a multi-hour benchmark suite will actually sustain — budget suite wall-clock off **~1.0-1.2 tok/s steady-state**, not 2.93. Root cause not fully isolated (thermal throttling vs. cumulative memory/bandwidth contention vs. both — see STRIX_HALO.md's existing "decode is bandwidth-contention-limited, ~40% swings" note, though this decay is larger and monotonic within-session rather than just run-to-run noise); a clean reboot-vs-just-restart A/B would be needed to separate thermal from memory causes if it's worth the time.
   - **Restart-based lever tests hit a real Ollama-restart bug along the way**: `Stop-Process -Name ollama,"ollama app"` orphans the `llama-server.exe` child, leaking the whole model's memory — see PLATFORM_QUIRKS.md 2026-07-05 entry. Confirmed as the cause of a mid-session near-total-OOM (432 MB available RAM, subsequent model load failed with `cudaMalloc failed: out of memory`); killing the orphan directly recovered to 24 GB available with no reboot needed. Any future restart-based testing on this model must explicitly kill `llama-server` too.
   - **Practical implication for L2/L3 timeouts**: `L2_GEN_TIMEOUT_S` guidance in point 3 above (default 150s "barely fits ~300 raw tokens" at ~3 tok/s) should be re-budgeted against the ~1.0-1.2 tok/s steady-state number instead — 150s only covers ~150-180 tokens at steady state, not 300.

---

## codegemma:latest / codegemma:2b (2026-07-12)

1. **`OLLAMA_HOST=0.0.0.0:11434` (no scheme) silently zeroes every score.** Hit this on `codegemma:latest` — a stale shell env var (missing `http://`, and `0.0.0.0` isn't a valid client target) made every request in `benchmark_quality.py` fail; the harness's broad `except Exception: pass` per-task swallowed it into a clean-looking 0/11 JSON output with no error surfaced. Manual `curl` confirmed the model was fine. **Always confirm `echo $OLLAMA_HOST` resolves to a real `http://host:port` before trusting a near-zero score**, especially after cross-machine work in the same shell session leaves it set to something unusual.

2. **`codegemma:2b` is a BASE/completion-only model, not instruction-tuned** (unlike `codegemma:latest`). It degenerates into repetition or garbage on any natural-language/instruction prompt, in both `/api/generate` (raw) and `/api/chat` — confirmed via manual probe, this is genuine model behavior, not a harness bug. Its official FIM special tokens (`<|fim_prefix|>`/`<|fim_suffix|>`/`<|fim_middle|>`) are **not registered as special tokens by this Ollama GGUF pull** — they get echoed back as literal text instead of controlling generation, so true FIM-templated prompting doesn't work through this harness/pull either. It does score non-trivially (16.5%) on the L2 raw dataset specifically because that dataset's prefix shape (`class Problem { method(...) {`) happens to resemble natural code context, not because raw-mode unlocks real FIM behavior.

---

*Extend and refine these notes as insights are proven*
