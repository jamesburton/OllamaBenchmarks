#!/usr/bin/env python3
"""QLoRA fine-tuning script for IQuest-Coder-V1-7B-Instruct.

Targets RTX 3060 12GB with 4-bit quantization. Tries unsloth first for speed,
falls back to PEFT + TRL if the custom 'iquestcoder' architecture isn't supported.

Usage:
    python scripts/lora/train_iquest_lora.py
    python scripts/lora/train_iquest_lora.py --epochs 5 --lr 1e-4
    python scripts/lora/train_iquest_lora.py --skip-gguf  # skip GGUF export
"""

import argparse
import json
import os
import sys
from pathlib import Path

# ── Configuration ──────────────────────────────────────────────────────
SCRIPT_DIR = Path(__file__).resolve().parent
DEFAULT_TRAINING_DATA = SCRIPT_DIR / "training_data.jsonl"
DEFAULT_OUTPUT_DIR = SCRIPT_DIR / "output" / "iquest-7b-csharp-lora"
DEFAULT_MERGED_DIR = SCRIPT_DIR / "output" / "iquest-7b-csharp-merged"
DEFAULT_GGUF_DIR = SCRIPT_DIR / "output" / "iquest-7b-csharp-gguf"

BASE_MODEL = "IQuestLab/IQuest-Coder-V1-7B-Instruct"

# LoRA hyperparameters
LORA_R = 16
LORA_ALPHA = 32
LORA_DROPOUT = 0.05

# QLoRA target modules — covers all attention + MLP projections.
# These are the standard names for Qwen2-family / transformer architectures.
# If the model uses different names, we auto-detect from the model's named modules.
DEFAULT_TARGET_MODULES = [
    "q_proj", "k_proj", "v_proj", "o_proj",
    "gate_proj", "up_proj", "down_proj",
]


def parse_args():
    parser = argparse.ArgumentParser(
        description="QLoRA fine-tune IQuest-Coder-V1-7B for C#/.NET formatting"
    )
    parser.add_argument(
        "--training-data", type=Path, default=DEFAULT_TRAINING_DATA,
        help="Path to training_data.jsonl"
    )
    parser.add_argument(
        "--output-dir", type=Path, default=DEFAULT_OUTPUT_DIR,
        help="Directory to save LoRA adapter"
    )
    parser.add_argument("--epochs", type=int, default=3)
    parser.add_argument("--batch-size", type=int, default=1)
    parser.add_argument("--gradient-accumulation", type=int, default=4)
    parser.add_argument("--lr", type=float, default=2e-4)
    parser.add_argument("--max-seq-length", type=int, default=4096)
    parser.add_argument("--lora-r", type=int, default=LORA_R)
    parser.add_argument("--lora-alpha", type=int, default=LORA_ALPHA)
    parser.add_argument(
        "--skip-gguf", action="store_true",
        help="Skip GGUF export after training"
    )
    parser.add_argument(
        "--skip-merge", action="store_true",
        help="Skip merging adapter into base model"
    )
    return parser.parse_args()


def load_training_data(path: Path) -> list[dict]:
    """Load JSONL training data."""
    if not path.exists():
        print(f"ERROR: Training data not found at {path}")
        print("Run generate_training_data.py first.")
        sys.exit(1)

    examples = []
    with open(path, "r", encoding="utf-8") as fh:
        for line in fh:
            line = line.strip()
            if line:
                examples.append(json.loads(line))
    print(f"Loaded {len(examples)} training examples")
    return examples


def detect_target_modules(model) -> list[str]:
    """Auto-detect linear layer names suitable for LoRA targeting."""
    import torch

    linear_names = set()
    for name, module in model.named_modules():
        if isinstance(module, (torch.nn.Linear,)):
            # Get the last component of the name (e.g., "q_proj" from "model.layers.0.self_attn.q_proj")
            short_name = name.split(".")[-1]
            linear_names.add(short_name)

    # Remove lm_head — we don't want to LoRA the output projection
    linear_names.discard("lm_head")

    # Prefer known attention + MLP modules if they exist
    known = set(DEFAULT_TARGET_MODULES)
    found_known = linear_names & known
    if len(found_known) >= 4:
        result = sorted(found_known)
        print(f"Using known target modules: {result}")
        return result

    # Fallback: use all detected linear layers
    result = sorted(linear_names)
    print(f"Auto-detected target modules: {result}")
    return result


