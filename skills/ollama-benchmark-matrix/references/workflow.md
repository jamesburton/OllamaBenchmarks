# Snapshot Layout

Archive each machine under `results/systems/<host-slug>-<label>/`.

Each snapshot should contain:

- `host-info.json`
- `manifest.json`
- `throughput-resource-current.json`
- `quality-current.json`
- `backend-comparison-current.json`
- `optimization-sweep-current.json`
- `session-YYYY-MM-DD-summary.md` when available

# Comparison Table Fields

Keep these fields in `results/cross-system-summary.md`:

- System id
- OS
- CPU and RAM
- GPU
- Primary coding model
- Best value / balanced model
- Fastest raw model
- Backend note

# Verification Checklist

- The archive directory exists and contains `host-info.json` plus `manifest.json`.
- `manifest.json` points at the copied artifacts for the machine.
- `results/cross-system-summary.md` includes one row per archived system.
- The narrative summary still aligns with the latest local benchmark results.
