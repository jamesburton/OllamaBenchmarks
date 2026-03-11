# Plan 01: Automation And Baselines

## Goal

Turn the ad hoc benchmark flow into a repeatable baseline run that can be executed after any Ollama, driver, firmware, or model update.

## Tasks

1. Freeze prompt sets for throughput, quick quality, and expanded quality.
2. Add a single orchestrator script that calls all benchmark scripts in order.
3. Add stable output naming with timestamped result directories.
4. Add result diffing against the latest prior run.
5. Add a short validation check that rejects incomplete model result sets.

## Success criteria

- One command produces a complete run directory.
- Re-running the suite on the same model set yields comparable JSON structure.
- Missing models or failed runs are surfaced explicitly.

