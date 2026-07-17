# Handoff

## Goal

Two threads this session, both now complete:

1. Finish benchmarking **mistral-medium-3.5:iq3m** (dense 125B, 256k ctx) — the L2 chat, L3
   think=high, and L2 raw re-runs that the 2026-07-05 handoff left outstanding (contaminated
   overnight chain had been killed, none of the three had a trustworthy result).
2. Benchmark two new small models — **codegemma:latest** and **codegemma:2b** — quality, L2
   raw+chat, L3, throughput, and compare against the top-5 on Strix for fine-tuning viability.

Both are done. Along the way, fixed a genuine cross-machine build-break bug and added
per-task-checkpoint/resume support to harnesses that lacked it (which then immediately paid for
itself when Framework crashed mid-run).

## Current State (2026-07-14, HEAD `8275638` + uncommitted changes below)

### mistral-medium-3.5:iq3m — all four benchmark layers now trustworthy

| Benchmark | Result | File |
|---|---|---|
| Quality | 11/11 (unchanged from prior session) | `results/quality-mistral-medium-3.5_iq3m.json` |
| L2 chat | **51.27%** (81/158) | `results/coding-mistral-medium-3.5_iq3m-chat.json` |
| L2 raw | **41.14%** (65/158) | merged into `results/coding-mistral-medium-3.5_iq3m.json` |
| L3 no-think | **0.691** weighted (35/50, unchanged from prior session) | same file, `layer3_*` keys |
| L3 think=high | **abandoned** — see below | not run |

L3 think=high was deliberately abandoned, not merely deferred: at this model's ~1.0-1.2 tok/s
bandwidth-bound steady-state (established in the 2026-07-05 perf-lever sweep), think=high's extra
reasoning-token overhead pushed real per-task generation past even a bumped 1200s timeout, with an
unrelated concurrent GPU job (a separate 24h LoRA retrain) on the same Strix box adding further
real contention. Projected ETA was ~10 hours for 50 tasks with 0 passes recorded. User's call:
not worth it given the no-think baseline (0.691) and L2 chat (51.27%) already characterize this
model's quality. See MODEL_QUIRKS.md "mistral-medium-3.5:iq3m" point 7 for the full writeup.

### codegemma:latest / codegemma:2b — full sweep complete, committed

| Model | Quality | L2 raw | L2 chat | L3 |
|---|---|---|---|---|
| `codegemma:latest` (9B instruct) | 4/11 | 31.6% | 32.3% | 0.382 |
| `codegemma:2b` (base, FIM-only, NOT instruction-tuned) | 0/11 (genuine) | 16.5% | 1.9% | 0.0 (genuine) |

Verdict already written into `benchmark-models.json` backend_notes: `codegemma:latest` is a
reasonable small-model (9B) fine-tune target for C#/.NET completion or bolting on tool-calling
(zero native tool-calling ability is the gap, quality otherwise mid-pack); `codegemma:2b` is a
genuine base/FIM-only model, not viable for general instruction-following fine-tuning. Full
comparison against the top-5 Strix models is in `benchmark-models.json`. This work, plus the
mistral-medium-3.5 perf-lever sweep and initial 2-worker parallel infra, was **committed and
pushed** in `8275638`.

### Uncommitted work from this session (not yet committed)

- **Cross-machine build-break root-caused and fixed.** An ambient `%TEMP%\Directory.Build.props`
  (sets `TreatWarningsAsErrors=true`, `EnforceCodeStyleInBuild=true`) plus an ambient
  `%TEMP%\.editorconfig` (escalates Style/Design/Performance analyzer categories to `warning`) on
  **Framework** — not part of this repo, pre-existing on that box for the user's other .NET work —
  combine to turn every LLM-generated-code style nit into a build error, since every benchmark task
  builds in a fresh `%TEMP%\...` subdirectory that inherits both files via MSBuild's automatic
  upward directory search. Fixed with a **single, non-analyzer-severity property** added to all
  four template `.csproj` files: `<EnforceCodeStyleInBuild>false</EnforceCodeStyleInBuild>` +
  `<TreatWarningsAsErrors>false</TreatWarningsAsErrors>`. This satisfies the user's explicit
  instruction ("do not set analyzer severities to none — only targeted exclusions") because it
  doesn't touch any analyzer/rule severity at all, just the two MSBuild switches that promote
  warnings to fatal errors. The four templates' `.editorconfig` overrides (an earlier,
  insufficient attempt — see below) were deleted as redundant once this landed.
