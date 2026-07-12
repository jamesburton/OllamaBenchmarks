# Handoff

## Goal
Benchmark **mistral-medium-3.5** (dense 125B, 256k ctx, per-request reasoning toggle) on the Strix
box across the full suite (quality, throughput, L3, L2 raw+chat), with fair sampling for both
thinking and non-thinking modes, and document every platform quirk hit along the way.

## Current State (2026-07-05, HEAD `c3e55af` + uncommitted changes)

### What is complete and trustworthy

| Benchmark | Result | File |
|---|---|---|
| Quality | **11/11** (coding 5/5, tool 4/4, agentic 2/2) | `results/quality-mistral-medium-3.5_iq3m.json` |
| Throughput (cold/best-case) | **2.93 tok/s decode**, 100% GPU, 57.8 GB VRAM, 8192 ctx | `results/throughput-resource-mistral-medium-3.5_iq3m.json` |
| L3 no-think | **35/50, weighted 0.6909** | `results/coding-layer3-mistral-medium-3.5_iq3m.json` (merged into `results/coding-mistral-medium-3.5_iq3m.json`) |
| Perf-lever sweep | See below — bandwidth-bound confirmed, concurrency is the one real lever | `results/perf-levers/*.json` |

### ⛔ Overnight L2-chat run — STOPPED, discard, do not resume as-is

The overnight chain (L2 chat → L3 think=high → L2 raw), launched as a detached process the prior
session, was **killed this session** (PID 22104 and its child tree) after confirming it was still
RAM-contaminated (`Process terminated` / chat timeouts, same symptoms as before) and progressing
too slowly (~7 min/task, 40+ chat tasks still left plus two more full stages queued — well past a
4-hour remaining-time threshold). **L2 chat, L3 think=high, and L2 raw for this model are all still
outstanding** — none of the three has a trustworthy result yet.

### ✅ Perf-lever investigation — completed this session

Goal was to find levers to close the gap between "quality is great" and "speed isn't." Full
methodology, scripts, and results:

- `scripts/benchmark_sweep.py` (pre-existing, no service restart needed) — swept `num_thread`,
  `num_batch`, `num_gpu` runtime options. Output: `results/perf-levers/sweep-runtime-opts-iq3m.json`.
- `scripts/perf_lever_service_sweep.ps1` (new this session) — swept `OLLAMA_KV_CACHE_TYPE`
  (q8_0/q4_0) and `OLLAMA_NUM_PARALLEL` (concurrency), each requiring an Ollama service restart.
  Output: `results/perf-levers/service-sweep.json`.

**Headline findings** (full detail in MODEL_QUIRKS.md "mistral-medium-3.5:iq3m" point 6):

1. **Roofline: ~4.1 tok/s theoretical ceiling** at Strix's ~256 GB/s unified-memory bandwidth for
   this ~57.8 GiB dense model (one full weight sweep per decoded token). The original 2.93 tok/s
   is ~71% of that ceiling — already fairly efficient, not much headroom from micro-tuning.
2. **`num_thread`/`num_batch`/`num_gpu` — no effect.** All landed within noise of baseline. Expected:
   decode is a single-token GEMV already 100% GPU-offloaded; these levers affect prefill/host
   orchestration, not bandwidth-bound decode.
3. **KV cache quantization (q8_0/q4_0) — no effect.** KV cache is a small fraction of total
   bytes-per-token for a 125B dense model at 8192 ctx.
4. **Concurrency is the one real lever: `OLLAMA_NUM_PARALLEL=2` gave ~1.8x aggregate wall-clock
   throughput** for 2 concurrent requests (171.8s vs 311.7s wall-clock for the same total tokens),
   even though each individual stream's own tok/s didn't improve. **This is the actionable one** —
   running L2/L3 harnesses with concurrency ≥2 could cut suite wall-clock roughly in half. Only
   tested at concurrency=2 so far (tight system RAM); do not push higher without more headroom.
