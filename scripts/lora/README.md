# LoRA Fine-Tuning: IQuest-Coder-V1-7B for C#/.NET

Fine-tune IQuest-Coder-V1-7B-Instruct to produce clean C#/.NET code output using
benchmark data from this repository. The training teaches the model to output raw C#
without markdown fences or explanatory prose.

## Hardware Requirements

- **GPU:** NVIDIA RTX 3060 12GB (or any GPU with >= 12GB VRAM)
- **RAM:** 32GB recommended (16GB minimum)
- **Disk:** ~30GB free (model weights + training checkpoints)
- **OS:** Linux (Ubuntu 22.04+ recommended)
- **CUDA:** 12.x with cuDNN

## Setup

```bash
# 1. Create a virtual environment
python3 -m venv venv
source venv/bin/activate

# 2. Install PyTorch (match your CUDA version)
pip install torch --index-url https://download.pytorch.org/whl/cu124

# 3. Install dependencies
pip install -r scripts/lora/requirements.txt

# 4. (Optional) Install unsloth for 2-3x faster training
pip install unsloth
# Note: IQuest-Coder-V1-7B uses a custom 'iquestcoder' architecture.
# If unsloth doesn't support it, the training script automatically falls back to PEFT+TRL.
```

## Step 1: Generate Training Data

```bash
python scripts/lora/generate_training_data.py
```

This reads:
- 20 task definitions from `scripts/coding_tasks/tasks/*.yaml`
- Reference API docs from `scripts/coding_tasks/references/*.md`
- High-quality C# outputs from top models in `results/coding-generated/`

Sources (in priority order):
1. `qwen3-coder-next` — primary reference (24 tasks, highest quality)
2. `RogerBen_qwen3.5-35b-opus-distill` — 24 tasks
3. `gpt-oss_120b` — 20 tasks
4. `mistral-small` — 20 tasks

Output: `scripts/lora/training_data.jsonl` (~70-80 examples in chat format)

## Step 2: Run Training

```bash
python scripts/lora/train_iquest_lora.py
```

### Training Configuration

| Parameter | Value | Notes |
|---|---|---|
| Quantization | 4-bit NF4 (QLoRA) | Fits in 12GB VRAM |
| LoRA rank | 16 | Good quality/VRAM tradeoff |
| LoRA alpha | 32 | 2x rank (standard) |
| Target modules | q,k,v,o_proj + gate,up,down_proj | All attention + MLP |
| Epochs | 3 | |
| Batch size | 1 | Limited by VRAM |
| Gradient accumulation | 4 | Effective batch = 4 |
| Learning rate | 2e-4 | With cosine schedule |
| Max sequence length | 4096 | Matches benchmark prompts |
| Optimizer | paged_adamw_8bit | Saves VRAM |

### Optional Arguments

```bash
python scripts/lora/train_iquest_lora.py \
  --epochs 5 \
  --lr 1e-4 \
  --lora-r 32 \
  --lora-alpha 64 \
  --skip-gguf         # skip GGUF conversion
  --skip-merge        # skip adapter merging (adapter-only output)
```

### Output

- **LoRA adapter:** `scripts/lora/output/iquest-7b-csharp-lora/`
- **Merged model:** `scripts/lora/output/iquest-7b-csharp-merged/`
- **GGUF file:** `scripts/lora/output/iquest-7b-csharp-gguf/iquest-7b-csharp-Q4_K_M.gguf`

## Step 3: Import to Ollama

After training and GGUF export:

```bash
cd scripts/lora/output/iquest-7b-csharp-gguf/
ollama create iquest-csharp -f Modelfile
```

Test it:

```bash
ollama run iquest-csharp "Create a C# record type for a Person with Name and Age properties"
```

### Manual GGUF Conversion

If automatic conversion fails (likely with the custom architecture), use llama.cpp directly:

```bash
git clone https://github.com/ggerganov/llama.cpp
cd llama.cpp && cmake -B build && cmake --build build --config Release
python convert_hf_to_gguf.py ../scripts/lora/output/iquest-7b-csharp-merged/ \
  --outfile ../scripts/lora/output/iquest-7b-csharp-gguf/f16.gguf --outtype f16
./build/bin/llama-quantize \
  ../scripts/lora/output/iquest-7b-csharp-gguf/f16.gguf \
  ../scripts/lora/output/iquest-7b-csharp-gguf/iquest-7b-csharp-Q4_K_M.gguf Q4_K_M
```

## Time Estimates (RTX 3060 12GB)

| Step | Time |
|---|---|
| Generate training data | < 5 seconds |
| Download model (first run) | 5-15 min (depends on connection) |
| Training (3 epochs, ~75 examples) | 15-30 minutes |
| Merge adapter | 5-10 minutes (runs on CPU) |
| GGUF export + quantize | 5-10 minutes |
| **Total** | **~30-60 minutes** |

## Architecture Notes

IQuest-Coder-V1-7B uses a custom `iquestcoder` architecture (7.6B parameters) with
`trust_remote_code=True` required for loading. The training script:

1. Tries **unsloth** first for speed (2-3x faster, lower VRAM usage)
2. If unsloth cannot handle the custom architecture, falls back to **PEFT + TRL**
3. Auto-detects the correct linear layer names for LoRA targeting
4. Uses `attn_implementation="eager"` as a safe default (switch to `flash_attention_2`
   if your setup supports it for faster training)

## Troubleshooting

**OOM errors:**
- Reduce `--max-seq-length 2048`
- Reduce `--lora-r 8 --lora-alpha 16`
- Ensure no other processes are using the GPU (`nvidia-smi`)

**unsloth fails to load:**
- Expected — the custom architecture may not be supported
- The script automatically falls back to PEFT+TRL

**GGUF export fails:**
- Use the manual llama.cpp conversion steps above
- Ensure llama.cpp is built with the model's architecture support
