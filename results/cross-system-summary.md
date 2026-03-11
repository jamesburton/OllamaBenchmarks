# Cross-System Benchmark Summary

Systems captured: 1

| System | OS | CPU / RAM | GPU | Primary coding model | Best value / balanced | Fastest raw model | Backend note |
|---|---|---|---|---|---|---|---|
| strix-20260311 | Windows 11 | AMD64 Family 26 Model 112 Stepping 0, AuthenticAMD / 63.65 GB | AMD Radeon(TM) 8060S Graphics | qwen3-coder-next:latest (32.87 tok/s) (4/4) | lfm2:24b (33.54 tok/s) (4/4) | glm-4.7-flash:latest (43.94 tok/s) (2/4) | auto led at 32.59 tok/s; rocm followed at 32.13 tok/s. |

## Update Process

1. Run the benchmark scripts to refresh the `*-current.json` artifacts for the target system.
2. Run `python .\scripts\archive_system_benchmarks.py --label <system-date-or-tag>` to snapshot the host metadata and current artifacts under `results/systems/`.
3. Run `python .\scripts\build_cross_system_summary.py` to rebuild this comparison table after each new system capture.

