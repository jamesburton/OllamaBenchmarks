#!/usr/bin/env python3
"""Sequential gap-fill queue: work through models missing benchmark stats.

Three tiers, run strictly in order (Strix's iGPU hosts one model at a time,
so inference is never parallelized -- only fetching is):

  1. Already-installed models with partial coverage (just missing L2/L3/etc).
  2. Already-installed models with zero coverage.
  3. Not-yet-installed models (HF/Ollama pulls) -- fetched one entry AHEAD of
     the currently-benching model, so download time overlaps with GPU time
     instead of stalling the queue. Each tier-3 entry is removed from
     benchmark-models.json's missing_from_local once its pipeline completes.

Waits for any currently-running pipeline (matched by a log file containing a
"pipeline finished" marker) before starting its own first model, so it can be
launched while another gap-fill/one-off benchmark is still in flight.

Disk-safety guard: before pulling a not-yet-installed model, checks free
space on the Ollama models drive. Models whose expected size is unknown and
large (flagged in HUGE_MODELS below) are skipped with a warning rather than
blindly pulled -- this box was at ~100GB free / 95% full when this queue was
authored, and this is a long unattended run.

Usage:
    python scripts/run_gap_fill_queue.py
    python scripts/run_gap_fill_queue.py --wait-for-log results/overnight-logs/bonsai-q1_0-pipeline.log
"""
import argparse
import json
import os
import re
import shutil
import subprocess
import sys
import time
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
RESULTS_DIR = REPO_ROOT / "results"
LOG_DIR = RESULTS_DIR / "overnight-logs"
BENCHMARK_MODELS_JSON = REPO_ROOT / "benchmark-models.json"
DATASET_PATH = "scripts/coding_tasks/datasets/data/humaneval-cs-reworded.json"

# Tier 0: explicit priority -- top-3 model by L3 with zero L2 data.
# qwen3-coder-next:latest moved to BLOCKED_BY_UMA 2026-07-20 -- see there.
TIER0 = []

# Tier 1: installed, partial coverage. (model, stages_to_run)
TIER1 = [
    "nemotron-3-nano:latest",
    "nemotron-3-nano:4b",
    "nemotron-nano-9b-v2-toolfix:latest",
    "lfm2.5-thinking:1.2b",
    "LFM2-2.6b-tools:latest",
    "phi4-mini:latest",
    "sam860/lfm2:2.6b",
    # Added 2026-07-22: already-accessible cloud model with partial coverage
    # (quality 10/11, L3 40/50 already recorded) found via a sweep of every
    # model with results/*.json data that predates the gap-fill queue.
    "minimax-m3:cloud",
]

# Tier 2: installed, zero coverage.
TIER2 = [
    "devstral:24b",
    "qwen36-bartowski:q4km",
    "qwen36-apex:balanced",
    "qwen3:4b-instruct",
    "qwen3:0.6b",
    "gemma4:e4b",
    # Added 2026-07-30: found via the CURRENT_QUALITY_MAX sweep (stale /5
    # quality runs, see that constant's definition below). These four are
    # already installed (confirmed via `ollama list`) but were never tracked
    # in any tier, so nothing was ever refreshing their quality score --
    # zero fetch cost. Placed in TIER2 for tracking, but also added to
    # KEEP_INSTALLED below per explicit user correction: the user already
    # wants this gemma4/glm-4.7-flash set kept on disk long-term, not
    # reclaimed once scored. A bare-name/no-tag duplicate of each ("gemma4",
    # "glm-4.7-flash") also appeared in the stale sweep but shares the same
    # model_slug() as the :latest-tagged entry here, so no separate line is
    # needed for those.
    "gemma4:latest",
    "gemma4:31b",
    "gemma4:e2b",
    "glm-4.7-flash:latest",
]

# 2026-07-25: disk kept hitting the 25GB safety margin because freshly-fetched
# Tier3 models (14-28GB each) accumulate faster than anything gets reclaimed.
# User approved generalizing the one-off qwen36-bartowski/apex reclaim into a
# blanket policy: every TIER2/TIER3 model gets `ollama rm`'d automatically
# once its full pipeline (quality/throughput/L3/L2raw/L2chat) is confirmed
# complete. TIER0/TIER1 are NOT auto-reclaimed -- those are the
# already-installed, partial-coverage models the user is actively tracking.
# KEEP_INSTALLED is the explicit exemption list for TIER2/TIER3 models the
# user wants to keep on disk after scoring (e.g. for real-task/LoRA use).
KEEP_INSTALLED = {
    # step-3.5-flash-reap-121b-8k:latest (Q5_K_M) was kept here 2026-07-25 to
    # survive the deferred L2 chat resume; that run finished (158/158 L2 chat,
    # 50/50 L3, 158/158 L2 raw) and the model was reclaimed 2026-07-27 to free
    # 85GB. A Q4_K_M re-run is planned (see the TIER3 exclusion note below and
    # models/step-3.5-flash-reap-121b-8k-q4km.Modelfile) -- not queued here
    # automatically since it needs a manual `ollama create` first.
    #
    # Added 2026-07-30 per explicit user correction: these were added to
    # TIER2 (auto-reclaim) as part of the CURRENT_QUALITY_MAX stale-quality
    # fix, but the user already wants this gemma4/glm-4.7-flash/gpt-oss set
    # kept on disk long-term (real-task use, not just benchmark scoring) --
    # they stay in TIER2 for tracking/reclaim-eligibility bookkeeping, but
    # this set exempts them from actually being deleted once scored. Only
    # genuinely new fetches (qwen3-coder:30b, qwen3.5:9b, the nvidia
    # Nemotron-Nano-9B-v2 and unsloth GLM-4.7-Flash-REAP-23B-A3B GGUFs) get
    # removed after retesting, per the same instruction.
    "gemma4:latest",
    "gemma4:31b",
    "gemma4:e2b",
    "glm-4.7-flash:latest",
    "gpt-oss:20b",
}

