---
name: ollama-benchmark-matrix
description: Run the Ollama benchmark harness in this repository, capture host metadata, archive per-system benchmark snapshots, and update cross-system comparison tables. Use when Codex needs to benchmark the current machine, add another system to the benchmark matrix, or refresh summaries under results/systems and results/cross-system-summary.md.
---

# Ollama Benchmark Matrix

Use this skill to benchmark a machine with the repository harness and keep the cross-system comparison artifacts current.

## Workflow

1. Inspect `results/` and confirm whether the `*-current.json` artifacts already exist for the machine you are working on.
2. If the artifacts are missing or stale, run:
   - `.\scripts\benchmark_throughput_resource.ps1 -Models @(...) -OutputPath .\results\throughput-resource-current.json`
   - `python .\scripts\benchmark_quality.py --models ... --output .\results\quality-current.json`
   - `.\scripts\benchmark_backend.ps1 -Model <lead-model> -OutputPath .\results\backend-comparison-current.json`
   - `python .\scripts\benchmark_sweep.py --model <lead-model> --output .\results\optimization-sweep-current.json`
   - Run the backend comparison and option sweep sequentially for very large models; do not overlap them when a model already pushes system memory or VRAM limits.
3. Capture host details and archive the current benchmark set with:
   - `python .\scripts\archive_system_benchmarks.py --label <yyyymmdd-or-system-tag>`
4. Rebuild the matrix summary with:
   - `python .\scripts\build_cross_system_summary.py`
5. Refresh the narrative summary in `results/session-YYYY-MM-DD-summary.md` if the leaderboard or recommendations changed.

## Required Outputs

- `results/throughput-resource-current.json`
- `results/quality-current.json`
- `results/backend-comparison-current.json`
- `results/optimization-sweep-current.json`
- `results/systems/<host-slug>-<label>/host-info.json`
- `results/systems/<host-slug>-<label>/manifest.json`
- `results/cross-system-summary.md`

## Host Metadata

Use `python .\scripts\collect_host_info.py` when you need to inspect the captured machine details directly. The benchmark archive step must preserve:

- Hostname and normalized host slug
- OS version and architecture
- Manufacturer and model
- CPU identity and logical CPU count
- Total physical memory
- GPU names and driver versions
- Ollama version

## Cross-System Table Maintenance

When adding a new machine:

1. Keep its freshly generated `*-current.json` files in `results/`.
2. Run the archive step to create a new directory under `results/systems/`.
3. Rebuild `results/cross-system-summary.md`.
4. Verify the new row shows the system hardware and the benchmark leaders for that machine.

For the expected snapshot layout and table fields, read [references/workflow.md](references/workflow.md).