5. **Throughput decays within a session, even with a clean (non-leaked) process**: fresh-restart
   baseline measured 2.72 → 1.24 → 1.11 tok/s across 3 back-to-back identical calls in ~5 minutes.
   All later variants sat around ~1.05-1.1 tok/s regardless of lever. **Treat 2.93 tok/s as a
   cold/best-case number; budget multi-hour benchmark suites off ~1.0-1.2 tok/s steady-state.**
   Root cause (thermal vs. cumulative memory/bandwidth contention) not fully isolated — a
   reboot-vs-restart A/B would be needed to separate them if worth the time.

**Bug found and fixed along the way**: `Stop-Process -Name ollama,"ollama app"` **orphans the
`llama-server.exe` child**, leaking the entire model's memory (~60 GB) instead of freeing it. This
caused a real near-total-OOM mid-session (432 MB available RAM, next model load failed with
`cudaMalloc failed: out of memory`) — recovered by killing the orphan directly (24 GB available
afterward, no reboot needed). Documented in PLATFORM_QUIRKS.md; `perf_lever_service_sweep.ps1` now
kills `llama-server` explicitly on every restart. **Any other script/workflow that restarts Ollama
via `Stop-Process` on this box should add the same fix.**

## Key Decisions Made

- **Quant: IQ3_M (59.5 GB), not the official Q4 (80 GB).** The official Ollama `mistral-medium-3.5`
  pull cannot load at all under the current 96 GB BIOS UMA split — it hits the Windows commit
  ceiling (~67 GB) even though ROCm reports 89/89 layers offloaded. Confirmed by three separate
  OOM traces at different allocation sizes. See PLATFORM_QUIRKS.md "96 GB BIOS VRAM split caps
  Ollama loads". IQ3_M was chosen as the largest bartowski quant that fits comfortably under the
  ~65 GB envelope that has previously worked (nemotron-3-super).
- **Sharded GGUF merged on Framework, not Strix**, to avoid evicting the user's own models for
  transient merge disk (T5500 also lacked space). This required a whole ad-hoc pipeline (see What
  Worked) since Ollama can neither `pull` nor `create` directly from HF's multi-file GGUF layout.
- **Import replicates the official packaging exactly**: pulled the official manifest's TEMPLATE and
  SYSTEM blobs by digest from the Ollama registry API and reused them verbatim in the local
  Modelfile, then patched `"parser":"ministral"` into the config blob post-creation (Ollama's
  Modelfile has no PARSER directive) via `gguf-cache/mm35-meta/patch_parser.py`. This makes the
  IQ3_M import behaviorally identical to the official model except for quant level.
  Also added a `num_ctx:8192` params layer the same way (manifest surgery) — see below.
  Both patch scripts double as a proven pattern for future sharded/custom Ollama imports.
