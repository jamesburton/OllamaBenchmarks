# LoRA Fine-Tuning Research: Qwen3.5-9B for .NET/C# Code Generation

**Researched:** 2026-04-02
**Domain:** LoRA fine-tuning, GGUF export, consumer hardware constraints
**Confidence:** MEDIUM (Qwen3.5 is new; tooling is catching up with its novel architecture)

## Summary

Qwen3.5-9B uses a hybrid Gated DeltaNet + Gated Attention architecture (released March 2, 2026) that creates unique constraints for fine-tuning. The official Unsloth documentation **explicitly recommends against QLoRA (4-bit) for all Qwen3.5 models** due to "higher than normal quantization differences." The recommended approach is bf16 LoRA, which requires **22GB VRAM for the 9B model** -- exceeding the RTX 3060's 12GB.

This creates a hard constraint: the 9B model cannot be QLoRA'd reliably, and bf16 LoRA doesn't fit in 12GB VRAM. The practical options are: (A) drop to Qwen3.5-4B which fits comfortably, (B) use Unsloth's CPU offloading features to squeeze 9B bf16 LoRA into 12GB + system RAM, or (C) attempt QLoRA anyway accepting potential quality degradation.

**Primary recommendation:** Use Unsloth on the Framework Laptop (RTX 3060) with Qwen3.5-4B bf16 LoRA as the safe path. If 9B is essential, attempt bf16 LoRA with aggressive gradient checkpointing and CPU offloading first, falling back to QLoRA with quality validation.

---

## 1. Hardware Assessment

### Framework Laptop (RTX 3060 12GB eGPU)
| Property | Value |
|----------|-------|
| GPU VRAM | 12GB GDDR6 |
| System RAM | 96GB |
| CPU | Intel Core Ultra 7 155H |
| CUDA | Yes (Ampere) |
| Verdict | **Primary training machine.** CUDA support is mandatory for Unsloth. |

### Strix / Bosgame BeyondMax (Radeon 8060S)
| Property | Value |
|----------|-------|
| GPU | Radeon 8060S iGPU (~4GB allocatable) |
| Unified Memory | 128GB |
| ROCm Status | ROCm 6.4.4 basic support (gfx1151), incomplete hipBLASLt |
| Verdict | **Not viable for GPU training.** ROCm support is immature for this chip. CPU-only training theoretically possible but impractical (days/weeks for 9B). |

**Decision: Use Framework Laptop for all training.**

---

## 2. Qwen3.5-9B Architecture Implications

Qwen3.5-9B is **not a standard transformer.** Key differences:

| Property | Value |
|----------|-------|
| Parameters | 9B (dense) |
| Hidden dim | 4096 |
| Layers | 32 |
| Architecture | Hybrid: 8x (3x Gated DeltaNet + FFN, 1x Gated Attention + FFN) |
| Attention | ~75% linear attention (DeltaNet), ~25% softmax attention |
| Context | 262K native, extensible to 1M |
| Tokenizer vocab | 248,320 |

