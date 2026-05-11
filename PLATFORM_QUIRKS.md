# Platform Quirks

Environmental diagnostics, gotchas, and recommended settings for the Windows hosts that run this benchmark harness. Each entry names a symptom you might see, the cause, and the verified fix.

## Strix Halo (AMD Ryzen AI 395, Radeon 8060S iGPU)

### Use ROCm, not Vulkan, for Ollama

Ollama's Strix Halo support uses the ROCm runtime, not Vulkan. The Vulkan ggml backend in llama.cpp can crash on the first generation when fed qwen35moe MoE models. The default Ollama install on Strix picks ROCm automatically — don't override it.

### `OLLAMA_GPU_MEMORY=120G` unlocks the full 128 GB unified memory

By default Ollama assumes the iGPU has only ~64 GB available (its half of the unified pool). Setting `OLLAMA_GPU_MEMORY=120G` exposes the full 128 GB DDR5 to model offload, which is what lets the 86 GB nemotron-3-super or 65 GB gpt-oss:120b load at 100% GPU. Set it as a system environment variable so it persists across Ollama service restarts.

### BIOS UMA frame buffer must be small for large models

Ollama refuses to load large models on Strix when the BIOS UMA frame buffer is set high (≥ 64 GB). The cause is that the UMA reservation creates a hard partition, and Ollama's allocator double-counts it. The fix is to reduce the BIOS UMA frame buffer to 16–32 GB and let `OLLAMA_GPU_MEMORY=120G` (above) handle dynamic allocation. After changing the BIOS setting, a full power-cycle (not just reboot) is required for the firmware to release the reserved pool.

### `OLLAMA_FLASH_ATTENTION=1` and `OLLAMA_SKIP_MEMORY_CHECK=1`

Both help on Strix. Flash attention is a meaningful tok/s win on long contexts; the memory check is overly conservative for unified-memory layouts and refuses to load models that would actually fit.

### Single iGPU = sequential model loads

Benchmark runs on Strix must be sequential. Loading a second model while another is in use will not produce parallel inference — Ollama will queue. For the benchmark harness this means: do not try to parallelize the `run_think_variants_l3.py` or `fill_l2_gaps.py` chains; use the supplied sequential drivers.

### `ollama ps` 4-minute idle timeout

By default Ollama unloads a model after 4 minutes of idle. The L3 coding benchmark generates one task at a time with `dotnet build`/`test` in between (often 30–60 s each), so the model stays warm. The L2 chain runs back-to-back generations with no test phase between — fine. The think variants of slow dense models (qwen3.6:27b, gemma3:27b) can occasionally cross the 4-minute boundary during `dotnet test`; the next request reloads in ~5 s on iGPU, so this is cosmetic. If you see "Stopping…" in `ollama ps`, the model is mid-unload but the next request will trigger a reload.

## Framework 16 (Intel Core Ultra + RTX 3060 12 GB)

### GPU offload sweet spot

The RTX 3060 has 12 GB VRAM. Models up to ~6–7 GB run fully on GPU. Models in the 9–11 GB range partially offload and lose half their speed because of CPU/GPU transfer. Models above 12 GB run with `cpu_avg_pct` > 20% and tok/s drops by 4–10×. The "fully GPU fits" tier is the sweet spot: nemotron-3-nano:4b (3 GB, 84 tok/s), gemma4:e2b (9 GB, 89 tok/s), cogito:8b (4.9 GB, 61 tok/s).

### `num_ctx` cost at full size

Long context KV cache costs VRAM (≈ `num_layers * num_heads * head_dim * 2 * num_ctx` bytes per quant byte). On a 12 GB GPU a 256 K context for an 8 GB model exceeds VRAM and forces partial offload. TurboQuant TQ3 or similar can compress the KV cache enough to fit; otherwise cap `num_ctx` to 32 K for the benchmark suite.

## T5500 (older Xeon + RTX A4000-equivalent, 36 GB RAM + 12 GB VRAM)

### Pre-0.20 Ollama qwen35moe crash

Ollama 0.18.x and 0.20.7 panic at `ggml.go:276` when asked to load qwen35moe 35B variants (qwen3.5:27b/35b, qwen3.6). Small variants (qwen3.5:4b, qwen3.5:9b) of the same architecture load fine. Upgrade Ollama on the host before benchmarking these models; do not use llama-server as a workaround for this arch.

### MoE memory floor

Nemotron-Cascade 2 (30B-A3B MoE) needs ~22.6 GiB system memory at load time. T5500 has 36 GB RAM but only ~22.4 GiB free after OS/CUDA overhead, so the load crashes the Ollama service. Even the smallest HF quant (IQ1_S, 18 GB) hits the same floor due to the MoE expert layout. This is a Strix-only model in practice.

## Common to all Windows hosts

### Ollama service is per-user, not per-machine

`OLLAMA_GPU_MEMORY` and `OLLAMA_FLASH_ATTENTION` must be set in the *user* environment (or as `Machine` scope) and the Ollama tray app restarted before they take effect. Setting them only in a shell session does not change the service's environment. Verify with `Get-Process ollama | Format-List Path,StartTime` and re-launch from the Start menu if `StartTime` predates your env change.

### `--no-restore` requires a warm NuGet cache

Both Layer 2 and Layer 3 use `dotnet build --no-restore` to keep iteration fast. The template cache (`scripts/coding_tasks/templates/.cache/`) pins exact transitive package versions. If those versions are no longer in `%USERPROFILE%\.nuget\packages\`, every build fails with NETSDK1064. Delete `templates/.cache/` to force a fresh restore on next run.

### Python output buffering during long subprocess runs

Python's `subprocess.run(capture_output=True)` buffers stdout/stderr in memory until the subprocess exits. For long L2/L3 runs (30–90 min), the buffer never flushes mid-run, so `Get-Content` on the output file returns empty until the job finishes. The benchmark drivers (`run_overnight_strix.ps1`, `run_think_variants_l3.py`, `fill_l2_gaps.py`) tee through PowerShell so you can `Get-Content -Tail`. To monitor progress mid-run, look at `results/coding-generated/<model_slug>/*.cs` mtimes or `ollama ps`.

### HF Hub auth and Ollama HF GGUF pulls

`ollama pull hf.co/<owner>/<repo>:<tag>` works without auth for public repos but is rate-limited to ~10 MB/s without an HF token. Set `HF_TOKEN` in the user environment and cache the token at `%USERPROFILE%\.cache\huggingface\token` to lift the cap. Final small config blobs (~400 bytes) occasionally time out near 100%; a manual `curl` of the blob URL with `Authorization: Bearer $HF_TOKEN` then re-running `ollama pull` finalizes the import.

### Connectivity bottlenecks during large HF pulls

A model pull stalling at 1–2 MB/s for hours despite a 1 Gbit/s WAN link almost always indicates a Wi-Fi / wired LAN routing problem rather than an HF throttle. The Carnice-V2 27B pull took 3.5 days on wired Ethernet and finished in minutes after switching to Wi-Fi on the same machine. Run `Test-NetConnection huggingface.co` and `Speedtest.exe` to confirm.

### `git status -uall` on this repo

The `results/coding-generated/` tree contains ~1 800 generated `.cs` files per model. `git status -uall` enumerates them all and can OOM on hosts with < 16 GB RAM. Use plain `git status` (the default) which truncates untracked listings.

---

*Extend and refine these notes as insights are proven*