# Tier 3: not installed -- needs a pull. Ordered smallest/most-relevant first
# where size is known or guessable.
TIER3 = [
    "granite4:7b-a1b-h",
    "gemma3:4b",
    "zac/phi4-tools",
    "hf.co/Jackrong/Qwopus3.5-9B-v3-GGUF:Q8_0",
    "hf.co/empero-ai/Qwythos-9B-Claude-Mythos-5-1M-GGUF:Q4_K_M",
    "omnicoder:9b-q4_k_m",
    "gemma3:12b",
    "mistral-small",
    "mistral-small-4",
    "granite4:32b-a9b-h",
    "hf.co/RJ000/Mellum2-12B-A2.5B-Thinking-GGUF:Q4_K_M",
    "lfm2:24b",
    "gemma3:27b",
    "gemma4:26b-a4b-it-q8_0",
    "glm-4.7-flash:bf16",
    "nemotron-3-nano:30b-a3b-q8_0",
    "cogito:14b",
    "nemotron-cascade-2",
    "RogerBen/qwen3.5-35b-opus-distill",
    # Removed 2026-08-03: "qwen3.5:35b-a3b-q2_k_l" -- library/qwen3.5 publishes
    # no q2_k_l for 35b-a3b (only q4_K_M/q8_0/int4/int8/bf16/mxfp8/nvfp4), and
    # that exact quant is already covered by the bartowski entry below, which
    # has been fetched+scored+reclaimed. Pure duplicate; failed every pass.
    "hf.co/bottlecapai/ThinkingCap-Qwen3.6-27B-GGUF:Q4_K_M",
    # Removed 2026-08-03: "qwen3.5:27b-claude-4.6-opus-reasoning-distilled-q2_k"
    # -- no Q2_K build of this distill exists under any namespace (checked the
    # kwangsuklee / moophlo / juilpark / sinhang / zierocode / ImpurestClub
    # re-uploads). Dropped rather than substituted: swapping in a different
    # quant would silently change what the score means.
    # Repointed 2026-08-03: was "qwen3.5:27b-claude-4.6-opus-reasoning-distilled-q3_k_m",
    # which is unnamespaced and so resolved against library/qwen3.5, where these
    # community distills have never existed. These re-uploads also bake the quant
    # into the REPO NAME and publish only :latest, so a quant-suffixed tag can
    # never match. Exact Q3_K_M equivalent (14GB, 256K ctx):
    "yolo0perris/Qwen3.5-27B-Claude-4.6-Opus-Reasoning-Distilled-GGUF_Q3_K_M",
    "hf.co/Abiray/Nanbeige4.2-3B-GGUF:Q4_K_M",
    # Added 2026-07-22: sweep of every model referenced in results/*.json with
    # partial coverage that predates the gap-fill queue (found via the same
    # discover_archived_models() logic the dashboard uses, filtered manually).
    # Excluded from this sweep, with reasons, rather than silently dropped:
    #   - retired/gated Ollama Cloud tags: glm-5:cloud, deepseek-v3.2:cloud,
    #     cogito-2.1:671b-cloud, minimax-m2.7:cloud, glm-5.1:cloud (see their
    #     backend_notes -- 410 Gone / 403 subscription-required)
    #   - qwen3-coder-next (all tags) and nemotron-3-super: already in
    #     BLOCKED_BY_UMA below
    #   - bare "qwen3.5" tag: dropped 2026-07-20, superseded by qwen3.6
    #   - phi4-mini-16k/-strict variants: no saved Modelfile, not reproducible
    #   - vibethinker-3b-cs:50k-q4, vibethinker-csharp-p1-5k: local LoRA
    #     fine-tune outputs, not `ollama pull`-able -- need scripts/lora's
    #     merge/import flow instead, out of scope for this queue
    #   - step-3.5-flash-* (4 variants), glm-4.7-reap-218b-q3km: benchmarked
    #     via the T5500 llama-server/OpenAI-compatible API workflow (see
    #     scripts/prompt_configs/), not a plain `ollama pull` -- this queue
    #     can't reproduce that path
    #   - step-3.5-flash-reap-121b-8k-q4km: created via `ollama create` (see
    #     its entry at the very end of this list, mirroring where the Q5_K_M
    #     predecessor ended up) rather than excluded -- already installed, so
    #     start_pull()'s is_installed() check skips the doomed `ollama pull`
    #     attempt for it cleanly.
    #   - case/tag duplicates: kept one canonical spelling each for
    #     trinity-mini, shenwen-coderV2, qwen3.6-unsloth-iq2_m
    #   - gemma4:31b-cloud, gemma4-31b-iq2xxs, gemma4-12b-ud-q4kxl,
    #     glm-4.7-flash-reap-toolfix: unclear/unverified provenance (no
    #     Modelfile, uncertain registry availability) -- skipped rather than
    #     risk a silent bad entry; revisit if specifically wanted
    #
    # Added 2026-07-30: CURRENT_QUALITY_MAX sweep (see that constant's
    # definition below) turned up 73 models with stale /5 quality runs.
    # 49 already had a tracking entry somewhere (TIER0-3/KEEP_INSTALLED/
    # BLOCKED_BY_UMA) and just needed the coverage() fix to get picked up
    # again automatically. Of the rest: 4 were bare-name/no-tag duplicates
    # of an already-tracked :latest/:4b-style entry (same model_slug(), no
    # separate line needed) or a differently-cased duplicate tag
    # ("sam860/LFM2:2.6b" vs the already-tracked "sam860/lfm2:2.6b" --
    # Ollama registry names are case-insensitive, so this is the identical
    # remote content); the previously-documented exclusions above (cloud
    # tags, phi4-mini-16k/-strict, qwen3.5 bare, glm-4.7-flash-reap-toolfix)
    # covered another ~9; 4 were already-installed zero-fetch-cost adds (see
    # TIER2 above: gemma4:latest/31b/e2b, glm-4.7-flash:latest). The
    # remaining genuinely-new, safely-sized fetch targets are added below.
    # "gpt-oss:120b" was found too (~65GB) but deliberately left out --
    # right at the same ~67.4GB host-committed-bytes ceiling flagged for
    # Laguna-S-2.1/Mistral-Medium-3.5 above; revisit with a size-safe quant
    # if wanted.
    "qwen3-coder:30b",
    "qwen3.5:9b",
    "hf.co/bartowski/nvidia_NVIDIA-Nemotron-Nano-9B-v2-GGUF:Q4_K_M",
    "hf.co/unsloth/GLM-4.7-Flash-REAP-23B-A3B-GGUF:Q4_K_M",
    "MichelRosselli/GLM-4.5-Air",
    "cogito:8b",
    "deepcoder:1.5b",
    "devstral-small-2:24b-instruct-2512-q4_K_M",
    "devstral-small-2:24b-instruct-2512-q8_0",
    "gemma3-12b-tools",
    "gemma3n:e4b",
    "gemma4:12b",
    "gpt-oss:20b",
    "hf.co/bartowski/Qwen_Qwen3.5-35B-A3B-GGUF:Q2_K_L",
    # Replaced 2026-08-06: was "hf.co/bartowski/Tesslate_OmniCoder-9B-GGUF:Q4_K_M".
    # That tag is valid and the file is real, but `ollama pull` fails on it
    # reproducibly with "Error: context deadline exceeded" AFTER downloading the
    # blob to 100% -- seen at 4.6GB, 90GB and 80GB free, so it is an
    # ollama-client timeout finalising large HF pulls, not disk pressure.
    # `hf download` fetches the identical blob without trouble, so the GGUF is
    # now imported locally via gguf-cache/Modelfile.tesslate-omnicoder-9b-q4km
    # (the .gguf there is a hardlink to the HF hub cache blob, one copy on disk).
    # NB this is a THINKING model: it emits into message.thinking and leaves
    # message.content empty until reasoning finishes, so a low score here should
    # be checked against MODEL_QUIRKS.md before being believed.
    "tesslate-omnicoder-9b-q4km:latest",
    "hf.co/bartowski/cerebras_GLM-4.5-Air-REAP-82B-A12B-GGUF:IQ4_XS",
    "hf.co/bartowski/kai-os_Carnice-V2-27b-GGUF:Q4_K_M",
    "hf.co/grapeV-ai/gemma-4-26B-A4B-it-gguf:Q4_K_M",
    # Added 2026-07-29 per explicit user request. Only one GGUF quant
    # published for this repo (checked via the HF API), so no quant choice
    # to make.
    "hf.co/KyleHessling1/Qwopus3.6-27B-Fusion-GGUF:Q5_K_M",
    "hf.co/mradermacher/shenwen-coderV2-Instruct-GGUF:Q8_0",
    "hf.co/protoLabsAI/Ornith-1.0-9B-MTP-GGUF:ornith-9b-mtp-kl-Q6_K.gguf",
    "hf.co/protoLabsAI/Ornith-1.0-9B-MTP-GGUF:ornith-9b-mtp-kl-Q8_0.gguf",
    # Added 2026-07-30 per explicit user request. poolside/Laguna-S-2.1 is a
    # brand-new (2026-07-13) 256-expert/10-active MoE, architecture "laguna"
    # (LagunaForCausalLM) -- support in Ollama's bundled llama.cpp is
    # unverified, may fail like the documented nanbeige "unknown model
    # architecture" case. UD-IQ4_XS (57.6GB) chosen over Q4_K_M (73.1GB) to
    # stay under the ~67.4GB host-committed-bytes ceiling documented in
    # PLATFORM_QUIRKS.md (the same ceiling that blocks Mistral-Medium-3.5-128B
    # Q4_K_M at 74.9GB) -- see user confirmation in session history.
    "hf.co/unsloth/Laguna-S-2.1-GGUF:UD-IQ4_XS",
    # Added 2026-07-30 per explicit user request: a higher-quality comparison
    # point against the already-benchmarked mistral-medium-3.5:iq3m (IQ3_M,
    # 56GB, see .docs/handoff.md and MODEL_QUIRKS.md "mistral-medium-3.5:iq3m"
    # entries). UD-Q3_K_XL (62.5GB) chosen over IQ4_XS (67.1GB, right at the
    # ~67.4GB host-committed-bytes ceiling from PLATFORM_QUIRKS.md -- the same
    # ceiling that blocks Q4_K_M at 74.9GB on the do-not-pull list) and over
    # plain Q3_K_M (60.6GB, lower quality for barely less size).
    "hf.co/unsloth/Mistral-Medium-3.5-128B-GGUF:UD-Q3_K_XL",
    "hf.co/xhxlb/IQuest-Coder-V1-14B-Instruct-GGUF:Q4_K_M",
    "hf.co/xhxlb/IQuest-Coder-V1-7B-Instruct-GGUF:Q4_K_M",
    "ingu627/exaone4.0:32b",
    # Fixed 2026-08-01: was bare "jacob-ebey/phi4-tools" -- `ollama list`
    # shows it installed as "jacob-ebey/phi4-tools:latest" and is_installed()
    # does an exact match, so the bare name was silently treated as "not
    # installed" for 2 days (9.1GB sitting on disk, fully paid for, never
    # benched) while the disk-margin check kept masking the underlying bug
    # by blocking the redundant pull attempt first. Same bug class as the
    # step-3.5-flash-reap-121b-8k fix from 2026-07-25 -- see that history
    # above. Audited the rest of TIER3 for the same pattern, this was the
    # only live instance.
    "jacob-ebey/phi4-tools:latest",
    "lfm2.5-thinking",
    "llama4:16x17b",
    "minicpm-v:8b",
    "ministral-3:14b",
    "qwen2.5vl:7b",
    # Added 2026-08-04: Qwen's agentic/world-model release (June 2026), a
    # finetune of Qwen3.5-35B-A3B-Base. unsloth's is the canonical GGUF (534K
    # downloads); UD-Q4_K_M is 22.1GB, comfortably inside this box's budget and
    # consistent with the other unsloth UD- entries here. Of direct interest to
    # the planned Layer 4 agentic/tool-use suite
    # (docs/agentic-tool-longcontext-benchmark-plan.md).
    "hf.co/unsloth/Qwen-AgentWorld-35B-A3B-GGUF:UD-Q4_K_M",
    "qwen3.5:122b",
    "qwen3.5:122b-a10b",
    # Removed 2026-08-03: "qwen3.5:27b-claude-4.6-opus-reasoning-distilled-v2-q4_k_m"
    # -- same unnamespaced-tag problem as the q2_k/q3_k_m entries above. No exact
    # v2 Q4_K_M re-upload exists; the only v2 build found is
    # ImpurestClub/...-v2-q3km (Q3, not Q4), so there is nothing to repoint to.
    "qwen3.5:35b-a3b",
    "qwen3.5:4b",
    "qwen3.6-unsloth-iq2_m",
    "qwen3.6:35b-a3b-q4_K_M",
    "qwen3:14b",
    "qwen3:8b",
    "richardyoung/qwythos-9b-abliterated:Q4_K_M",
    "rnj-1:8b",
    "trinity-mini:q4_k_m",
    # Moved to the very end 2026-07-25 per user request: L2 raw done (70/158),
    # L2 chat interrupted mid-run at 59/158 (checkpoint preserved) because this
    # 85GB model runs ~5-7min/task and was starving the rest of the queue of
    # progress. Model is kept installed (not `ollama rm`'d, see KEEP_INSTALLED
    # above) -- only run bench_model()'s remaining l2_chat stage last, after
    # everything else has a chance to complete. num_ctx capped to 8192 via
    # models/step-3.5-flash-reap-121b-8k.Modelfile (see 2026-07-25 note above).
    # NOTE: must carry the ":latest" tag -- `ollama list` shows it installed as
    # "step-3.5-flash-reap-121b-8k:latest" and is_installed() does an exact
    # match, so the bare name silently skipped it entirely on 2026-07-25
    # (fetch was also blocked by the disk-headroom margin, compounding it).
    # Removed 2026-07-27: run completed and model was reclaimed (85GB freed).
    #
    # Q4_K_M follow-up (step-3.5-flash-reap-121b-8k-q4km) created 2026-07-27,
    # same REAP-121B family via models/step-3.5-flash-reap-121b-8k-q4km.Modelfile.
    # First placed at the end of TIER2 (2026-07-27) -- wrong: TIER2 runs to
    # completion *before* TIER3 even starts, so a slow model there blocks
    # every remaining TIER3 model, not just the ones after it. Confirmed
    # 2026-07-29: quality/throughput/L3(25/50)/L2raw(60/158) finished
    # overnight, but L2 chat alone ran ~12 hours for 90/158 tasks (thinking
    # model, long traces) with all 30 remaining TIER3 models sitting
    # completely idle that whole time. Killed the run (checkpointed progress
    # preserved -- both L2 harnesses write coding-{slug}[-chat].json after
    # every task) and moved it here instead, mirroring where the Q5_K_M
    # predecessor ended up. Also fixed start_pull() the same day to skip the
    # `ollama pull` attempt entirely for already-installed models, so this
    # (or any future custom Modelfile model) is safe to place anywhere in
    # TIER3, not just relying on the disk-margin check happening to skip the
    # doomed pull attempt first.
    "step-3.5-flash-reap-121b-8k-q4km:latest",
]
# Approximate on-disk sizes, used ONLY to order TIER3 smallest-first (below).
# Sizes marked "verified" were read from the Hugging Face API on 2026-08-04
# (summing split-GGUF parts); the rest are parameter-count x quant estimates.
# They do not need to be exact -- they only need to sort large from small.
SIZE_HINT_GB = {
    # verified
    "hf.co/unsloth/Mistral-Medium-3.5-128B-GGUF:UD-Q3_K_XL": 62.5,
    "hf.co/unsloth/Laguna-S-2.1-GGUF:UD-IQ4_XS": 57.6,
    "hf.co/unsloth/Qwen-AgentWorld-35B-A3B-GGUF:UD-Q4_K_M": 22.1,
    "tesslate-omnicoder-9b-q4km:latest": 5.9,  # locally imported, never pulled
    # estimated
    "qwen3.5:122b": 70.0,
    "qwen3.5:122b-a10b": 70.0,
    "llama4:16x17b": 67.0,
    "hf.co/bartowski/cerebras_GLM-4.5-Air-REAP-82B-A12B-GGUF:IQ4_XS": 44.0,
    "MichelRosselli/GLM-4.5-Air": 40.0,
    "qwen3.6:35b-a3b-q4_K_M": 22.0,
    "qwen3.5:35b-a3b": 22.0,
    "qwen3.6-unsloth-iq2_m": 13.0,
    "hf.co/bartowski/kai-os_Carnice-V2-27b-GGUF:Q4_K_M": 16.5,
    "yolo0perris/Qwen3.5-27B-Claude-4.6-Opus-Reasoning-Distilled-GGUF_Q3_K_M": 14.0,
    "ingu627/exaone4.0:32b": 19.0,
    "mistral-small-4": 14.0,
    "ministral-3:14b": 9.0,
    "qwen3:14b": 9.0,
}
DEFAULT_SIZE_HINT_GB = 10.0

