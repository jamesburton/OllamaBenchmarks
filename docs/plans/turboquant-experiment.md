# TurboQuant KV Cache Experiment Plan

**Date:** 2026-03-30
**Goal:** Test TurboQuant KV cache quantization for parallel inference on Strix + large MoE models on Framework

---

## Background

qwen3-coder-next (51GB model) uses 58.6GB VRAM on Strix with a 262K context window.
With OLLAMA_NUM_PARALLEL=4, there's no VRAM headroom for additional KV cache slots,
so parallel requests serialize anyway.

TurboQuant compresses KV cache from FP16 to 3-4 bits, giving 4-5x memory reduction.
This would free ~22GB for parallel KV slots.

## Current Ollama KV Options (available now)

```bash
# q8_0: 2x compression (28GB -> 14GB KV)
OLLAMA_KV_CACHE_TYPE=q8_0 OLLAMA_NUM_PARALLEL=2 OLLAMA_FLASH_ATTENTION=1 ollama serve

# q4_0: 4x compression (28GB -> 7GB KV) — closest to TurboQuant
OLLAMA_KV_CACHE_TYPE=q4_0 OLLAMA_NUM_PARALLEL=4 OLLAMA_FLASH_ATTENTION=1 ollama serve
```

## TurboQuant Implementation Sources

1. **ikawrakow/ik_llama.cpp** — Most complete, has CPU + CUDA kernels
   - Gist: https://gist.github.com/veritatisquaesitoressumus/6aa5973955007ffd858889c76aa60408
   - Branch: `ug/turboquant-tq3_0` (ubergarm fork)

2. **TheTom/llama-cpp-turboquant** — Fork of mainline llama.cpp with TQ types
   - https://github.com/TheTom/llama-cpp-turboquant

3. **Aaryan-Kapoor fork** — Branch: `turboquant-tq3_0`

### Usage flags (once built)
```bash
llama-server --cache-type-k tq3 --cache-type-v tq3 -m model.gguf
llama-server --cache-type-k tq4 --cache-type-v tq4 -m model.gguf
```

### Expected results
- TQ3: 4.9x KV compression, ~0.1 PPL degradation
- TQ4: 3.8x KV compression, ~0.02 PPL degradation
- With TQ3 on qwen3-coder-next: KV drops from ~28GB to ~6GB
  -> Room for 4-5 parallel request slots in remaining VRAM

## Build Requirements

- CMake 3.14+
- C++17 compiler (MSVC 2022, GCC 11+, or Clang 14+)
- For GPU: Vulkan SDK (AMD) or CUDA toolkit (NVIDIA)
- **Strix currently lacks build tools (only VS 2018 installed)**

### Setup on Strix (needs one-time install)
```powershell
# Install Visual Studio 2022 Build Tools + CMake
winget install Microsoft.VisualStudio.2022.BuildTools
winget install Kitware.CMake
```

### Or build on a machine with tools, then copy the binary

## Worktree Setup (on a build-capable machine)

```bash
cd /path/to/workspace
git clone https://github.com/TheTom/llama-cpp-turboquant.git
cd llama-cpp-turboquant
cmake -B build -DGGML_VULKAN=ON  # For AMD GPU
cmake --build build --config Release -j
# Test:
./build/bin/llama-server -m /path/to/qwen3-coder-next.gguf \
  --cache-type-k tq3 --cache-type-v tq3 \
  --host 0.0.0.0 --port 8080
```

## Strix Unified Memory Issue

**Current state:** 137.4 GB physical RAM, Windows reports 4.3GB GPU VRAM,
Ollama somehow uses 58.6GB (via Vulkan shared memory).

**BIOS fix:** Set "iGPU Memory Configuration" / "UMA Frame Buffer Size" to
maximum available (up to 96GB on Strix Halo).

**Ollama override:**
```bash
OLLAMA_GPU_MEMORY=96GB ollama serve
```

**Linux (full access):**
```
# In /etc/default/grub:
GRUB_CMDLINE_LINUX_DEFAULT="amdgpu.gttsize=117760"
# Then: sudo update-grub && reboot
# This gives ~115GB usable for GPU compute
```