- **Per-task checkpoint/resume added to harnesses that lacked it**, mirroring the pattern
  `benchmark_coding_layer2.py` (raw) already had:
  - `scripts/benchmark_coding_layer2_chat.py` — had NO per-task checkpoint at all (wrote once at
    the end). Now checkpoints after every task and resumes by skipping already-completed problem
    names on restart.
  - `scripts/benchmark_coding_layer2.py` — already checkpointed per-task but did not skip
    already-done problems on restart (would redo everything). Fixed.
  - `scripts/benchmark_coding_layer3.py` — had NO per-task checkpoint at all. Same fix as L2 chat.
  - `scripts/run_parallel_workers.py` — removed the `shutil.rmtree(work_root, ...)` at the start
    of both `run_l2_stage`/`run_l3_stage` (was wiping any resumable per-worker checkpoint on
    restart); dataset/task sharding is deterministic given the same input+worker-count so
    re-splitting onto the same `worker{i}` dirs is safe. Also added periodic live-merging
    (`run_workers()` now polls every 90s via an `on_tick` callback instead of blocking on
    `proc.wait()`) so the merged output file is refreshed continuously during a run, not just at
    the end — gives real-time progress visibility and means a crash mid-run leaves a fresh-ish
    merged snapshot, not just per-worker fragments.
  - This paid off immediately: **Framework crashed and was offline for ~1.5 hours mid-L2-raw-run**
    (see below). On reboot, the run was resumed from checkpoint (151/158 already done) rather than
    restarted from scratch.
- **L3 generation timeout made configurable.** `task_runner.py`'s per-task Ollama call timeout was
  hardcoded `600` regardless of task weight. Added `L3_GEN_TIMEOUT_S` env override (default stays
  `600` for standalone/fast-model runs); `run_parallel_workers.py`'s `base_env()` now defaults it to
  `1200` for parallel cross-machine runs, matching the existing `L2_CHAT_TIMEOUT_S=1200`. This was
  necessary but NOT sufficient to make L3 think=high viable for mistral-medium-3.5 (see abandonment
  above) — the model is just too slow for that variant on this hardware.

## Key Decisions Made

- **Abandon L3 think=high for mistral-medium-3.5 rather than push through or wait out contention.**
  User's explicit call after being shown the ~10hr ETA and 0/50 pass rate so far. Reasoning: the
  quality signal (think-vs-no-think comparison) isn't worth hours of wall-clock on a model whose
  practical-use verdict ("too slow for production use here") is already established independent of
  this specific data point.
- **Leave the concurrent GPU job on Strix alone rather than reclaim/kill it.** When discovered that
  another job (`claude-moe-sc-retrain`, a 24h LoRA retrain) had force-claimed our GPU lock after it
  went stale (forgot to `refresh` it for ~12 hours), user chose "leave both running, just monitor"
  over reclaiming or investigating further. The lock was later restored to our name by the other
  process yielding on its own; from then on refreshed every ~20 min for the rest of the session.