# Kept at the very end of TIER3 regardless of size -- see the long note on the
# step-3.5 entry above: it is a slow thinking model whose L2-chat stage alone
# ran ~12 hours, and it must not block the rest of the queue.
PINNED_LAST = {"step-3.5-flash-reap-121b-8k-q4km:latest"}


def order_by_size(models):
    """Order TIER3 smallest-first, keeping PINNED_LAST entries at the end.

    Added 2026-08-04. The queue kept ending passes early on the disk-headroom
    guard: a single very large model (llama4:16x17b at ~67GB, then 62.5GB
    Mistral-Medium and 57.6GB Laguna still pending) drives free space under
    MIN_FREE_GB_TO_PULL, and because a skipped model is never benched it is
    never reclaimed either -- so every remaining model in the pass is skipped
    too. Two such cascades in two days each stranded ~16 models and idled the
    box for hours. Running small models first means the big ones can only
    strand the tail of the queue rather than the middle, and by the time they
    run everything cheap is already scored.

    Stable sort: models with equal (or defaulted) hints keep their existing
    relative order, so this does not reshuffle the hand-curated list.
    """
    pinned = [m for m in models if m in PINNED_LAST]
    rest = [m for m in models if m not in PINNED_LAST]
    rest.sort(key=lambda m: SIZE_HINT_GB.get(m, DEFAULT_SIZE_HINT_GB))
    return rest + pinned


