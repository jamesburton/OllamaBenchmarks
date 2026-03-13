# Cross-System Benchmark Summary

Systems captured: 3

| System | OS | CPU / RAM | GPU | Primary coding model | Best value / balanced | Fastest raw model | Backend note |
|---|---|---|---|---|---|---|---|
| framework-20260312 | Windows 10 | Intel64 Family 6 Model 170 Stepping 4, GenuineIntel / 95.47 GB | Intel(R) Arc(TM) Graphics, NVIDIA GeForce RTX 3060 | qwen3.5:latest (37.71 tok/s) (5/5) | qwen3.5:latest (37.71 tok/s) (5/5) | lfm2.5-thinking:1.2b (205.66 tok/s) (0/5) | auto led at 2.97 tok/s; rocm followed at 2.33 tok/s. |
| strix-20260311 | Windows 11 | AMD64 Family 26 Model 112 Stepping 0, AuthenticAMD / 63.65 GB | AMD Radeon(TM) 8060S Graphics | qwen3-coder-next:latest (33.92 tok/s) (5/5) | glm-4.7-flash:latest (43.11 tok/s) (5/5) | glm-4.7-flash:latest (43.11 tok/s) (5/5) | auto led at 33.89 tok/s; rocm followed at 33.1 tok/s. |
| t5500-20260311-xeon3060 | Windows 10 | Intel64 Family 6 Model 44 Stepping 2, GenuineIntel / 36.0 GB | NVIDIA GeForce RTX 3060 | lfm2:24b (11.67 tok/s) (4/4) | ministral-3:14b (29.68 tok/s) (4/4) | lfm2.5-thinking:latest (220.75 tok/s) (0/4) | auto led at 20.09 tok/s; rocm followed at 7.57 tok/s. |

## Update Process

1. Run the benchmark scripts to refresh the `*-current.json` artifacts for the target system.
2. Run `python .\scripts\archive_system_benchmarks.py --label <system-date-or-tag>` to snapshot the host metadata and current artifacts under `results/systems/`.
3. Run `python .\scripts\build_cross_system_summary.py` to rebuild this comparison table after each new system capture.

