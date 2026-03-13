# Requested Model Sweep (2026-03-13)

## Requested models

| Model | Throughput (tok/s) | Quality | Notes |
|---|---:|---:|---|
| cogito:8b | 41.27 | 3/5 | Fastest of the requested set, but weak on tool-use |
| cogito:14b | 28.88 | 5/5 | Best overall requested model on this machine |
| zac/phi4-tools:latest | 29.85 | 2/5 | Good decode speed, but weak on tool and agentic checks |
| phi4-mini:latest | 1.03 | 2/5 | Very slow under this harness on this machine |

## Artifacts

- `results/throughput-resource-requested-20260313.json`
- `results/quality-requested-20260313.json`
- `results/throughput-resource-cogito_8b.json`
- `results/throughput-resource-cogito_14b.json`
- `results/throughput-resource-zac_phi4-tools_latest.json`
- `results/throughput-resource-phi4-mini_latest.json`
- `results/quality-cogito_8b.json`
- `results/quality-cogito_14b.json`
- `results/quality-zac_phi4-tools_latest.json`
- `results/quality-phi4-mini_latest.json`