TIER3 = order_by_size(TIER3)

# qwen3.5 (bare tag) dropped 2026-07-20 -- doesn't resolve via Ollama library,
# and superseded by the qwen3.6 generation already covered elsewhere in this
# suite (qwen3.6:latest, qwen3.6:27b). Not pursuing a re-fetch.

# Blocked 2026-07-20 by the "Ollama UMA bug" (PLATFORM_QUIRKS.md): BIOS UMA
# frame buffer is currently set high (~96GB), leaving Windows only ~31.6GB
# visible RAM and a ~67GB commit ceiling regardless of OLLAMA_GPU_MEMORY.
# qwen3-coder-next:latest (49GB ROCm buffer) confirmed failing under this
# config -- repeated retries drove memory pressure severe enough to kill
# unrelated background processes on the box. Anything here needs the BIOS
# UMA frame buffer reduced to 16-32GB + a full power-cycle before retrying;
# do NOT re-enable without confirming that fix landed (re-check
# TotalVisibleMemorySize > 90GB first).
BLOCKED_BY_UMA = [
    ("qwen3-coder-next:latest", "49GB ROCm buffer; confirmed failing 2026-07-20, was Tier 0"),
    ("qwen3-coder-next:q8_0", "even larger than the already-failing :latest tag"),
    ("nemotron-3-super", "86GB; PLATFORM_QUIRKS.md notes this needs the good (16-32GB) BIOS split"),
    ("hf.co/Abiray/Mistral-Medium-3.5-128B-Q4_K_M-GGUF:Q4_K_M", "confirmed 74.9GB, well over the ~67GB commit ceiling"),
]

# Models we intend to bench that DO NOT EXIST YET. Deliberately kept out of
# TIER3: an unreleased tag in the live list burns a failed `ollama pull` on
# every pass, which is exactly the dead-tag problem cleaned up on 2026-08-03.
# These are logged at startup as a visible reminder and never fetched. Move an
# entry into TIER3 once its availability has actually been confirmed (check the
# Ollama library page and Hugging Face for a GGUF -- the Qwen org ships
# safetensors first, and Ollama needs a GGUF re-upload such as unsloth's).
AWAITING_RELEASE = [
    ("qwen3.8:27b",
     "expected week commencing 2026-08-10; as of 2026-08-04 ollama.com/library/"
     "qwen3.8 is 404 and Qwen's HF org has no 3.8 -- their latest is Qwen3.6 "
     "(27B / 35B-A3B, Apr 2026). NB the only current HF hit for 'Qwen3.8' is "
     "Ma7ee7/Qwen3.8_4B_Distilled_GGUF, which is an unrelated Qwen3-4B-Thinking"
     "-2507 repack, NOT this model -- do not substitute it"),
]