**Why this matters for fine-tuning:**
- Custom Mamba/DeltaNet Triton kernels compile slowly on first run
- QLoRA quantization introduces "higher than normal quantization differences" per Unsloth docs
- Requires `transformers v5` (not v4)
- One reported bug (issue #4188) shows unexpectedly high VRAM/CPU usage during Qwen3.5 training, even on RTX 5090 32GB

---

## 3. Tool Comparison

### Unsloth (RECOMMENDED)
| Property | Value |
|----------|-------|
| Qwen3.5 support | Yes -- dedicated fine-tuning guide and notebooks |
| Windows support | Yes -- native (Conda), Docker, or WSL |
| QLoRA 4-bit | Supported but **officially not recommended** for Qwen3.5 |
| bf16 LoRA VRAM (9B) | ~22GB |
| CPU offloading | Yes -- paged optimizers, gradient checkpointing to system RAM (~1.9% overhead) |
| GGUF export | Built-in `save_pretrained_gguf()` with q4_k_m, q8_0, f16 |
| Ollama export | Built-in `save_pretrained_ollama()` with auto-generated Modelfile |
| Multi-GPU | No (single GPU only) |
| Speed | 1.5-2x faster than standard HuggingFace + FlashAttention2 |

**Strengths:** Best single-GPU efficiency, built-in GGUF/Ollama export, active Qwen3.5 support.
**Risks:** Qwen3.5 support is new (March 2026); some users report unexpectedly high resource usage.

### Axolotl
| Property | Value |
|----------|-------|
| Qwen3.5 support | Likely (wraps HuggingFace), not explicitly documented |
| Windows support | WSL/Docker only |
| Best for | Multi-GPU, RLHF pipelines, complex training configs |
| GGUF export | Manual (merge adapter, then convert separately) |

**Verdict:** Overkill for single-GPU LoRA. Adds complexity without benefit here. Axolotl can use Unsloth as a backend anyway.

### PEFT + Transformers (Direct)
| Property | Value |
|----------|-------|
| Qwen3.5 support | Yes (requires transformers v5) |
| Windows support | Yes |
| Best for | Maximum control, custom training loops |
| GGUF export | Manual (merge, then llama.cpp convert_hf_to_gguf.py) |

**Verdict:** Viable fallback if Unsloth has bugs. More manual work, no built-in optimizations.

### llama.cpp finetune
| Property | Value |
|----------|-------|
| Status | **BROKEN** for modern architectures (issue #18805) |
| Qwen3.5 support | No -- DeltaNet not supported in finetune binary |

**Verdict:** Not viable. Inference-only tool with a broken finetune feature.

### LLaMA-Factory (CPU mode)
| Property | Value |
|----------|-------|
| CPU training | Yes, documented for LoRA |
| Qwen3.5 support | Qwen3.5 docs "to be updated" -- uncertain |

**Verdict:** Possible CPU fallback on Strix, but training time would be days/weeks. Not practical for iteration.

---

## 4. VRAM Budget Analysis for RTX 3060 (12GB)

### Option A: Qwen3.5-4B bf16 LoRA (SAFE PATH)
| Component | VRAM |
|-----------|------|
| Model weights (bf16) | ~8GB |
| LoRA adapters | ~0.5GB |
| Optimizer states | ~1GB |
| Activations (gradient checkpointing) | ~1-2GB |
| **Total** | **~10-11GB** |
| Fits in 12GB? | **Yes, comfortably** |

### Option B: Qwen3.5-9B bf16 LoRA + CPU offloading (STRETCH)
| Component | VRAM |
|-----------|------|
| Model weights (bf16) | ~18GB |
| With CPU offloading | ~10-12GB on GPU, rest on system RAM |
| LoRA adapters | ~0.5GB |
| Optimizer states (paged, CPU) | ~0GB GPU (offloaded) |
| Activations (gradient checkpointing) | ~1GB |
| **Total GPU** | **~12-13GB** |
| Fits in 12GB? | **Tight -- may OOM** |

### Option C: Qwen3.5-9B QLoRA 4-bit (NOT RECOMMENDED by Unsloth)
| Component | VRAM |
|-----------|------|
| Model weights (4-bit) | ~5GB |
| LoRA adapters | ~0.5GB |
| Optimizer states | ~1GB |
| Activations | ~1-2GB |
| **Total** | **~7-9GB** |
| Fits in 12GB? | **Yes** |
| Quality risk | **HIGH -- Unsloth warns of "higher than normal quantization differences"** |

---

## 5. Recommended Strategy

### Primary: Qwen3.5-4B bf16 LoRA on Framework (RTX 3060)

This is the reliable path. The 4B model:
- Fits comfortably in 12GB VRAM with bf16 LoRA
- Scores 74.0 on math reasoning (decent coding capability)
- Is part of the same Qwen3.5 architecture family
- Has clean GGUF export path via Unsloth

### If 9B is critical: Tiered fallback

1. **Try bf16 LoRA + aggressive offloading** on Framework:
   - `gradient_checkpointing = "unsloth"` (offloads activations to CPU RAM)
   - Paged AdamW 8-bit optimizer (offloads optimizer states to CPU)
   - `per_device_train_batch_size = 1`, `gradient_accumulation_steps = 4`
   - Max sequence length: 1024-2048 (not full 262K)
   - If it fits: best quality, ~2-4 hours for 1000 examples

2. **If OOM: Try QLoRA 4-bit anyway** with quality validation:
   - Train with QLoRA, then benchmark output quality
   - Compare against base model on your .NET/C# test cases
   - The "quantization differences" warning may not matter for code generation (which is more structured than creative text)

3. **If quality is unacceptable: Fall back to 4B bf16 LoRA**

---

## 6. GGUF Export Pipeline

### Via Unsloth (recommended)

```python
# After training, merge LoRA and export to GGUF
model.save_pretrained_gguf(
    "qwen35-9b-dotnet",
    tokenizer,
    quantization_method="q4_k_m"  # or q8_0 for higher quality
)

# Or export directly for Ollama
model.save_pretrained_ollama(
    "qwen35-9b-dotnet",
    tokenizer,
    quantization_method="q4_k_m"
)
```

### Known issue (RESOLVED)

There was a bug in llama.cpp `convert_lora_to_gguf.py` for Qwen3.5 LoRA adapters (issue #21125 -- `LoraTorchTensor.reshape()` NotImplementedError during V head reordering). **Fixed in PR #21354** -- merged into llama.cpp main. Ensure you use latest llama.cpp if doing manual conversion.

### Manual fallback

```bash
# 1. Merge LoRA adapter into base model
python merge_lora.py --base Qwen/Qwen3.5-9B --adapter ./lora_output --output ./merged

# 2. Convert to GGUF
python convert_hf_to_gguf.py ./merged --outfile qwen35-9b-dotnet.gguf --outtype q4_k_m

# 3. Create Ollama Modelfile
echo 'FROM ./qwen35-9b-dotnet.gguf
TEMPLATE "{{ .System }}\n{{ .Prompt }}"
PARAMETER stop "<|endoftext|>"' > Modelfile

# 4. Import into Ollama
ollama create qwen35-9b-dotnet -f Modelfile
```

---

## 7. Installation (Windows 11, Framework Laptop)

### Prerequisites
- NVIDIA drivers installed (`nvidia-smi` works)
- Python 3.12 (not 3.13+)
- Visual Studio Build Tools with C++ and Windows SDK

### Setup

```bash
# Create conda environment
conda create --name lora python=3.12 -y
conda activate lora

# Install PyTorch with CUDA (check your CUDA version via nvidia-smi)
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu124

# Install Unsloth
pip install unsloth

# Install transformers v5 (required for Qwen3.5)
pip install "transformers>=5.0"

# Verify
python -c "import unsloth; print('Unsloth OK')"
python -c "import torch; print(f'CUDA: {torch.cuda.is_available()}, VRAM: {torch.cuda.get_device_properties(0).total_mem / 1e9:.1f}GB')"
```

---

## 8. Training Configuration Template

```python
from unsloth import FastLanguageModel
from trl import SFTTrainer
from transformers import TrainingArguments
from datasets import load_dataset

# Load model with LoRA
model, tokenizer = FastLanguageModel.from_pretrained(
    model_name="unsloth/Qwen3.5-4B",  # or Qwen3.5-9B
    max_seq_length=2048,
    dtype=None,  # auto-detect
    load_in_4bit=False,  # bf16 LoRA, NOT QLoRA
)

model = FastLanguageModel.get_peft_model(
    model,
    r=16,               # LoRA rank (16 is good default for code)
    target_modules=["q_proj", "k_proj", "v_proj", "o_proj",
                     "gate_proj", "up_proj", "down_proj"],
    lora_alpha=16,
    lora_dropout=0,      # Unsloth optimized = 0
    bias="none",
    use_gradient_checkpointing="unsloth",  # CPU offload activations
    random_state=42,
)

# Training arguments
training_args = TrainingArguments(
    per_device_train_batch_size=2,     # reduce to 1 if OOM
    gradient_accumulation_steps=4,
    warmup_steps=5,
    num_train_epochs=3,                # 3 epochs for 1000 examples
    learning_rate=2e-4,
    fp16=not torch.cuda.is_bf16_supported(),
    bf16=torch.cuda.is_bf16_supported(),
    logging_steps=1,
    optim="adamw_8bit",                # paged optimizer, offloads to CPU
    output_dir="outputs",
    save_strategy="epoch",
)

# Train
trainer = SFTTrainer(
    model=model,
    tokenizer=tokenizer,
    train_dataset=dataset,
    args=training_args,
    max_seq_length=2048,
)
trainer.train()

# Export
model.save_pretrained_gguf("qwen35-dotnet", tokenizer, quantization_method="q4_k_m")
```

---

## 9. Dataset Format for .NET/C# Code Generation

For 1,000 examples, use the standard chat/instruction format:

```json
{
  "conversations": [
    {"role": "system", "content": "You are a .NET/C# expert. Generate clean, idiomatic C# code."},
    {"role": "user", "content": "Write a method that reads a CSV file and returns a list of records using CsvHelper."},
    {"role": "assistant", "content": "```csharp\nusing CsvHelper;\nusing System.Globalization;\n...\n```"}
  ]
}
```

**Tips:**
- Use the **same chat template** during training and inference (Qwen3.5 uses ChatML-style)
- Include system prompts in training data
- Keep examples under 2048 tokens each
- Mix difficulty levels: simple methods, class design, async patterns, LINQ, etc.

---

## 10. Strix Assessment (128GB Unified Memory)

### Can it help?

| Approach | Feasibility | Training Time | Quality |
|----------|-------------|---------------|---------|
| Unsloth GPU (ROCm) | No -- ROCm too immature for Radeon 8060S training | N/A | N/A |
| LLaMA-Factory CPU-only | Technically possible | Days to weeks | Same as GPU |
| Unsloth CPU mode | Not a documented mode | N/A | N/A |

**Verdict:** The Strix's 128GB is tantalizing but unusable for practical fine-tuning. There is no well-supported CPU-only LoRA training path for Qwen3.5's DeltaNet architecture. The Framework with its RTX 3060 is the only viable machine.

The Strix could theoretically run LLaMA-Factory in CPU-only mode, but training 1,000 examples on a 9B model would take many days -- not practical for iterating on dataset/hyperparameter choices.

---

## 11. Common Pitfalls

### Pitfall 1: Using QLoRA on Qwen3.5
**What goes wrong:** Degraded output quality due to DeltaNet layers quantizing poorly.
**How to avoid:** Use bf16 LoRA. If VRAM-constrained, try 4B model instead.

### Pitfall 2: Wrong transformers version
**What goes wrong:** Model fails to load or produces errors.
**How to avoid:** Must use `transformers >= 5.0` for Qwen3.5.

### Pitfall 3: Wrong chat template in Ollama
**What goes wrong:** Model generates garbage or ignores instructions after deployment.
**How to avoid:** Use Unsloth's `save_pretrained_ollama()` which auto-generates the correct Modelfile with matching chat template.

### Pitfall 4: Sequence length too long
**What goes wrong:** OOM on 12GB GPU even with small batch size.
**How to avoid:** Cap `max_seq_length` at 2048 for training. Code examples rarely need more.

### Pitfall 5: First training run compiles slowly
**What goes wrong:** Qwen3.5's custom Triton kernels take 5-15 minutes to compile on first run.
**How to avoid:** This is expected. Subsequent runs use cached kernels.

---

## 12. Decision Matrix

| Factor | Qwen3.5-4B bf16 LoRA | Qwen3.5-9B bf16+offload | Qwen3.5-9B QLoRA |
|--------|----------------------|------------------------|-------------------|
| Fits 12GB VRAM | Yes | Maybe (tight) | Yes |
| Quality | Good | Best | Uncertain (warned against) |
| Training time (~1K examples) | ~1-2 hours | ~2-4 hours | ~1-2 hours |
| GGUF export | Clean | Clean | Clean |
| Risk level | Low | Medium (OOM risk) | High (quality risk) |
| **Recommendation** | **Start here** | Try second | Last resort |

---

## Sources

### Primary (HIGH confidence)
- [Unsloth Qwen3.5 Fine-tuning Guide](https://unsloth.ai/docs/models/qwen3.5/fine-tune) -- VRAM requirements, QLoRA warning
- [Unsloth Windows Installation](https://unsloth.ai/docs/get-started/install/windows-installation) -- setup steps
- [Unsloth GGUF Export](https://unsloth.ai/docs/basics/inference-and-deployment/saving-to-gguf) -- export methods
- [Unsloth Ollama Export](https://unsloth.ai/docs/basics/inference-and-deployment/saving-to-ollama) -- Ollama integration
- [Qwen3.5-9B HuggingFace](https://huggingface.co/Qwen/Qwen3.5-9B) -- model architecture specs

### Secondary (MEDIUM confidence)
- [llama.cpp issue #21125](https://github.com/ggml-org/llama.cpp/issues/21125) -- Qwen3.5 LoRA GGUF conversion bug (RESOLVED via PR #21354)
- [Unsloth issue #4188](https://github.com/unslothai/unsloth/issues/4188) -- High VRAM/CPU usage report for Qwen3.5
- [Unsloth Gradient Checkpointing](https://unsloth.ai/blog/long-context) -- CPU offloading details
- [Spheron: Axolotl vs Unsloth vs TorchTune 2026](https://www.spheron.network/blog/axolotl-vs-unsloth-vs-torchtune/) -- framework comparison
- [ROCm Radeon 8060S](https://medium.com/@GenerationAI/ultralytics-yolo-sam-with-rocm-7-0-on-amd-ryzen-ai-max-395-strix-halo-radeon-8060s-gfx1151-6f48bb9bcbf9) -- ROCm status on Strix

### Tertiary (LOW confidence)
- [llama.cpp finetune issue #18805](https://github.com/ggml-org/llama.cpp/issues/18805) -- llama-finetune broken on modern architectures
- VRAM estimates for 4B model are extrapolated from 9B figures (not directly measured)
