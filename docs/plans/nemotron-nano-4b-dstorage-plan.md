# Nemotron-3-Nano:4b + DirectStorage Optimisation Plan

**Date:** 2026-04-01
**Machine:** Framework (96GB RAM, RTX 3060 12GB, Crucial CT4000P3 4TB NVMe PCIe 3.0 x4)

---

## Model Overview

`nemotron-3-nano:4b` is the 4B active-parameter variant of the 30B Nemotron-3-Nano MoE:

- **Architecture:** Hybrid Mamba-Transformer MoE (30B total, 3.2B active/token)
- **VRAM footprint:** ~3 GB at Q4 — fits entirely in 12GB RTX 3060
- **Context window:** 256K tokens
- **Ollama size:** 2.8 GB
- **Expected quality:** High for its class — 30B Nano scored 5/5 on our benchmark suite

Because the full model fits in VRAM, **expert selection overhead is minimal** — the GPU holds all
expert weights hot. This makes it fundamentally different from large MoE models (nemotron-3-nano:30b,
mistral-small-4) where only active experts can be resident at once.

---

## Phase 1: Baseline Benchmarks (Ollama)

Run the standard suite against the freshly-pulled model:

```bash
python ./scripts/benchmark_quality.py --models "nemotron-3-nano:4b"
powershell -File ./scripts/benchmark_throughput_resource.ps1 -Models "nemotron-3-nano:4b"
```

Also measure cold-load time (first inference after Ollama restart):

```bash
ollama stop; sleep 2
time curl -s http://127.0.0.1:11434/api/generate \
  -d '{"model":"nemotron-3-nano:4b","prompt":"Hello","stream":false,"options":{"num_predict":1}}' \
  | python -c "import sys,json; d=json.load(sys.stdin); print(f'load: {d[\"load_duration\"]/1e9:.2f}s')"
```

---

## Phase 2: dstorage_gpu Integration

### Why it's relevant

`dstorage_gpu` (github.com/jamesburton/dstorage_gpu) achieves 11.7 GB/s NVMe→GPU vs 0.73 GB/s
via torch.load on this exact machine (RTX 3060, Crucial NVMe PCIe 3.0). Ollama's cold-load path
goes: NVMe → OS page cache → CPU RAM → cudaMemcpy H2D → GPU.

For a 2.8 GB model, the traditional path takes ~3.8s cold-load vs ~0.24s with DirectStorage
(theoretical — actual will be higher due to Ollama overhead).

### Integration options

#### Option A: GGUF preload script (minimal invasiveness)
Extract GGUF tensor data, pre-warm the OS/GPU before Ollama starts:

```python
# preload_nemotron.py — pre-stage weights in GPU memory via dstorage_gpu
# so Ollama's mmap picks them up from hot cache
from dstorage_gpu import DirectStorageLoader
import glob

loader = DirectStorageLoader()
gguf_path = r"C:\Users\james\.ollama\models\blobs\sha256-527db2cf6c70..."
specs = [(gguf_path, file_size_bytes // 4)]
loader.load_tensors(specs)
# GPU now has weights; Ollama cudaMemcpy will find them in OS cache
```

**Limitation:** Ollama uses mmap, not direct torch loading — the GPU tensor from dstorage_gpu isn't
directly accessible to Ollama. This warms the OS page cache but doesn't bypass the CPU hop.

#### Option B: Custom llama-server inference script
Build a Python inference layer that:
1. Loads GGUF weight tensors via dstorage_gpu (NVMe→GPU, 11.7 GB/s)
2. Converts to llama-cpp-python compatible format
3. Passes pre-loaded tensors to llama.cpp via `llama_model_load_from_tensors` (if exposed)

**Complexity:** High — requires llama-cpp-python API integration.

#### Option C: llama-server with `--no-mmap` + pre-staged memory
llama-server's `--no-mmap` flag forces a single contiguous GPU allocation rather than mmap.
Use dstorage_gpu to pre-populate a pinned buffer, then `cudaIpcGetMemHandle` to share with
llama-server process — complex but theoretically possible on Windows.

#### Option D: MoE expert streaming (30B model only)
For the **30B variant** (24GB model, too large for 12GB VRAM), dstorage_gpu could stream
expert blocks directly from NVMe to GPU per-token, bypassing the CPU entirely:

```python
# Expert loading loop (conceptual):
for token in tokens:
    active_experts = router(token)  # determine which 4 of 128 fire
    expert_tensors = loader.load_tensors([
        (f"experts/{e}.bin", expert_size) for e in active_experts
    ])
    output = compute(token, expert_tensors)
```

At 11.7 GB/s and ~500MB per expert block, loading 4 experts takes ~170ms — comparable to CPU
inference at 2-3 tok/s. Needs profiling to see if it beats the current llama.cpp MoE CPU path.

---

## Phase 3: TurboQuant KV Cache (Long Context)

The 256K context window is the killer feature of nemotron-3-nano:4b. At long contexts the KV
cache dominates memory:

| Context | KV (FP16) | KV (TQ3) | Remaining VRAM (of 12GB) |
|---------|-----------|----------|--------------------------|
| 8K      | ~0.3 GB   | ~60 MB   | 11.7 GB / 11.97 GB       |
| 64K     | ~2.4 GB   | ~490 MB  | 9.6 GB / 11.5 GB         |
| 128K    | ~4.8 GB   | ~980 MB  | 7.2 GB / 11.0 GB         |
| 256K    | ~9.6 GB   | ~1.96 GB | 2.4 GB / 10.0 GB         |

**Without TurboQuant:** 256K context uses 9.6 GB KV, plus 3 GB weights = 12.6 GB → exceeds 12GB VRAM.
**With TQ3:** 1.96 GB KV + 3 GB weights = 4.96 GB → 256K context fits comfortably!

This means TurboQuant is **required** to actually use the full 256K context on 12GB.

### Build plan for TQ3 on this model

```bash
# 1. Build spiritbuun/llama-cpp-turboquant-cuda (CUDA fork)
git clone https://github.com/spiritbuun/llama-cpp-turboquant-cuda.git
cd llama-cpp-turboquant-cuda
cmake -B build -DGGML_CUDA=ON -DCMAKE_BUILD_TYPE=Release
cmake --build build -j

# 2. Find the GGUF blob path
ollama show nemotron-3-nano:4b --modelfile | grep FROM

# 3. Run with TQ3 KV and full 256K context
./build/bin/llama-server \
  -m C:/Users/james/.ollama/models/blobs/sha256-527db2cf6c70... \
  --cache-type-k tq3 --cache-type-v tq3 \
  -c 262144 \
  --host 0.0.0.0 --port 8080
```

---

## Phase 4: dstorage_gpu + Expert Streaming Prototype (30B)

If the 4B baseline looks promising and we want to push to the 30B:

1. Install dstorage_gpu: `pip install dstorage-gpu`
2. Extract GGUF expert blocks to individual .bin files (write a tool)
3. Build a thin Python inference loop:
   - Attention layers: loaded once to GPU at startup via dstorage_gpu
   - Expert blocks: streamed per-token from NVMe

Expected gains vs CPU inference:
- CPU MoE path: ~2-4 tok/s (expert weights in RAM, PCIe transfer per token)
- dstorage_gpu path: theoretically 8-15 tok/s (11.7 GB/s NVMe→GPU direct)
- Requires profiling — PCIe latency and CUDA kernel launch overhead matter

---

## Recommended Test Sequence

1. **[Now]** Run baseline quality + throughput for `nemotron-3-nano:4b`
2. **[Now]** Measure cold-load time (Ollama default)
3. **[Next]** Build spiritbuun CUDA fork, test TQ3 at 8K/64K/128K/256K context
4. **[Next]** Profile: does the 256K context fit with TQ3? What's tok/s at long context?
5. **[Later]** Prototype dstorage_gpu preload script, measure cold-load improvement
6. **[Later]** Evaluate whether 30B MoE expert streaming is worth the engineering effort

---

## Key Decision Points

| Question | Answer |
|----------|--------|
| Quality score on 4b? | **5/5** (coding 2/2, tool 2/2, agentic 1/1) |
| Tok/s GPU on 4b? | **84.3 tok/s** — fastest on Framework, beats cogito:8b (61.6) |
| Cold-load time (Ollama)? | **0.41s** — already very fast; dstorage benefit is marginal here |
| Max context without TQ? | ~200K (estimated — 9.6GB KV at 256K pushes past 12GB total) |
| Max context with TQ3? | 256K theoretical (1.96 GB KV + 3 GB weights = 4.96 GB) |
| dstorage preload helps? | Minimal for 4b (0.41s load already fast). High value for 30B. |
| 30B expert streaming viable? | TBD — requires prototype |

## Phase 1 Results Summary (2026-04-01)

**nemotron-3-nano:4b is now the Framework GPU champion:**
- 5/5 quality from a 2.8GB model at 84.3 tok/s
- Beats cogito:8b on both speed (84.3 vs 61.6) and VRAM (3GB vs 4.9GB)
- 9GB VRAM headroom for KV cache — can serve very long contexts
- 256K context window makes TurboQuant the key next step

**dstorage_gpu assessment for 4b:**
- Cold load is already 0.41s — DirectStorage overhead (~50ms setup) would give marginal gain
- dstorage_gpu shines for the **30B variant** where expert streaming is the bottleneck
- Primary value: prototype for 30B MoE expert streaming from NVMe

**Next priority: TurboQuant for 256K context**
Build spiritbuun/llama-cpp-turboquant-cuda and test nemotron-3-nano:4b at 64K/128K/256K context.
Without TQ, 256K context requires 9.6GB KV + 3GB weights = 12.6GB → barely exceeds 12GB VRAM.
With TQ3, same context needs 1.96GB KV → comfortably fits with 7GB to spare.