MIN_FREE_GB_TO_PULL = 30.0  # abort further tier-3 pulls below this headroom
# Lowered from 25.0 -> 12.0 on 2026-07-27, then raised 12.0 -> 30.0 later the
# same day after it caused two real disk-to-0.0GB incidents. The margin only
# gates whether a pull *starts* -- it does nothing to stop a download in
# progress, and the fetch-ahead loop lets the NEXT model's pull run
# concurrently with the CURRENT model's (possibly hours-long) bench. Both
# incidents were the same root cause: a large model (28GB gemma4 q8_0; then
# an 82B REAP IQ4_XS overlapping a 27B Q4_K_M) blew straight through whatever
# margin was left at pull-start, because nothing checks remaining disk again
# once the download is running. 30GB is not a guarantee against a repeat --
# it's sized to survive one small-to-medium overlap, not two large ones at
# once. If this happens a third time, the real fix is checking free disk
# periodically *during* a pull (or capping to one in-flight pull for models
# above some size threshold), not just raising the number further.

# `ollama rm` returns long before the OS has actually released the model's
# blobs (llama-server can still have them mapped), so free space read
# immediately after a reclaim under-reports -- sometimes by tens of GB.
# Combined with the fetch-ahead ordering (model i+1's pull is started BEFORE
# model i is benched and reclaimed), one large model near the end of TIER3
# could pin the reading below the margin and false-skip every model after it:
# nothing benches, so nothing reclaims, so space never recovers within the
# pass -- a self-reinforcing cascade. Observed 2026-08-03: a pass ended at
# 12:51 having skipped ~40 TIER3 models at a reported 21.7GB free, while the
# same drive measured 45.8GB an hour later with nothing else having run.
# Fix: before honouring a headroom skip, wait (bounded) for a pending reclaim
# to settle and re-read. See headroom_ok() below.
RECLAIM_SETTLE_S = 90.0  # max wait for a just-issued `ollama rm` to free space
RECLAIM_POLL_S = 5.0

# Multi-pass loop (see main()). A pass that dies on the disk guard leaves work
# undone; disk usually recovers within minutes once the pass stops holding
# models open, so waiting and re-running clears it without human intervention.
MAX_PASSES = 20
NO_PROGRESS_LIMIT = 2   # consecutive zero-bench passes before giving up
PASS_COOLDOWN_S = 600   # 10 min, generous enough for lazy blob release to land

# Size-aware pull guard (see required_free_gb). PULL_BUFFER_GB covers the
# resident model plus the fetch-ahead overlap on top of the incoming model's
# own footprint; ABSOLUTE_MIN_FREE_GB is the floor that protects the box no
# matter how small the download -- running this drive to 0 has previously
# driven memory pressure hard enough to kill unrelated processes.
PULL_BUFFER_GB = 8.0
ABSOLUTE_MIN_FREE_GB = 12.0


def model_slug(model: str) -> str:
    model = re.sub(r":latest$", "", model)
    return re.sub(r"[^\w\.-]", "_", model.replace(":", "_").replace("/", "_").replace("\\", "_"))


def free_gb(path: Path) -> float:
    total, used, free = shutil.disk_usage(str(path.anchor or "C:\\"))
    return free / (1024 ** 3)


_LIST_ORPHAN_PULLS_PS = r"""
$pulls = @(Get-CimInstance Win32_Process -Filter "Name='ollama.exe'" |
           Where-Object { $_.CommandLine -like '*pull*' })
$out = foreach ($p in $pulls) {
  $alive = $null -ne (Get-Process -Id $p.ParentProcessId -ErrorAction SilentlyContinue)
  [pscustomobject]@{ pid = $p.ProcessId; ppid = $p.ParentProcessId
                     parentAlive = $alive; cmd = $p.CommandLine }
}
@($out) | ConvertTo-Json -Compress
"""


def ollama_blob_dir() -> Path:
    root = os.environ.get("OLLAMA_MODELS")
    if root:
        return Path(root) / "blobs"
    return Path.home() / ".ollama" / "models" / "blobs"


