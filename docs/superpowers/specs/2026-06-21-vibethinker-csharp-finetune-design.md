# VibeThinker-3B → C# Specialist: Fine-Tuning Design

**Date:** 2026-06-21
**Status:** Approved (pending spec review)
**Author:** James Burton + Claude

## Goal

Fine-tune `WeiboAI/VibeThinker-3B` (Qwen2.5-Coder-3B base) into a C#/.NET
specialist. The base model is trained on LeetCode-style competitive programming
(Python), math, and STEM — it has near-zero enterprise .NET knowledge. We are
explicitly willing to trade away its Python/LeetCode ability to gain C#/.NET
competence.

### Measured baselines (Framework RTX 3060, Ollama 0.30.10, 2026-06-20)

| Metric | Score | Notes |
|--------|-------|-------|
| Throughput | 74.13 tok/s | 2.01 GB VRAM, 100% GPU, 2.86 s load |
| Quality | 2/11 | coding 2/5 (think-budget suppressed), tool/agentic 0 (untrained) |
| L3 (.NET, 50 tasks) | **0/50** | genuine domain gap — prose + broken C# |
| L2 raw (158 tasks) | 0/158 | artifact (chat model on raw prefix) |
| L2 chat (158 tasks) | **25/158 (15.8%)** | real C# baseline; mostly syntax build errors |

Every training iteration is measured against **L2 chat 25/158** and **L3 0/50**.

### Think-flag quirk (carried into training)

VibeThinker ignores `think:false` (always emits `<think>` tags) and returns
**empty content** on `think:true`. Do not use `CODING_BENCH_THINK=true`. The
L2/L3 extractors strip `<think>` tags automatically. Documented in
`MODEL_QUIRKS.md`.

## Architecture: two phases, three data streams

```
Stream A  The Stack (v1 dedup) C# filtered     ~50k   completion-style, code-only      Phase 1
Stream B  Curated .NET OSS repos               ~5–10k LLM-synthesised instruction pairs Phase 1 tail / Phase 2 warmup
Stream C  Synthetic L3-targeted                ~1–3k  think+code traces (existing gen)   Phase 2
```

- **Phase 1** trains on A + B (code-only assistant turns) → teaches C# syntax
  and idioms. Target: lift L2 chat.
- **Phase 2** continues from phase-1 weights on B + C (think+code traces) →
  injects enterprise .NET API patterns. Target: lift L3 off zero.
- **Hold-out:** 10% stratified per stream, withheld from training, used to flag
  overfitting between iterations.

## Data pipeline

### Stream A — The Stack v1 dedup (completion-style)

Use `bigcode/the-stack-dedup` (v1, C# subset) for the first pass — it has inline
file content and needs no Software Heritage S3 plumbing. Move to
`the-stack-v2-dedup` only if volume/recency demands it.

Filtering (in order):
1. License: permissive only (dataset license metadata).
2. Modern C#: file uses `namespace` / `record` / `async`; drop detectably
   pre-C# 8 patterns.
3. Size: 200 bytes – 8 KB (fits 4096-token window after function split).
4. Parse check: at least one `signature { body }` extractable (lightweight
   Roslyn or regex).
5. Near-duplicate dedup by normalized-content hash.

Conversion: signature + leading `/// <summary>` XML doc → user turn; body →
assistant turn. No frontier model needed. Target ~50k.

### Stream B — Curated .NET repos (LLM-synthesised)

Hand-pick 15–25 high-quality OSS .NET repos (ASP.NET Core samples, EF Core,
MassTransit, eShopOnWeb-style reference apps). Clone, extract files matching L3
categories, pass each to a cloud model (`glm-5:cloud` or `deepseek-v3.2:cloud`,
both already available) to synthesise a natural-language instruction whose answer
is the original code. Target ~5–10k.

### Stream C — Synthetic L3-targeted (existing generator)

Extend `scripts/lora/generate_training_data_scaled.py` to emit think+code traces.
Target ~1–3k. **Hard rule:** exclude the 50 L3 benchmark tasks and their
reference docs from generation prompts to prevent eval contamination.

## Training & evaluation loop

### Script: `scripts/lora/train_vibethinker_lora.py`

Adapted from `train_qwen35_lora.py`:
- `BASE_MODEL = "WeiboAI/VibeThinker-3B"` (Qwen2.5 arch — same 7 target modules
  already in the script: q/k/v/o_proj, gate/up/down_proj).
- 3B bf16 LoRA fits RTX 3060 12 GB (~6 GB weights + activations) — try bf16
  GPU-only first; keep the fp16-CPU-split path as fallback.
- `--phase {1,2}` selects dataset and whether assistant turns carry `<think>`.
- `--resume-from <phase1-adapter>` continues phase 2 from phase-1 weights.
- LoRA rank: start r=32 / alpha=64; raise to r=64 if underfitting.

### Iteration loop

```
1. Train phase-1 LoRA (Framework, 5k slice first)
2. Merge adapter → GGUF Q4_K_M → ollama create vibethinker-csharp-<iter>
3. Run L2 chat + L3 benchmarks (existing scripts, new slug)
4. Compare to baseline (25/158, 0/50) and previous iteration
5. Decision point:
   - L2 improving, L3 still 0  → expand phase-1 data, continue
   - L2 plateaus, L3 untouched → move to phase 2 (B+C, think+code)
   - Regression                → inspect hold-out, fix data filtering
```

### Naming

Iterations get slugs like `vibethinker-csharp-p1-5k`, `vibethinker-csharp-p1-50k`,
`vibethinker-csharp-p2`. The benchmark harness treats these as ordinary models;
results land in `results/coding-{slug}.json` etc., so coverage matrix and summary
charts work unchanged.

### Success gates

- **Phase 1 proof-of-life:** L2 chat 25 → ≥ 40/158 on the 5k slice (validates
  pipeline end-to-end).
- **Phase 1 full:** L2 chat ≥ 70/158 (competitive with small generalists).
- **Phase 2:** L3 0 → ≥ 5/50 (validates the approach warrants pushing further).

## Hardware orchestration (parallel tracks)

| Track | Machine | Work | Blocks on |
|-------|---------|------|-----------|
| 1 | Framework | Pipeline + 5k proof-of-life + iteration loop | nothing — starts now |
| 2 | T5500 | Stream + filter full Stack v1 C# → 50k dataset | confirm T5500 free (CPU/data-prep: announce & proceed) |
| 3 | Framework or T5500 | Full 50k phase-1 train once 5k proves out | Track 1 success + Track 2 dataset |

Per global instructions: T5500 data-prep (CPU/network-bound) is announced and
proceeds; the T5500 **GPU training** run requires explicit go-ahead (state
machine + device + duration) before launch.

## Risks & mitigations

- **Catastrophic forgetting of reasoning** — phase-1 code-only may erase the
  math/think edge. Mitigation: LoRA (not full fine-tune), low rank; base
  reasoning lives in frozen weights. Python/LeetCode loss is accepted.
- **GGUF conversion of Qwen2.5 LoRA** — mainline llama.cpp supports qwen2;
  validated at merge step; existing script has fallback paths if Unsloth export
  fails.
- **Eval contamination** — Stream C must exclude the 50 L3 tasks. Hard rule in
  the generator.
- **The Stack access friction** — if HF gating/token blocks it, fall back to
  `bigcode/the-stack-smol` or a language-filtered `codeparrot` C# subset for the
  proof-of-life slice.

## Out of scope (YAGNI)

- Full fine-tune (LoRA only).
- Multi-quant export (Q4_K_M only until a winner emerges).
- Automated hyperparameter search (manual decision points instead).