**Windows limitation:** No `amdgpu.gttsize` equivalent. BIOS UMA setting
is the primary control. Some users report success with OLLAMA_GPU_MEMORY
override on Ollama 0.18+.

## Framework: Mistral-Small-4 Q4_K_M via TurboQuant

**Machine:** Framework laptop, 96GB RAM + 12GB VRAM (RTX 3060)

### Why TurboQuant is needed

Mistral-Small-4 is a 119B MoE (128 experts, 4 active, 6.5B active/token).
The Q4_K_M GGUF is 72.6GB — fits in 96GB RAM, but KV cache at FP16 adds
8-15GB+ at long contexts, pushing past the memory ceiling.

With TQ3 KV cache (~4.9x compression), KV drops to ~2-3GB, giving ~17GB
headroom for OS, CUDA, and inference buffers.

### Memory budget

| Component             | Without TQ | With TQ3 |
|-----------------------|------------|----------|
| Model weights (Q4_K_M)| 72.6 GB   | 72.6 GB  |
| KV cache (8K ctx)     | ~8 GB     | ~1.6 GB  |
| KV cache (32K ctx)    | ~15 GB    | ~3 GB    |
| OS + CUDA overhead    | ~5 GB     | ~5 GB    |
| **Total (32K ctx)**   | **~93 GB**| **~81 GB** |

### Preferred fork

Use `spiritbuun/llama-cpp-turboquant-cuda` — explicit CUDA kernel support,
275 stars, better fit for RTX 3060 than the Vulkan-oriented TheTom fork.

### Build steps (Framework)

```powershell
# Prerequisites: Visual Studio 2022 Build Tools + CMake + CUDA Toolkit
cd C:\Development
git clone https://github.com/spiritbuun/llama-cpp-turboquant-cuda.git
cd llama-cpp-turboquant-cuda
cmake -B build -DGGML_CUDA=ON
cmake --build build --config Release -j
```

### Download model

```bash
# bartowski Q4_K_M split files (~72.6GB total)
# Download from: https://huggingface.co/bartowski/mistralai_Mistral-Small-4-119B-2603-GGUF
# Place in a models directory, e.g. C:\Models\
```

### Run

```bash
./build/bin/llama-server \
  -m C:/Models/Mistral-Small-4-119B-2603-Q4_K_M.gguf \
  --cache-type-k tq3 --cache-type-v tq3 \
  -ngl 10 \
  --host 0.0.0.0 --port 8080
```

- `-ngl 10`: offload ~10 layers to RTX 3060 (12GB VRAM) for acceleration
- Bulk of model stays in system RAM
- Adjust `-ngl` up/down based on observed VRAM usage

### Fallback: without TurboQuant

If the TurboQuant fork has issues, standard llama-server with `q4_0` KV cache
is available now and gives ~4x compression (similar to TQ4):

```bash
llama-server \
  -m C:/Models/Mistral-Small-4-119B-2603-Q4_K_M.gguf \
  --cache-type-k q4_0 --cache-type-v q4_0 \
  -ngl 10 \
  --host 0.0.0.0 --port 8080
```

### Benchmark plan

1. Build spiritbuun/llama-cpp-turboquant-cuda with CUDA on Framework
2. Download Mistral-Small-4 Q4_K_M from bartowski
3. Run llama-server with TQ3 KV cache, -ngl 10
4. Quick quality check: 5-question suite
5. Throughput test: tok/s at 8K and 32K context
6. Compare vs Q3_K_M (54.6GB) to see if Q4_K_M quality uplift justifies the tighter fit
7. If TQ fork fails, fall back to standard llama-server with q4_0 KV cache

---

## Strix: Parallel Inference via TurboQuant

### Recommended Test Sequence

1. Try `OLLAMA_KV_CACHE_TYPE=q4_0` first (available now, ~4x compression)
2. Install VS 2022 Build Tools + CMake on Strix
3. Clone TheTom/llama-cpp-turboquant, build with Vulkan
4. Run llama-server with TQ3 KV cache
5. Benchmark with llama-benchy at concurrency 1-4
6. Compare vs Ollama q4_0 and Ollama default