def cleanup_orphaned_pulls(log):
    """Kill `ollama pull` processes orphaned by a previous queue run, then drop
    the partial blobs they were writing.

    The fetch-ahead design starts model i+1's pull as a child of the queue. If
    the queue dies or is killed between models, that child keeps downloading
    with no parent -- observed 2026-08-03, when a 17.5GB partial was still
    growing hours after its parent had exited, and a restarted queue would have
    raced it on the same blob.

    Deliberately conservative: only pulls whose PARENT IS GONE are killed, so a
    concurrently-running queue or a manual `ollama pull` is left alone. Partial
    blobs are only removed once no pull process of any kind remains, because a
    live pull's partials are its working state -- deleting those would corrupt
    a download this queue does not own.
    """
    if os.name != "nt":
        return  # process inspection here is Windows-specific (this box is Strix/Windows)
    try:
        res = subprocess.run(
            ["powershell", "-NoProfile", "-NonInteractive", "-Command",
             _LIST_ORPHAN_PULLS_PS],
            capture_output=True, text=True, timeout=60)
        entries = json.loads(res.stdout.strip() or "[]")
    except (OSError, subprocess.SubprocessError, json.JSONDecodeError, ValueError) as exc:
        log(f"  [startup] could not inspect ollama pull processes ({exc}); skipping sweep")
        return
    if isinstance(entries, dict):  # ConvertTo-Json unwraps a single object
        entries = [entries]

    orphans = [e for e in entries if not e.get("parentAlive")]
    live = [e for e in entries if e.get("parentAlive")]
    for e in orphans:
        cmd = (e.get("cmd") or "").strip()
        log(f"  [startup] killing orphaned pull (pid {e['pid']}, dead parent "
            f"{e['ppid']}): {cmd}")
        subprocess.run(["taskkill", "/PID", str(e["pid"]), "/F"],
                       stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    if live:
        log(f"  [startup] leaving {len(live)} pull(s) with a live parent alone; "
            f"not touching partial blobs")
        return
    if not orphans:
        return

    blob_dir = ollama_blob_dir()
    freed = 0
    removed = 0
    for blob in blob_dir.glob("*-partial*"):
        try:
            size = blob.stat().st_size
            blob.unlink()
        except OSError as exc:
            log(f"  [startup] could not remove {blob.name}: {exc}")
            continue
        freed += size
        removed += 1
    if removed:
        log(f"  [startup] removed {removed} orphaned partial blob(s), "
            f"~{freed / (1024 ** 3):.1f}GB")


def required_free_gb(model):
    """Free space needed before pulling `model`.

    The old flat MIN_FREE_GB_TO_PULL blocked a 5.9GB pull for exactly the same
    reason as a 62GB one. On 2026-08-05 the box settled at 28.9GB free -- 1.1GB
    under the flat margin -- and every remaining model was skipped pass after
    pass, including 20 of the 22 that would have fit comfortably. Size the
    requirement to the model instead: its own footprint plus a buffer that
    covers the resident model and the fetch-ahead overlap, never below a hard
    floor that protects the box itself.
    """
    size = SIZE_HINT_GB.get(model, DEFAULT_SIZE_HINT_GB)
    return max(ABSOLUTE_MIN_FREE_GB, size + PULL_BUFFER_GB)


def headroom_ok(log, state, model=None, read_free=None, settle_s=RECLAIM_SETTLE_S,
                poll_s=RECLAIM_POLL_S, sleep=time.sleep):
    """Free space vs what `model` actually needs, tolerant of a lagging `ollama rm`.

    `state` is a dict with a "settle_exhausted" flag, cleared by the caller
    whenever a reclaim actually removes a model. Returns
    (ok, free_gb_at_decision) so the caller logs the same figure it decided on
    -- the previous code called free_gb() twice, so the logged number could
    differ from the one actually tested.

    `model` selects a size-aware requirement (see required_free_gb); omitting it
    falls back to the old flat margin.

    The injectable read_free/sleep are what make this testable without a real
    disk or a real 90s wait.
    """
    need = required_free_gb(model) if model is not None else MIN_FREE_GB_TO_PULL
    read_free = read_free or (lambda: free_gb(REPO_ROOT))
    free = read_free()
    if free >= need:
        return True, free
    if state.get("settle_exhausted"):
        return False, free
    log(f"  [disk] {free:.1f}GB free, below the {need:.1f}GB needed -- waiting up to "
        f"{settle_s:.0f}s for a pending reclaim to release blobs")
    waited = 0.0
    while waited < settle_s:
        sleep(poll_s)
        waited += poll_s
        free = read_free()
        if free >= need:
            log(f"  [disk] headroom recovered to {free:.1f}GB after {waited:.0f}s")
            return True, free
    state["settle_exhausted"] = True
    log(f"  [disk] no recovery after {settle_s:.0f}s ({free:.1f}GB free, need "
        f"{need:.1f}GB); skipping further settle-waits until the next reclaim")
    return False, free


def read_json(path: Path):
    if path.exists():
        try:
            return json.loads(path.read_text(encoding="utf-8"))
        except Exception:
            return {}
    return {}


# score_max of the current quality suite (coding_total + tool_total +
# agentic_total questions). The suite expanded from 5 -> 11 questions
# sometime before 2026-04-06; every run since then consistently scores /11
# (verified across 85 result files 2026-07-30). coverage() previously only
# checked bool(results), so it silently treated 101 old /5 runs across 73
# models as "complete" and never re-ran them. Hardcoded here rather than
# read from a live benchmark_quality.py run, matching the existing pattern
# for L2/L3's hardcoded 158/50 expected-task-count thresholds below.
CURRENT_QUALITY_MAX = 11


def coverage(model: str) -> dict:
    slug = model_slug(model)
    quality_results = read_json(RESULTS_DIR / f"quality-{slug}.json").get("results") or []
    quality = bool(quality_results) and quality_results[0].get("score_max") == CURRENT_QUALITY_MAX
    coding = read_json(RESULTS_DIR / f"coding-{slug}.json")
    l3_done = len(coding.get("layer3_results") or []) >= 50
    l2_raw_done = len(coding.get("layer2_results") or []) >= 158
    chat = read_json(RESULTS_DIR / f"coding-{slug}-chat.json")
    l2_chat_done = len(chat.get("layer2_chat_results") or []) >= 158
    throughput_files = list(RESULTS_DIR.glob(f"throughput-resource-{slug}.json"))
    throughput_done = bool(throughput_files)
    return {
        "quality": quality,
        "throughput": throughput_done,
        "l3": l3_done,
        "l2_raw": l2_raw_done,
        "l2_chat": l2_chat_done,
    }


def run(cmd, log_path: Path, cwd=REPO_ROOT):
    print(f"  $ {' '.join(cmd)}", flush=True)
    with open(log_path, "a", encoding="utf-8") as log:
        log.write(f"\n=== {' '.join(cmd)} ===\n")
        log.flush()
        proc = subprocess.run(cmd, cwd=cwd, stdout=log, stderr=subprocess.STDOUT, text=True)
    return proc.returncode


def is_installed(model: str) -> bool:
    try:
        out = subprocess.run(["ollama", "list"], capture_output=True, text=True, timeout=30)
    except Exception:
        return False
    # Exact match on the NAME column only -- a substring/base-name check here
    # false-positives whenever another tag of the same model family is present
    # (e.g. "gemma3:12b" incorrectly reads as installed because "gemma3:4b" is),
    # which lets bench_model() run generation against a tag that was never
    # pulled and silently records a bogus 0/158 (caught 2026-07-22 via gemma3:12b).
    names = {line.split()[0] for line in out.stdout.splitlines()[1:] if line.strip()}
    return model in names


def bench_model(model: str, log_path: Path):
    cov = coverage(model)
    env = os.environ.copy()
    env["OLLAMA_HOST"] = env.get("OLLAMA_HOST") or "http://127.0.0.1:11434"
    if not re.match(r"^https?://", env["OLLAMA_HOST"]):
        env["OLLAMA_HOST"] = "http://127.0.0.1:11434"

    if not cov["quality"]:
        subprocess.run([sys.executable, "scripts/benchmark_quality.py", "--models", model],
                        cwd=REPO_ROOT, env=env, stdout=open(log_path, "a", encoding="utf-8"),
                        stderr=subprocess.STDOUT)
    if not cov["throughput"]:
        subprocess.run(["powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File",
                         "scripts/benchmark_throughput_resource.ps1", "-Models", model],
                        cwd=REPO_ROOT, env=env, stdout=open(log_path, "a", encoding="utf-8"),
                        stderr=subprocess.STDOUT)
    if not cov["l3"]:
        subprocess.run([sys.executable, "scripts/benchmark_coding_layer3.py", "--models", model],
                        cwd=REPO_ROOT, env=env, stdout=open(log_path, "a", encoding="utf-8"),
                        stderr=subprocess.STDOUT)
    if not cov["l2_raw"]:
        subprocess.run([sys.executable, "scripts/benchmark_coding_layer2.py", "--models", model,
                         "--dataset-path", DATASET_PATH],
                        cwd=REPO_ROOT, env=env, stdout=open(log_path, "a", encoding="utf-8"),
                        stderr=subprocess.STDOUT)
    if not cov["l2_chat"]:
        subprocess.run([sys.executable, "scripts/benchmark_coding_layer2_chat.py", "--models", model,
                         "--dataset-path", DATASET_PATH],
                        cwd=REPO_ROOT, env=env, stdout=open(log_path, "a", encoding="utf-8"),
                        stderr=subprocess.STDOUT)


def mark_fetched_in_tracking(model: str):
    if not BENCHMARK_MODELS_JSON.exists():
        return
    d = json.loads(BENCHMARK_MODELS_JSON.read_text(encoding="utf-8"))
    if model in d.get("missing_from_local", []):
        d["missing_from_local"].remove(model)
        BENCHMARK_MODELS_JSON.write_text(json.dumps(d, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def wait_for_log_marker(log_path: str, marker: str = "pipeline finished"):
    p = Path(log_path)
    print(f"[queue] Waiting for '{marker}' in {p} before starting (GPU is single-tenant)...", flush=True)
    while True:
        if p.exists() and marker in p.read_text(encoding="utf-8", errors="ignore"):
            print(f"[queue] {p} finished, proceeding.", flush=True)
            return
        time.sleep(30)


def run_pass(first_pass=True):
    """Run one full sweep of all tiers. Returns the number of models benched."""
    completed = 0
    parser = argparse.ArgumentParser()
    parser.add_argument("--wait-for-log", default=None,
                         help="Poll this log file for 'pipeline finished' before starting (avoids GPU contention).")
    args = parser.parse_args()

    LOG_DIR.mkdir(parents=True, exist_ok=True)
    master_log = LOG_DIR / "gap-fill-queue.log"

    def log(msg):
        line = f"{time.strftime('%H:%M:%S')} {msg}"
        print(line, flush=True)
        with open(master_log, "a", encoding="utf-8") as f:
            f.write(line + "\n")

    # Only honour --wait-for-log on the first pass: it exists to avoid GPU
    # contention with another pipeline at launch, and re-waiting on a stale
    # marker between passes would stall the loop forever.
    if args.wait_for_log and first_pass:
        wait_for_log_marker(args.wait_for_log)

    log("=== Gap-fill queue starting ===")
    log(f"Tier 0 (priority): {len(TIER0)} models")
    log(f"Tier 1 (partial, installed): {len(TIER1)} models")
    log(f"Tier 2 (zero coverage, installed): {len(TIER2)} models")
    log(f"Tier 3 (needs fetch): {len(TIER3)} models")
    for name, reason in BLOCKED_BY_UMA:
        log(f"  [BLOCKED] {name} -- {reason} (Ollama UMA bug, needs BIOS fix + power-cycle, see PLATFORM_QUIRKS.md)")
    for name, note in AWAITING_RELEASE:
        log(f"  [AWAITING RELEASE] {name} -- {note}")

    cleanup_orphaned_pulls(log)

    # Set when a settle-wait has already timed out without recovering headroom,
    # and cleared whenever a reclaim actually removes something. Without it, a
    # genuine out-of-space pass would burn RECLAIM_SETTLE_S on every remaining
    # TIER3 entry (~40 x 90s) waiting for space that nothing is going to free.
    disk_state = {"settle_exhausted": False}

    def reclaim_if_complete(model):
        if model in KEEP_INSTALLED:
            return
        cov = coverage(model)
        if all(cov.values()):
            log(f"  [reclaim] scores complete, removing from disk: ollama rm {model}")
            subprocess.run(["ollama", "rm", model], cwd=REPO_ROOT,
                            stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            # Space may not be released yet -- headroom_ok() waits for it. Just
            # re-arm the settle-wait so the next pull decision is allowed to.
            disk_state["settle_exhausted"] = False
        else:
            log(f"  [reclaim] SKIPPED removal for {model} -- coverage incomplete {cov}, "
                f"leaving on disk for a future retry")

    # `completed` must count only models that actually still owed work.
    # Counting re-verification of already-complete TIER0/1/2 models made every
    # pass report ~18 "benched" even when nothing advanced, which silently
    # disabled main()'s NO_PROGRESS_LIMIT guard -- the loop would have burned
    # all 20 passes on a stuck disk instead of stopping for a human after 2.
    # (Found 2026-08-05: 3 passes, "18 benched" each, pending stuck at 22.)
    def bench_and_count(model, model_log):
        nonlocal completed
        was_pending = not all(coverage(model).values())
        bench_model(model, model_log)
        if was_pending:
            completed += 1

    # --- Tier 0 & 1: already installed, actively tracked -- never auto-reclaimed ---
    for model in TIER0 + TIER1:
        slug = model_slug(model)
        model_log = LOG_DIR / f"gap-fill-{slug}.log"
        log(f"--- {model} (installed) ---")
        bench_and_count(model, model_log)
        log(f"  done: {model} (log: {model_log})")

    # --- Tier 2: already installed, zero coverage -- auto-reclaimed once scored ---
    for model in TIER2:
        slug = model_slug(model)
        model_log = LOG_DIR / f"gap-fill-{slug}.log"
        log(f"--- {model} (installed) ---")
        bench_and_count(model, model_log)
        log(f"  done: {model} (log: {model_log})")
        reclaim_if_complete(model)

    # --- Tier 3: fetch-ahead, bench-behind ---
    pull_procs = {}
    # model -> (log path, open handle) for the in-flight pull. Before 2026-08-03
    # pull output went to DEVNULL, so every failure logged an identical
    # "[FAIL] ... (exit 1)" with no way to tell a nonexistent tag from a gated
    # repo from a transient network drop -- four such failures had to be
    # researched by hand against the Ollama and HF APIs to be told apart.
    pull_logs = {}

    def finish_pull_log(model):
        """Close a pull's log handle and return a one-line failure summary."""
        entry = pull_logs.pop(model, None)
        if entry is None:
            return ""
        path, handle = entry
        try:
            handle.close()
        except OSError:
            pass
        try:
            text = path.read_text(encoding="utf-8", errors="replace")
        except OSError:
            return ""
        # `ollama pull` redraws progress using ANSI cursor control, NOT \r:
        # ESC[1G (column 1) and ESC[A (up one line). Splitting on \r alone left
        # every redraw concatenated onto a single enormous line, so the 400-char
        # truncation cut off the error that follows them -- the first real
        # failure this feature captured (2026-08-05, Tesslate OmniCoder) logged
        # a wall of "pulling manifest" with the actual
        # "Error: context deadline exceeded" invisible past the cut. Treat those
        # cursor moves as line breaks first, then strip the remaining escapes.
        text = text.replace("\r", "\n")
        text = re.sub(r"\x1b\[(?:\d*[AG])", "\n", text)   # redraw boundaries
        text = re.sub(r"\x1b\[[0-9;?]*[A-Za-z]", "", text)  # other CSI codes
        cleaned = []
        for line in text.splitlines():
            # drop braille spinners (U+2800-U+28FF) and block-drawing bar glyphs
            line = "".join(c for c in line
                           if not ("⠀" <= c <= "⣿") and not ("▀" <= c <= "▟")).strip()
            if not line or "%" in line:
                continue
            if cleaned and cleaned[-1] == line:
                continue
            cleaned.append(line)
        return " | ".join(cleaned[-3:])[:400]

    def start_pull(model):
        # Skip the pull attempt entirely for models already installed --
        # matters for custom Modelfile builds (e.g. step-3.5-flash-*) that
        # aren't `ollama pull`-able under their local tag: a failed pull
        # (nonzero returncode) previously caused the per-model check below
        # to wrongly `continue`/skip an already-installed, benchable model.
        # Added 2026-07-29 after that exact bug forced killing and
        # rescheduling a mid-run bench (see project_gap_fill_custom_modelfiles
        # memory note).
        # Never re-download a model whose coverage is already complete. Added
        # 2026-08-05 after this turned out to be the root cause of the repeated
        # disk emergencies: reclaim_if_complete() deletes a fully-scored model
        # to free space, and the next pass dutifully downloads it again just to
        # re-confirm scores already on disk. At the time of the fix 46 of 68
        # TIER3 entries were in that state, including gemma4:26b-a4b-it-q8_0
        # (28GB), glm-4.7-flash:bf16 (~46GB) and llama4:16x17b (67GB). Two of
        # those re-pulls ran the box to 4.6GB free within minutes of each other
        # and had to be killed by hand. Re-pulling also inflated every pass into
        # a multi-day affair while the genuinely-pending 22 models waited.
        if all(coverage(model).values()):
            log(f"  [fetch] {model} already fully scored, skipping pull")
            return None
        if is_installed(model):
            log(f"  [fetch] {model} already installed, skipping pull")
            return None
        ok, free = headroom_ok(log, disk_state, model=model)
        if not ok:
            log(f"  [SKIP FETCH] {model} -- only {free:.1f}GB free, needs "
                f"{required_free_gb(model):.1f}GB "
                f"(~{SIZE_HINT_GB.get(model, DEFAULT_SIZE_HINT_GB):.0f}GB model "
                f"+ {PULL_BUFFER_GB:.0f}GB buffer)")
            return None
        log(f"  [fetch] starting background pull: {model}")
        pull_log = LOG_DIR / f"gap-fill-pull-{model_slug(model)}.log"
        try:
            handle = open(pull_log, "w", encoding="utf-8", errors="replace")
        except OSError:
            handle = None
        proc = subprocess.Popen(
            ["ollama", "pull", model], cwd=REPO_ROOT,
            stdout=handle if handle else subprocess.DEVNULL,
            stderr=subprocess.STDOUT if handle else subprocess.DEVNULL)
        if handle:
            pull_logs[model] = (pull_log, handle)
        return proc

    if TIER3:
        pull_procs[TIER3[0]] = start_pull(TIER3[0])

    for i, model in enumerate(TIER3):
        proc = pull_procs.get(model)
        if proc is not None:
            proc.wait()

        # kick off the NEXT model's fetch now, overlapping with this model's bench --
        # unconditionally, BEFORE any `continue` below, so one failed/skipped pull
        # doesn't break the fetch-ahead chain and silently skip every model after
        # it for the rest of the run (bug found 2026-07-25: a single bad tag mid-list
        # cascaded into ~30 consecutive false "disk headroom" skips).
        if i + 1 < len(TIER3):
            pull_procs[TIER3[i + 1]] = start_pull(TIER3[i + 1])

        detail = finish_pull_log(model) if proc is not None else ""
        if proc is not None and proc.returncode not in (0, None):
            log(f"  [FAIL] pull failed for {model} (exit {proc.returncode}), skipping benchmarks"
                + (f" -- {detail}" if detail else " -- (no output captured)"))
            continue
        # Complete models are neither pulled (above) nor benched -- benching
        # them re-runs a whole pipeline to reproduce scores already recorded.
        # This is what let a pass spend most of its wall-clock on
        # re-confirmations while pending models waited.
        if all(coverage(model).values()):
            continue
        if proc is None and not is_installed(model):
            log(f"  [SKIP] {model} not installed and fetch was skipped (disk headroom)")
            continue

        slug = model_slug(model)
        model_log = LOG_DIR / f"gap-fill-{slug}.log"
        log(f"--- {model} (fetched) ---")
        bench_model(model, model_log)
        completed += 1
        mark_fetched_in_tracking(model)
        log(f"  done + removed from missing_from_local: {model} (log: {model_log})")
        reclaim_if_complete(model)

    log(f"=== Gap-fill queue finished === ({completed} model(s) benched this pass)")
    return completed


def pending_models():
    """Models across all tiers that still have incomplete coverage."""
    pending = []
    for model in TIER0 + TIER1 + TIER2 + TIER3:
        try:
            if not all(coverage(model).values()):
                pending.append(model)
        except Exception:
            pending.append(model)  # can't tell -- assume still owed work
    return pending


def main():
    """Run passes until the work is done, or until passes stop achieving anything.

    Added 2026-08-04. A pass that ends on the disk-headroom guard leaves real
    work undone, and until now nothing restarted it -- the box sat idle for
    hours (3h45m on 2026-08-04, ~4h the day before) until a human noticed.
    Disk almost always recovers on its own shortly after a pass ends, because
    `ollama rm` releases blobs lazily, so simply waiting and going again is
    usually enough.

    Guarded against the failure mode the handoff warns about -- looping
    restarts that achieve nothing. A pass that benches zero models counts as
    no-progress, and NO_PROGRESS_LIMIT consecutive such passes stop the loop so
    a human can look, rather than spinning on a disk constraint that needs
    actually fixing.
    """
    log_dir_ready = LOG_DIR
    log_dir_ready.mkdir(parents=True, exist_ok=True)

    def note(msg):
        line = f"{time.strftime('%H:%M:%S')} {msg}"
        print(line, flush=True)
        with open(LOG_DIR / "gap-fill-queue.log", "a", encoding="utf-8") as f:
            f.write(line + "\n")

    no_progress = 0
    for pass_num in range(1, MAX_PASSES + 1):
        note(f"=== pass {pass_num}/{MAX_PASSES} ===")
        completed = run_pass(first_pass=(pass_num == 1))

        still_pending = pending_models()
        if not still_pending:
            note("=== all tracked models have complete coverage -- queue done ===")
            return

        if completed == 0:
            no_progress += 1
            note(f"  [loop] pass {pass_num} benched nothing "
                 f"({no_progress}/{NO_PROGRESS_LIMIT} consecutive)")
            if no_progress >= NO_PROGRESS_LIMIT:
                note(f"=== stopping: {no_progress} consecutive passes achieved nothing, "
                     f"{len(still_pending)} model(s) still pending. Needs a human: "
                     f"check free disk and the [SKIP FETCH]/[FAIL] lines above. ===")
                return
        else:
            no_progress = 0

        note(f"  [loop] {len(still_pending)} model(s) still pending; "
             f"sleeping {PASS_COOLDOWN_S}s before the next pass "
             f"(lets lazy `ollama rm` blob release land before disk is re-read)")
        time.sleep(PASS_COOLDOWN_S)

    note(f"=== stopping: reached MAX_PASSES ({MAX_PASSES}) ===")


if __name__ == "__main__":
    main()