- **Thinking must be requested as the string `"high"`, not `think:true`.** Verified by direct probe:
  the official template maps only `ThinkLevel=="high"` → `reasoning_effort:"high"`; boolean
  `true` renders `"none"` silently (no error, just wrong mode — 4-token direct answer). Added a
  `think_env_enabled()` fairness check and a `mistral-medium*` branch to `sampling_options()` in
  `scripts/coding_tasks/task_runner.py` so thinking runs automatically get temp 0.7/top_p 0.95 (the
  vendor-recommended thinking-mode sampling) while non-thinking keeps temp 0/top_p 1 (inside the
  vendor's 0.0–0.7/1.0 non-thinking range, and consistent with every other model in the suite).

## What Worked

- **Detached process launch survives background-task kills.** Something in this session's
  environment repeatedly killed `run_in_background: true` Bash/PowerShell tasks (observed 5+ times
  on the same L3 run, cause not identified — possibly the desktop session or a watchdog). The fix:
  launch via `Start-Process -WindowStyle Hidden -RedirectStandardOutput ...` (PowerShell) so the
  process is a genuine detached child, not tied to the tool-call's tracked task. Then poll/tail the
  redirected log file directly rather than relying on the task's own liveness.
- **HTTP range server for resumable large-file transfer.** scp/sftp both failed on the 59.5 GB
  merged GGUF (see What Didn't Work). A small Python `http.server` subclass adding Range-header
  support (`gguf-cache` doesn't have this script committed — it was written ad hoc in `/tmp` on
  Framework at `c:\Development\gguf-tmp\rangesrv.py` and deleted after use; recreate from the
  transcript if needed again) let `curl -C -` resume cleanly after a Framework reboot killed the
  in-flight scp.
- **Manifest surgery for anything Ollama's Modelfile can't express** (parser field, params/num_ctx
  layer): read the local JSON manifest at
  `~/.ollama/models/manifests/registry.ollama.ai/library/<name>/<tag>`, write a new content-addressed
  blob, rewrite the layer's digest/size, done. No `ollama create` needed for post-hoc patches.
- Fetching the **official Ollama registry manifest directly** (`registry.ollama.ai/v2/library/<model>/manifests/latest`)
  to get exact TEMPLATE/SYSTEM/config blob digests, then downloading those blobs by digest — this is
  the reliable way to make a custom import match the vendor's packaging exactly.
- **Cross-referencing `curl /api/ps` against the OS process list** is the reliable way to tell a
  legitimately-tracked llama-server from an orphaned one — high process memory alone is not evidence
  of a leak (a single active load of this model legitimately uses ~24-26 GB working set).

## What Didn't Work

- **scp/sftp for the 59.5 GB merged GGUF over the Strix↔Framework Tailscale path.** Died mid-transfer
  multiple times; `sftp reget` did not actually resume (restarted from 0 / dropped instantly).
  Framework also spontaneously rebooted mid-transfer (its known flaky-reboot issue, see
  OTHER_MACHINES.md). Switched to a resumable HTTP range server + `curl -C -` instead — worked
  first try post-reboot.
- **`ollama pull hf.co/.../<quant-tag>` on a sharded GGUF repo** — fails immediately:
  `"The specified tag is a sharded GGUF. Ollama does not support this yet."` Must merge shards
  locally with `llama-gguf-split --merge` (binary already present in `C:\Development\llama.cpp\`
  and on Framework) before `ollama create` will accept the file.
- **`OLLAMA_HOST=0.0.0.0:11434`** (this box's User-env server-bind setting) **as a benchmark
  client URL** — every script fails instantly with `URLError: unknown url type: 0.0.0.0`. Always
  override to `$env:OLLAMA_HOST='http://127.0.0.1:11434'` in the launching session first.
- **`num_ctx:0`/unset on a 256k-context model** — resolves to model-max context, tries to allocate
  ~88 GB of KV cache, OOMs. `benchmark_throughput_resource.ps1` sends `num_ctx:0` by default; must
  pass `-NumCtx 8192` explicitly, or (as done here) bake a `num_ctx:8192` params layer into the model.
- **`ollama create` on a near-full disk** — failed twice with `not enough space on the disk` even
  though the source file was already present, because create writes a **second full validated copy**
  and also **leaks partial blobs on failure** (`sha256-<10 raw digits>`, safe to delete). Needed
  ~65 GB headroom, not just ~60 GB.
- **`Stop-Process -Name ollama,"ollama app"` to restart the service** — orphans `llama-server.exe`
  instead of killing it, leaking the whole model's RAM. See PLATFORM_QUIRKS.md 2026-07-05 entry.
  Must include `llama-server` in the `-Name` list too.

## Recent Changes (this session)

- Killed the contaminated overnight chain (PID 22104 + child tree).
- `scripts/benchmark_sweep.py` — pre-existing, run against mistral-medium-3.5:iq3m this session
  (results in `results/perf-levers/sweep-runtime-opts-iq3m.json`).
- `scripts/perf_lever_service_sweep.ps1` — new, tests KV-cache-type and concurrency levers via
  service restart; fixed mid-session to also kill `llama-server` on restart (was orphaning it).
- `PLATFORM_QUIRKS.md` — new entry: "`Stop-Process -Name ollama,"ollama app"` orphans
  `llama-server.exe`, leaking the whole model's RAM".
- `MODEL_QUIRKS.md` — new point 6 under "mistral-medium-3.5:iq3m" covering the full perf-lever
  sweep findings (roofline, no-effect levers, concurrency win, within-session throughput decay).
- `results/perf-levers/` — new directory: `sweep-runtime-opts-iq3m.json`, `service-sweep.json`,
  plus stdout/stderr logs for both runs.
- `results/quality-mistral-medium-3.5_iq3m.json`, `results/throughput-resource-mistral-medium-3.5_iq3m.json`,
  `results/coding-layer3-mistral-medium-3.5_iq3m.json`, `results/coding-mistral-medium-3.5_iq3m.json`,
  `results/coding-generated/mistral-medium-3.5_iq3m/` — carried over from prior session, still
  untracked/ready to commit.

## Important Context

- **This is the model everyone means by "mistral-medium-3.5" going forward on this box** — Ollama
  tag is `mistral-medium-3.5:iq3m` (not `:latest`, which is the broken official Q4).
- **System RAM is the active bottleneck, and it's tighter than previously known.** Under the 96 GB
  BIOS UMA split, this model alone leaves only a few hundred MB to low-GB free even in the
  healthy/no-leak case. Any additional restart-based testing must use the llama-server-inclusive
  kill pattern above, and should budget RAM headroom before trying concurrency > 2.
- **Real-world throughput for planning purposes is ~1.0-1.2 tok/s steady-state, not 2.93 tok/s.**
  Use this for any future timeout/duration budgeting (`L2_GEN_TIMEOUT_S` etc.).

## Next Steps

1. **Re-run L2 chat, L3 think=high, L2 raw** for this model now that the perf-lever picture is
   understood. Recommended: set `OLLAMA_NUM_PARALLEL=2` (restart required, use the fixed
   llama-server-inclusive kill pattern) and consider running 2 tasks concurrently if the harness
   supports it, to roughly halve wall-clock — otherwise budget for the ~1.0-1.2 tok/s steady-state
   number, which will make these runs multi-hour regardless. Re-raise `L2_GEN_TIMEOUT_S` and
   `L3_BUILD_TIMEOUT_S`/`L3_TEST_TIMEOUT_S` accordingly (see CLAUDE.md knobs).
2. **Quant-level tradeoff test — needs explicit user confirmation before running**, not yet started.
   Disk is only ~70 GB free; a smaller quant (e.g. IQ2_M, likely ~40-45 GB) would fit once, but
   `ollama create`'s patched-copy step needs ~2x the file size temporarily (~90 GB) — more than
   available. Would need either freeing significant disk first or reusing the Framework-merge
   pipeline again. Only pursue if the user wants to trade quality for speed.
3. **Optional, not yet done**: isolate whether the within-session throughput decay (2.72→1.11 tok/s)
   is thermal or memory-pressure-driven via a clean reboot-vs-restart-only A/B. Useful context but
   not blocking; the practical mitigation (budget off steady-state, don't trust cold numbers) is
   already actionable without knowing the exact cause.
4. **Write the `backend_notes` entry** in `benchmark-models.json` once L2 chat/L3 think/L2 raw are
   re-run and trustworthy: throughput (both cold and steady-state), RAM/VRAM, quality (11/11), L3
   no-think (35/50) + think, L2 raw (mark ARTIFACT) + chat.
5. **Commit**: `scripts/coding_tasks/task_runner.py`, `scripts/benchmark_sweep.py` (if changed),
   `scripts/perf_lever_service_sweep.ps1`, `MODEL_QUIRKS.md`, `PLATFORM_QUIRKS.md`, and all
   `results/*mistral-medium-3.5_iq3m*` + `results/perf-levers/*` files once the re-runs above land.
6. **Carried over from the 2026-07-02 handoff, still open, lower priority**: T5 long-context
   sweep on Strix for Qwythos-family models, Ornith-1.0-9B-MTP L2/L3 coding runs, Layer 4 summary
   chart. None of these touch mistral-medium-3.5 — pick up only after the above is committed.