def try_unsloth(args, examples):
    """Attempt training with unsloth (2-3x faster, lower VRAM)."""
    try:
        from unsloth import FastLanguageModel
    except ImportError:
        print("unsloth not installed, skipping unsloth path")
        return False

    print("\n=== Attempting unsloth fast path ===")
    try:
        model, tokenizer = FastLanguageModel.from_pretrained(
            model_name=BASE_MODEL,
            max_seq_length=args.max_seq_length,
            dtype=None,  # auto-detect
            load_in_4bit=True,
            trust_remote_code=True,
        )
    except Exception as e:
        print(f"unsloth cannot load this architecture: {e}")
        print("Falling back to PEFT + TRL")
        return False

    print("unsloth loaded model successfully")

    model = FastLanguageModel.get_peft_model(
        model,
        r=args.lora_r,
        target_modules=detect_target_modules(model),
        lora_alpha=args.lora_alpha,
        lora_dropout=LORA_DROPOUT,
        bias="none",
        use_gradient_checkpointing="unsloth",
        random_state=42,
    )

    from trl import SFTTrainer, SFTConfig
    from datasets import Dataset

    dataset = Dataset.from_list(examples)

    training_args = SFTConfig(
        output_dir=str(args.output_dir),
        num_train_epochs=args.epochs,
        per_device_train_batch_size=args.batch_size,
        gradient_accumulation_steps=args.gradient_accumulation,
        learning_rate=args.lr,
        weight_decay=0.01,
        warmup_ratio=0.1,
        lr_scheduler_type="cosine",
        logging_steps=1,
        save_strategy="epoch",
        save_total_limit=2,
        fp16=True,
        bf16=False,  # RTX 3060 has limited bf16 support
        max_seq_length=args.max_seq_length,
        dataset_text_field=None,
        seed=42,
        report_to="none",
    )

    def formatting_func(example):
        return tokenizer.apply_chat_template(
            example["messages"], tokenize=False, add_generation_prompt=False
        )

    trainer = SFTTrainer(
        model=model,
        tokenizer=tokenizer,
        train_dataset=dataset,
        args=training_args,
        formatting_func=formatting_func,
    )

    print("\n=== Starting training (unsloth) ===")
    trainer.train()

    print(f"\nSaving adapter to {args.output_dir}")
    model.save_pretrained(str(args.output_dir))
    tokenizer.save_pretrained(str(args.output_dir))
    return True


