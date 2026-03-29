# LoRA Fine-Tuning Plan: IQuest-Coder-V1-7B for .NET/C# Benchmark

**Date:** 2026-03-29
**Target model:** IQuestLab/IQuest-Coder-V1-7B-Instruct (7.6B params, custom `iquestcoder` architecture)
**Goal:** Improve benchmark score from 2/5 to 4+/5 by teaching the model our exact output format

---

## 1. Problem Analysis

### Current Failure Mode
The IQuest-Coder-V1-14B scores 1/5 and the 7B scores 2/5. The 14B's coding layer3
results show **every single task fails with "Empty code after extraction"** — the model
generates output but not in a format the code extractor recognises.

Our `code_extractor.py` expects either:
- Fenced code blocks (` ```csharp ` / ` ```cs ` / ` ``` `)
- Or raw text starting with C# tokens (`using `, `public `, `class `, `record `, etc.)

The model likely wraps its output in thinking tokens, extra prose, or a non-standard
format that the extractor cannot parse. This is a **format problem**, not a knowledge
problem — the model may already know C#, it just doesn't produce clean output.

### Target Output Format (from qwen3-coder-next reference)
- Single C# file, no namespace wrapping
- Starts with `using` statements
- Clean records/classes/interfaces at top level
- No markdown fences in the ideal case (the extractor handles them, but raw code is best)
- Concise: 20-80 lines for simple tasks, up to ~275 for complex vertical-slice tasks

---

## 2. Local Hardware Assessment

| Resource | Value | Notes |
|----------|-------|-------|
| GPU | AMD Radeon 8060S | ~4 GB VRAM, RDNA 4, **no CUDA** |
| CPU | AMD Ryzen (Strix Point) | 32 logical cores |
| RAM | 63.65 GB | Plenty for CPU-based workflows |
| PyTorch CUDA | Not available | `torch.cuda.is_available() = False` |
| Python | 3.14.3 | |
| Installed ML libs | transformers 5.3.0, accelerate 1.13.0, datasets 4.6.1 | |
| Missing | unsloth, peft, bitsandbytes, trl, axolotl | None of the LoRA training stack |

**Critical constraint:** No NVIDIA GPU. The Radeon 8060S has only 4 GB VRAM and no
CUDA support. Training locally is not viable for the primary path. ROCm support on
Windows for RDNA 4 is experimental at best.

---

## 3. Training Data Generation

### 3a. Existing Data: Prompt-Completion Pairs from Benchmark

We have 20 coding tasks (YAML files in `scripts/coding_tasks/tasks/`) and reference
outputs from the best-performing model (`qwen3-coder-next`, 24 files). This gives us
~20 high-quality prompt/completion pairs immediately.

**Extraction procedure:**
1. For each `.yaml` task file, extract the `prompt` field (with `{references}`
   expanded from `scripts/coding_tasks/references/*.md`)
2. Pair it with the corresponding `.cs` file from
   `results/coding-generated/qwen3-coder-next/`
3. Format as chat-style training examples:
   - System: "You are a C# code generator. Return only valid C# code in a single file.
     Do not wrap code in markdown fences or add explanatory text."
   - User: (the prompt)
   - Assistant: (the raw C# code)

### 3b. Augmenting with More Models' Outputs

Multiple models have generated outputs (up to 24 files each). We can harvest passing
outputs from other models to get variant completions for the same prompts — this
increases diversity. Good candidates:
- `qwen3-coder_30b` (20 files)
- `qwen3_14b` (22 files)
- `nemotron-3-super` (20 files)
- `granite4_32b-a9b-h` (20 files)

**Important:** Only use outputs from tasks that actually *passed* their tests. Cross-
reference with quality/coding result JSONs to filter.

### 3c. Synthetic Augmentation

20-40 examples is far too few for meaningful fine-tuning. Augment to 200-500 examples:

1. **Prompt variations:** For each existing task, create 3-5 paraphrased versions of the
   prompt with the same expected output (vary wording, reorder requirements, add/remove
   detail).

2. **New C# tasks from public datasets:**
   - Use exercises from dotnet/roslyn samples, Microsoft Learn tutorials
   - Convert HumanEval/MBPP problems to C# equivalents
   - Create simple CRUD, LINQ, async/await, DI, and EF Core examples
   - Each must follow our format: system prompt + user prompt -> raw C# code

3. **Negative examples (optional, for DPO):** Pair good outputs with bad outputs
   (wrong format — markdown-wrapped, namespaced, with prose) to teach format
   preferences.

### 3d. Dataset Format

Use the standard chat/conversation JSONL format:

```json
{
  "conversations": [
    {"role": "system", "content": "You are a C# code generator. ..."},
    {"role": "user", "content": "Generate a single C# file containing..."},
    {"role": "assistant", "content": "using System;\nusing System.Linq;\n\npublic record Sale..."}
  ]
}
```

---

## 4. Training Setup

### 4a. Framework: Unsloth (Preferred)

[Unsloth](https://github.com/unslothai/unsloth) is the recommended framework for
speed and memory efficiency. However:

- **Architecture concern:** IQuest-Coder uses a custom `iquestcoder` architecture.
  Unsloth supports a fixed set of architectures (Llama, Mistral, Qwen, Gemma, Phi,
  etc.). If `iquestcoder` is not a supported architecture, Unsloth will not work.
  - **Mitigation:** Check whether the architecture is a renamed/modified version of a
    supported base (likely Qwen2 or DeepSeek-Coder based, given the naming and param
    count). If so, it may work with minor patching.
  - **Fallback:** Use HuggingFace PEFT + TRL directly (already have transformers and
    accelerate installed).

### 4b. Where to Train

Given the hardware constraints (no NVIDIA GPU locally), training must happen elsewhere:

| Option | VRAM | Cost | Notes |
|--------|------|------|-------|
| **Google Colab Free** | T4 16GB | Free | Enough for 7B QLoRA. Session limits. |
| **Google Colab Pro** | A100 40GB | ~$10/mo | Comfortable for 7B, fast. |
| **RunPod / Vast.ai** | A100 80GB | ~$1-2/hr | Best for speed; 1-2 hours total. |
| **Lambda / Modal** | Various | Pay-per-use | Good for scripted runs. |
| **Kaggle Notebooks** | T4 16GB x2 | Free | 30 hr/week GPU quota. |
| **Locally via CPU** | N/A | Free | Possible with PEFT but extremely slow (days). Not recommended. |

**Recommendation:** Use Google Colab (free T4) for initial experiments, upgrade to
Colab Pro or RunPod A100 for the actual training run.

### 4c. LoRA Hyperparameters

For a 7B model with a small dataset (~200-500 examples):

| Parameter | Value | Rationale |
|-----------|-------|-----------|
| LoRA rank (r) | 16 | Small dataset; higher rank risks overfitting |
| LoRA alpha | 32 | Standard 2x rank ratio |
| Target modules | q_proj, k_proj, v_proj, o_proj, gate_proj, up_proj, down_proj | All attention + MLP for format learning |
| Dropout | 0.05 | Light regularization |
| Learning rate | 2e-4 | Standard for QLoRA |
| Batch size | 4 (effective, via gradient accumulation) | Fits T4 16GB |
| Epochs | 3-5 | Small dataset needs multiple passes |
| Max seq length | 2048 | Longest reference output is ~275 lines; 2048 tokens is sufficient |
| Quantization | 4-bit (QLoRA via bitsandbytes) | Required for T4 16GB with 7B model |
| Warmup | 10% of steps | Standard |
| Scheduler | Cosine | Standard |
| Weight decay | 0.01 | Light regularization |

### 4d. Alternative: Format-Only Fine-Tuning (Smaller Scope)

Since the core issue may be purely format (model knows C# but wraps it wrong), a
lighter approach could work:

- LoRA rank 8 instead of 16
- Only 50-100 examples focused on format
- 1-2 epochs
- Target only attention layers (q/k/v/o_proj)

This would be faster to iterate on and less likely to degrade the model's existing
C# knowledge.

---

## 5. Training Procedure

### Step 1: Prepare Environment (Cloud)

```bash
pip install unsloth peft trl bitsandbytes datasets
# or if unsloth doesn't support the architecture:
pip install peft trl bitsandbytes datasets accelerate transformers
```

### Step 2: Prepare Dataset

Run a local script (this machine) to:
1. Parse all 20 YAML task files, expand `{references}`
2. Pair with passing `.cs` outputs from qwen3-coder-next and other models
3. Add synthetic augmentation examples
4. Export as `train.jsonl` and `eval.jsonl` (90/10 split)
5. Upload to HuggingFace datasets or include in the Colab notebook

### Step 3: Fine-Tune

```python
from unsloth import FastLanguageModel  # or use transformers + peft directly
from trl import SFTTrainer

model, tokenizer = FastLanguageModel.from_pretrained(
    "IQuestLab/IQuest-Coder-V1-7B-Instruct",
    max_seq_length=2048,
    load_in_4bit=True,
)

model = FastLanguageModel.get_peft_model(
    model, r=16, lora_alpha=32,
    target_modules=["q_proj", "k_proj", "v_proj", "o_proj",
                     "gate_proj", "up_proj", "down_proj"],
    lora_dropout=0.05,
)

trainer = SFTTrainer(
    model=model,
    train_dataset=train_dataset,
    # ... training args as per table above
)
trainer.train()
```

**Expected training time:**
- T4 16GB: ~30-60 minutes for 500 examples x 3 epochs
- A100 40GB: ~10-20 minutes

### Step 4: Merge and Export to GGUF

```python
# Merge LoRA weights into base model
model.save_pretrained_merged("merged_model", tokenizer)

# Convert to GGUF using llama.cpp
# Option A: Use unsloth's built-in export
model.save_pretrained_gguf("gguf_output", tokenizer, quantization_method="q4_k_m")

# Option B: Manual llama.cpp conversion
# python convert_hf_to_gguf.py merged_model --outtype q4_k_m --outfile iquest-coder-7b-csharp.gguf
```

### Step 5: Deploy to Ollama

```bash
# Create Modelfile
cat > Modelfile <<EOF
FROM ./iquest-coder-7b-csharp-Q4_K_M.gguf
TEMPLATE "{{ .System }}\n{{ .Prompt }}"
PARAMETER num_ctx 12288
SYSTEM "You are a C# code generator. Return only valid C# code. Do not use markdown fences or add explanatory text."
EOF

ollama create iquest-coder-7b-csharp -f Modelfile
```

---

## 6. Evaluation

### 6a. Quick Smoke Test
Run the `_smoke_test.yaml` task against the LoRA model to verify basic format
compliance before the full benchmark.

### 6b. Full Benchmark
Run the complete benchmark suite against the fine-tuned model:

```bash
python scripts/run_new_models.py --model iquest-coder-7b-csharp
```

### 6c. Success Criteria

| Metric | Baseline (7B) | Target | Stretch |
|--------|---------------|--------|---------|
| Quality score | 2/5 | 4/5 | 5/5 |
| Coding tasks pass | ~0/20 (extraction fails) | 12+/20 | 16+/20 |
| Format compliance | 0% (empty extraction) | 95%+ | 100% |

### 6d. Iteration
If the first LoRA doesn't hit targets:
1. Analyze which tasks still fail — format issue or knowledge gap?
2. Add targeted training examples for failing categories
3. Adjust LoRA rank/epochs and retrain (each iteration ~30-60 min on T4)

---

## 7. Risks and Mitigations

### R1: Custom Architecture Not Supported by Unsloth
**Risk:** HIGH. The `iquestcoder` architecture tag is custom. Unsloth may refuse to
load it.
**Mitigation:** Fall back to raw PEFT + TRL (transformers). This is slower but
architecture-agnostic as long as transformers supports it (it does — the model has
`custom_code` and uses `AutoModelForCausalLM`). Alternatively, check if the
architecture is actually Qwen2-based under a different name.

### R2: Training Data Too Small
**Risk:** MEDIUM. 20 ground-truth examples is very thin. Even with augmentation to
200-500, overfitting is possible.
**Mitigation:** Use low LoRA rank (8-16), early stopping, and eval split. Focus
training on format compliance rather than C# knowledge. Monitor eval loss.

### R3: LoRA Degrades Existing C# Ability
**Risk:** MEDIUM. Fine-tuning on format could inadvertently hurt the model's coding
capability.
**Mitigation:** Keep LoRA rank low. Use format-focused examples (raw code output
without markdown) rather than trying to teach new C# patterns. Test on HumanEval-C#
or similar before and after.

### R4: GGUF Conversion Fails for Custom Architecture
**Risk:** MEDIUM. `llama.cpp`'s `convert_hf_to_gguf.py` may not support the custom
architecture.
**Mitigation:** Check if there's already a working GGUF conversion (there is — the
`xhxlb/IQuest-Coder-V1-7B-Instruct-GGUF` repo exists and Ollama loads it). The LoRA
merge produces a standard HF model, so the same conversion path should work.

### R5: No Local GPU for Training
**Risk:** LOW (known constraint). Training cannot happen on the local AMD GPU.
**Mitigation:** Use cloud GPU (Colab free tier is sufficient). Total cost: $0-10.

---

## 8. Alternatives to LoRA

If LoRA proves impractical, consider these alternatives in order of preference:

### A1: System Prompt Engineering (Zero Cost)
Before investing in fine-tuning, try aggressive system prompting in the Modelfile:
```
SYSTEM "Output ONLY raw C# code. No markdown. No explanations. No thinking.
Start your response with 'using' or 'public'. Never use ``` fences."
```
This costs nothing and may solve the format problem entirely.

### A2: Improved Code Extractor
Modify `code_extractor.py` to handle IQuest's specific output format (e.g., strip
thinking tokens, handle different fence styles). This is the cheapest fix if the
underlying code is correct.

### A3: Ollama Modelfile with Stop Tokens and Template
Configure the Modelfile with appropriate chat template and stop tokens to prevent
the model from emitting thinking/reasoning tokens in its output.

### A4: Use a Different 7B Model
If IQuest-Coder proves too difficult to adapt, consider models that already score
well at the 7B scale (e.g., Qwen3.5 4B scores, granite4 7B-a1b). The LoRA approach
could be applied to any of these as well.

---

## 9. Recommended Execution Order

1. **Try A1 + A2 first** (system prompt + extractor fix) — 1-2 hours, no cost
2. **If still failing,** prepare training dataset from existing benchmark data — 2-3 hours
3. **Run LoRA training** on Colab free tier — 1-2 hours including setup
4. **Convert to GGUF**, deploy to Ollama, run benchmark — 1 hour
5. **Iterate** if needed — 1-2 more training runs

**Total estimated effort:** 1-2 days elapsed, $0-10 in cloud compute.