- **Framework flagged as highly contended — default to NOT using it.** Added explicit guidance to
  `~/.claude/OTHER_MACHINES.md`: only reach for Framework when the task specifically needs its
  unique resource combination (CUDA + real discrete/iGPU pair, more RAM, newer CPU); otherwise
  avoid it. Grounded in this session's findings: `nvidia-smi` confirmed the RTX 3060 eGPU
  genuinely fell off the bus mid-session ("GPU is lost, reboot to recover"), event log shows a
  recurring unclean-shutdown pattern (Kernel-Power Event 41) roughly every 2-6 days, and it's
  never actually idle (VS Build Tools, chrome-devtools-mcp, context7-mcp, playwright-mcp,
  QdrantSkillsMCP, etc. all resident throughout, unrelated to our work). It went fully offline for
  ~1.5 hours mid-session (see What Didn't Work).
- **`EnforceCodeStyleInBuild=false` + `TreatWarningsAsErrors=false` over deleting/disabling the
  ambient Framework files.** Those files aren't part of this repo and are presumably intentional
  for the user's other .NET work on that box — fixing it at the per-template-project level is
  scoped correctly and doesn't touch machine state outside this repo.

## What Worked

- **`gpu-lock.sh` (from the sibling `dotLLM` repo) as the cross-project GPU coordination
  mechanism** for Strix — acquire/refresh/release around the whole cross-machine run. Caveat: must
  actually `refresh` periodically (every ~20 min against the 30-min staleness timeout) or another
  job can legitimately force-reclaim it.
- **WMI-detached process launch** (`Invoke-CimMethod Win32_Process.Create` via
  `scripts/launch_remote_worker.ps1`) for kicking off multi-hour cross-machine runs that must
  survive the launching SSH/tool session ending — proven again this session across L2 chat, the
  (abandoned) L3 think attempt, and L2 raw.
- **Per-task checkpoint + resume-by-skipping-done-names** — directly saved this session's L2 raw
  run from being redone from scratch after Framework's ~1.5hr outage.
- **Cross-checking `tailscale status` against SSH** when a box seems unreachable — distinguishes
  "genuinely offline" (Tailscale itself reports `offline, last seen Nm ago`) from a transient
  SSH-layer hiccup (Tailscale still shows connected/active but the SSH banner exchange times out
  — happened repeatedly this session, always recovered within a few minutes; the real 1.5hr outage
  showed a proper `offline` status the whole time).
- **Checking process CPU-time-vs-age via `Get-Process ... | Select CPU` before killing anything
  labeled a "zombie"** — found 33 genuinely-idle stray `dotnet.exe`/`cmd.exe` processes on
  Framework this way (some 37+ hours old with near-zero accumulated CPU) and killed only those,
  leaving the one actively-computing build process untouched.
- **Manual model probing before trusting a harness's near-zero score.** Caught a stale
  `OLLAMA_HOST=0.0.0.0:11434` (no scheme) that silently zeroed `codegemma:latest`'s quality score
  via the harness's broad `except Exception: pass`; a plain `curl` immediately proved the model was
  fine. Documented in MODEL_QUIRKS.md as a standing gotcha.

## What Didn't Work

- **Trusting a single WMI-launch success message when connectivity is flaky.** Retried the L3
  think=high launch 2 more times after seeing what looked like immediate failures ("file not
  found" checking the log seconds later) — all 3 attempts had actually succeeded, just with very
  delayed tool-notification arrival, leaving 3 duplicate orchestrators racing on the same
  work-root. Recovered by verifying via `wmic process where "CommandLine like '...'"` before
  assuming a launch failed, then killing the duplicates and relaunching once, cleanly. **Always
  verify via a live process query before retrying a launch on this box.**
- **The original targeted `.editorconfig`-only fix for the cross-machine build-break** (from the
  session before this one, `dotnet_diagnostic.IDE0055.severity = none`) — insufficient. A much
  broader set of rules (IDE0040, IDE0004, IDE0047, IDE0100, IDE0300, IDE0306, IDE0028, IDE0060,
  plus the genuine Roslyn analyzer `CA1852`) were also being elevated to build errors, none of
  which that single-rule override addressed. Root cause was the ambient
  `Directory.Build.props`/`.editorconfig` in Framework's `%TEMP%`, not anything in this repo's own
  templates — see Key Decisions above for the actual fix.
- **~1.5 hour Framework outage mid-L2-raw-run.** `nvidia-smi` showed the eGPU as lost shortly
  before; Tailscale then reported it fully offline for ~90 minutes (well beyond this box's usual
  transient blips), consistent with a real crash+reboot cycle (event log confirmed a matching
  Kernel-Power Event 41 at boot). No remote fix was possible — just had to wait for it to come back
  on its own, then resume the checkpointed run. This is exactly the scenario the checkpoint/resume
  work above was added to protect against, and it worked.

## Recent Changes (this session, uncommitted)

- `scripts/benchmark_coding_layer2_chat.py` — added per-task checkpointing + resume-by-name.
- `scripts/benchmark_coding_layer2.py` — added resume-by-name (checkpointing already existed).
- `scripts/benchmark_coding_layer3.py` — added per-task checkpointing + resume-by-name.
- `scripts/coding_tasks/task_runner.py` — `L3_GEN_TIMEOUT_S` env override for the previously
  hardcoded 600s generation timeout.
- `scripts/run_parallel_workers.py` — removed destructive `rmtree` of `work_root` on restart;
  `run_workers()` now polls with periodic `on_tick` merge callback instead of blocking wait;
  `base_env()` now sets `L3_GEN_TIMEOUT_S=1200` default.
- `scripts/coding_tasks/templates/{layer2_project,test_project,blazor_project,agentic_csharp}/*.csproj`
  — added `<EnforceCodeStyleInBuild>false</EnforceCodeStyleInBuild>` +
  `<TreatWarningsAsErrors>false</TreatWarningsAsErrors>`.
- `scripts/coding_tasks/templates/{layer2_project,test_project,blazor_project,agentic_csharp}/.editorconfig`
  — deleted (superseded, redundant once the csproj fix landed).
- `MODEL_QUIRKS.md` — new point 7 under "mistral-medium-3.5:iq3m" (L3 think=high abandonment
  writeup).
- `~/.claude/OTHER_MACHINES.md` — Framework's "Role" line updated with an explicit "highly
  contended, avoid by default" flag, plus a new dated note with this session's specific findings
  (GPU-lost, crash-history cross-check, thermal readings, the duplicate-WMI-launch gotcha).
- `results/coding-mistral-medium-3.5_iq3m.json` — L2 raw results merged in (L3 no-think preserved).
- `results/coding-mistral-medium-3.5_iq3m-chat.json` — new, L2 chat results (untracked).

Already committed and pushed in `8275638` (prior turn this session): codegemma full benchmark
suite + backend_notes, the mistral-medium-3.5 perf-lever sweep, and the first cut of the
cross-machine parallel-worker infra (before the checkpoint/resume and build-break fixes above).

## Important Context

- **Framework is CPU-only for this workload** — it never touches its own GPU for these benchmarks
  (dotnet build/test only); mistral-medium-3.5 generation always happens on Strix via
  `OLLAMA_HOST=http://strix:11434`. The eGPU-lost/crash issues found this session are real but
  don't directly break this specific cross-machine pattern, they just make the box unreliable to
  depend on generally (see the new OTHER_MACHINES.md guidance).
- **GPU lock discipline matters on shared boxes**: acquire, and actually refresh every ~15-20 min
  for the duration of a multi-hour run — a forgotten lock will silently go stale (30 min default)
  and can be legitimately reclaimed by another job.
- **mistral-medium-3.5's real steady-state throughput (~1.0-1.2 tok/s) governs every future timeout
  decision for this model** — established 2026-07-05, reconfirmed this session. Any new
  benchmark/harness timeout for this specific model should budget multiplicatively off that number
  (2x+ for think-mode variants), not additively.
- **Background-task tool notifications on this setup can arrive very delayed** (sometimes 10+
  minutes) relative to when the underlying remote command actually completed — this caused the
  duplicate-launch incident above. When in doubt about whether a launch/kill actually took effect,
  query live process state rather than trusting the first notification.

## Next Steps

1. **Commit the uncommitted work** listed above (checkpoint/resume additions, the build-break fix,
   updated mistral-medium-3.5 L2 raw+chat results, MODEL_QUIRKS.md entry). No commit has been made
   since `8275638`.
2. **Write the final `backend_notes` entry** for `mistral-medium-3.5:iq3m` in
   `benchmark-models.json` now that all four layers (quality/L2 raw/L2 chat/L3 no-think) are
   trustworthy, plus a note that L3 think=high was deliberately not run and why. (codegemma's
   backend_notes are already written and committed.)
3. **Sync the checkpoint/resume + build-break fixes to Framework's repo copy** if any further
   cross-machine runs are planned for other models — the local Strix repo has these fixes; confirm
   Framework's copy is in sync via `git pull` there before the next cross-machine run (it was
   manually `scp`'d mid-session for immediate use, but a clean `git pull` after committing is safer
   than relying on that).
4. **Quant-level tradeoff test for mistral-medium-3.5 — still not started**, carried over from the
   2026-07-05 handoff, still needs explicit user confirmation on target quant before executing.
   Disk headroom should be rechecked (was ~175-185 GB free at various points this session).
5. **Carried over, still open, lower priority**: T5 long-context sweep for Qwythos-family models,
   Ornith-1.0-9B-MTP L2/L3 coding runs, Layer 4 summary chart. Unrelated to this session's work.