def train_peft_trl(args, examples):
    """Standard PEFT + TRL training path (works with any architecture)."""
    import torch
    from transformers import (
        AutoModelForCausalLM,
        AutoTokenizer,
        BitsAndBytesConfig,
    )
    from peft import LoraConfig, get_peft_model, prepare_model_for_kbit_training
    from trl import SFTTrainer, SFTConfig
    from datasets import Dataset

    print("\n=== Loading model with 4-bit quantization (PEFT path) ===")

    bnb_config = BitsAndBytesConfig(
        load_in_4bit=True,
        bnb_4bit_quant_type="nf4",
        bnb_4bit_compute_dtype=torch.float16,
        bnb_4bit_use_double_quant=True,
    )

    tokenizer = AutoTokenizer.from_pretrained(
        BASE_MODEL,
        trust_remote_code=True,
        padding_side="right",
    )
    if tokenizer.pad_token is None:
        tokenizer.pad_token = tokenizer.eos_token

    model = AutoModelForCausalLM.from_pretrained(
        BASE_MODEL,
        quantization_config=bnb_config,
        device_map="auto",
        trust_remote_code=True,
        torch_dtype=torch.float16,
        attn_implementation="eager",  # safe default; flash_attention_2 if supported
    )

    model = prepare_model_for_kbit_training(model, use_gradient_checkpointing=True)

    # Detect which modules to target
    target_modules = detect_target_modules(model)

    lora_config = LoraConfig(
        r=args.lora_r,
        lora_alpha=args.lora_alpha,
        lora_dropout=LORA_DROPOUT,
        bias="none",
        task_type="CAUSAL_LM",
        target_modules=target_modules,
    )

    model = get_peft_model(model, lora_config)
    model.print_trainable_parameters()

    # Prepare dataset
    dataset = Dataset.from_list(examples)

    def formatting_func(example):
        return tokenizer.apply_chat_template(
            example["messages"], tokenize=False, add_generation_prompt=False
        )

    training_args = SFTConfig(
        output_dir=str(args.output_dir),
        num_train_epochs=args.epochs,
        per_device_train_batch_size=args.batch_size,
        gradient_accumulation_steps=args.gradient_accumulation,
        learning_rate=args.lr,
        weight_decay=0.01,
        warmup_ratio=0.1,
        lr_scheduler_type="cosine",
        logging_steps=1,
        save_strategy="epoch",
        save_total_limit=2,
        fp16=True,
        bf16=False,
        max_seq_length=args.max_seq_length,
        dataset_text_field=None,
        gradient_checkpointing=True,
        gradient_checkpointing_kwargs={"use_reentrant": False},
        optim="paged_adamw_8bit",
        max_grad_norm=0.3,
        seed=42,
        report_to="none",
    )

    trainer = SFTTrainer(
        model=model,
        tokenizer=tokenizer,
        train_dataset=dataset,
        args=training_args,
        formatting_func=formatting_func,
    )

    print("\n=== Starting training (PEFT + TRL) ===")
    print(f"  Epochs: {args.epochs}")
    print(f"  Batch size: {args.batch_size} x {args.gradient_accumulation} gradient accumulation")
    print(f"  Effective batch size: {args.batch_size * args.gradient_accumulation}")
    print(f"  Learning rate: {args.lr}")
    print(f"  Max sequence length: {args.max_seq_length}")
    print(f"  LoRA rank: {args.lora_r}, alpha: {args.lora_alpha}")

    trainer.train()

    print(f"\nSaving adapter to {args.output_dir}")
    model.save_pretrained(str(args.output_dir))
    tokenizer.save_pretrained(str(args.output_dir))


def merge_and_export(args):
    """Merge LoRA adapter into base model and save full-precision copy."""
    import torch
    from transformers import AutoModelForCausalLM, AutoTokenizer
    from peft import PeftModel

    merged_dir = DEFAULT_MERGED_DIR
    print(f"\n=== Merging adapter into base model ===")
    print(f"  Adapter: {args.output_dir}")
    print(f"  Base: {BASE_MODEL}")
    print(f"  Output: {merged_dir}")

    tokenizer = AutoTokenizer.from_pretrained(
        str(args.output_dir), trust_remote_code=True
    )

    # Load base model in float16 for merging (not quantized)
    base_model = AutoModelForCausalLM.from_pretrained(
        BASE_MODEL,
        torch_dtype=torch.float16,
        device_map="cpu",  # merge on CPU to avoid VRAM limits
        trust_remote_code=True,
    )

    model = PeftModel.from_pretrained(base_model, str(args.output_dir))
    model = model.merge_and_unload()

    merged_dir.mkdir(parents=True, exist_ok=True)
    model.save_pretrained(str(merged_dir), safe_serialization=True)
    tokenizer.save_pretrained(str(merged_dir))
    print(f"Merged model saved to {merged_dir}")
    return merged_dir


def export_gguf(merged_dir: Path):
    """Convert merged model to GGUF Q4_K_M format for Ollama."""
    import subprocess

    gguf_dir = DEFAULT_GGUF_DIR
    gguf_dir.mkdir(parents=True, exist_ok=True)
    gguf_path = gguf_dir / "iquest-7b-csharp-Q4_K_M.gguf"

    print(f"\n=== Exporting to GGUF Q4_K_M ===")
    print(f"  Input: {merged_dir}")
    print(f"  Output: {gguf_path}")

    # Try llama.cpp's convert script first (most reliable for custom architectures)
    convert_script = None
    for candidate in [
        "convert_hf_to_gguf.py",
        "convert-hf-to-gguf.py",
    ]:
        # Check if llama.cpp is installed or available
        result = subprocess.run(
            ["python", "-c", f"import llama_cpp; print(llama_cpp.__file__)"],
            capture_output=True, text=True
        )
        if result.returncode == 0:
            llama_cpp_dir = Path(result.stdout.strip()).parent.parent
            script = llama_cpp_dir / candidate
            if script.exists():
                convert_script = script
                break

    # Method 1: Use llama-cpp-python's built-in converter if available
    try:
        print("Attempting GGUF conversion via llama-cpp-python...")
        subprocess.run(
            [
                sys.executable, "-m", "llama_cpp.convert",
                str(merged_dir),
                "--outfile", str(gguf_path),
                "--outtype", "q4_k_m",
            ],
            check=True,
        )
        print(f"GGUF export successful: {gguf_path}")
        return gguf_path
    except (subprocess.CalledProcessError, FileNotFoundError):
        pass

    # Method 2: Try convert_hf_to_gguf.py from llama.cpp repo
    if convert_script:
        try:
            print(f"Trying {convert_script}...")
            # First convert to f16 GGUF
            f16_path = gguf_dir / "iquest-7b-csharp-f16.gguf"
            subprocess.run(
                [sys.executable, str(convert_script), str(merged_dir),
                 "--outfile", str(f16_path), "--outtype", "f16"],
                check=True,
            )
            # Then quantize
            subprocess.run(
                ["llama-quantize", str(f16_path), str(gguf_path), "Q4_K_M"],
                check=True,
            )
            # Clean up f16
            f16_path.unlink(missing_ok=True)
            print(f"GGUF export successful: {gguf_path}")
            return gguf_path
        except (subprocess.CalledProcessError, FileNotFoundError):
            pass

    # Method 3: Suggest manual steps
    print("\nWARNING: Automatic GGUF conversion failed.")
    print("To convert manually:")
    print(f"  1. Clone llama.cpp: git clone https://github.com/ggerganov/llama.cpp")
    print(f"  2. python llama.cpp/convert_hf_to_gguf.py {merged_dir} --outfile {gguf_dir}/f16.gguf --outtype f16")
    print(f"  3. ./llama.cpp/build/bin/llama-quantize {gguf_dir}/f16.gguf {gguf_path} Q4_K_M")
    return None


def create_ollama_modelfile(gguf_path: Path):
    """Generate an Ollama Modelfile for importing the fine-tuned model."""
    modelfile_path = gguf_path.parent / "Modelfile"
    content = f"""FROM {gguf_path.name}

PARAMETER temperature 0.2
PARAMETER top_p 0.9
PARAMETER stop "<|endoftext|>"
PARAMETER stop "<|im_end|>"
PARAMETER num_ctx 8192

SYSTEM "You are an expert C#/.NET developer. When asked to write code, return ONLY valid C# code in a single file. Do not include markdown fences, explanations, or commentary — just the raw C# source code."
"""
    modelfile_path.write_text(content, encoding="utf-8")
    print(f"\nOllama Modelfile written to: {modelfile_path}")
    print(f"To import into Ollama:")
    print(f"  cd {gguf_path.parent}")
    print(f"  ollama create iquest-csharp -f Modelfile")


def main():
    args = parse_args()

    # Validate training data exists
    examples = load_training_data(args.training_data)
    if not examples:
        print("ERROR: No training examples found.", file=sys.stderr)
        sys.exit(1)

    # Try unsloth first, fall back to PEFT+TRL
    success = try_unsloth(args, examples)
    if not success:
        train_peft_trl(args, examples)

    print("\n=== Training complete ===")
    print(f"Adapter saved to: {args.output_dir}")

    # Merge adapter into base model
    if not args.skip_merge:
        merged_dir = merge_and_export(args)

        # Export to GGUF
        if not args.skip_gguf:
            gguf_path = export_gguf(merged_dir)
            if gguf_path:
                create_ollama_modelfile(gguf_path)
    elif not args.skip_gguf:
        print("\nSkipping GGUF export (requires merge step)")
        print("Run without --skip-merge to enable GGUF export")

    print("\n=== All done ===")


if __name__ == "__main__":
    main()
