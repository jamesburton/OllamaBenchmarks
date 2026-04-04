#!/usr/bin/env python3
"""LoRA fine-tuning script for Qwen3.5-9B.

Strategy based on research:
- Unsloth warns against QLoRA (4-bit) for Qwen3.5 due to hybrid architecture
- bf16 LoRA on 9B needs ~22GB VRAM (won't fit RTX 3060 12GB without offloading)
- Approach: try bf16 LoRA with gradient checkpointing + 8-bit optimizer to
  offload to 96GB system RAM. Fall back to QLoRA if OOM.
- Strix (128GB unified) noted as alternative but ROCm immature for training.

Usage:
    python scripts/lora/train_qwen35_lora.py
    python scripts/lora/train_qwen35_lora.py --quantize 4bit  # force QLoRA
    python scripts/lora/train_qwen35_lora.py --quantize none  # force bf16
    python scripts/lora/train_qwen35_lora.py --epochs 5 --lr 1e-4
"""

import argparse
import json
import os
import subprocess
import sys
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
DEFAULT_TRAINING_DATA = SCRIPT_DIR / "training_data_1k.jsonl"
DEFAULT_OUTPUT_DIR = SCRIPT_DIR / "output" / "cogito-8b-csharp-lora"
DEFAULT_MERGED_DIR = SCRIPT_DIR / "output" / "cogito-8b-csharp-merged"
DEFAULT_GGUF_DIR = SCRIPT_DIR / "output" / "cogito-8b-csharp-gguf"

# Qwen3.5-9B segfaults on Windows (GatedDeltaNet needs triton which is broken).
# Using cogito-8B instead: Llama 3.1 arch, 5/5 quality, 61.6 tok/s on Framework.
BASE_MODEL = "deepcogito/cogito-v1-preview-llama-8B"

# LoRA config — conservative rank for 1K dataset to avoid overfitting
LORA_R = 32
LORA_ALPHA = 64
LORA_DROPOUT = 0.05

DEFAULT_TARGET_MODULES = [
    "q_proj", "k_proj", "v_proj", "o_proj",
    "gate_proj", "up_proj", "down_proj",
]

SYSTEM_PROMPT = (
    "You are an expert C#/.NET developer. When asked to write code, "
    "return ONLY valid C# code in a single file. Do not include markdown "
    "fences, explanations, or commentary — just the raw C# source code."
)


def parse_args():
    parser = argparse.ArgumentParser(
        description="LoRA fine-tune Qwen3.5-9B for C#/.NET code generation"
    )
    parser.add_argument("--training-data", type=Path, default=DEFAULT_TRAINING_DATA)
    parser.add_argument("--output-dir", type=Path, default=DEFAULT_OUTPUT_DIR)
    parser.add_argument("--epochs", type=int, default=3)
    parser.add_argument("--batch-size", type=int, default=1)
    parser.add_argument("--gradient-accumulation", type=int, default=8)
    parser.add_argument("--lr", type=float, default=2e-4)
    parser.add_argument("--max-seq-length", type=int, default=4096)
    parser.add_argument("--lora-r", type=int, default=LORA_R)
    parser.add_argument("--lora-alpha", type=int, default=LORA_ALPHA)
    parser.add_argument(
        "--quantize", choices=["auto", "4bit", "none"], default="auto",
        help="Quantization: auto (try bf16 then 4bit), 4bit (QLoRA), none (bf16)"
    )
    parser.add_argument("--skip-gguf", action="store_true")
    parser.add_argument("--skip-merge", action="store_true")
    parser.add_argument(
        "--base-model", default=BASE_MODEL,
        help="HuggingFace model ID (default: unsloth/Qwen3.5-9B)"
    )
    return parser.parse_args()


def load_training_data(path: Path) -> list[dict]:
    if not path.exists():
        print(f"ERROR: Training data not found at {path}")
        print("Run generate_training_data_scaled.py first.")
        sys.exit(1)
    examples = []
    with open(path, "r", encoding="utf-8") as fh:
        for line in fh:
            if line.strip():
                examples.append(json.loads(line))
    print(f"Loaded {len(examples)} training examples")
    return examples


def detect_target_modules(model) -> list[str]:
    import torch
    linear_names = set()
    for name, module in model.named_modules():
        if isinstance(module, torch.nn.Linear):
            linear_names.add(name.split(".")[-1])
    linear_names.discard("lm_head")
    known = set(DEFAULT_TARGET_MODULES)
    found = linear_names & known
    if len(found) >= 4:
        result = sorted(found)
        print(f"Using target modules: {result}")
        return result
    result = sorted(linear_names)
    print(f"Auto-detected target modules: {result}")
    return result


def try_unsloth(args, examples, load_in_4bit: bool):
    """Attempt training with Unsloth."""
    try:
        from unsloth import FastLanguageModel
    except ImportError:
        print("Unsloth not installed. Install with: pip install unsloth")
        return False

    quant_label = "4-bit QLoRA" if load_in_4bit else "bf16 LoRA"
    print(f"\n=== Unsloth {quant_label} path ===")

    try:
        model, tokenizer = FastLanguageModel.from_pretrained(
            model_name=args.base_model,
            max_seq_length=args.max_seq_length,
            dtype=None,
            load_in_4bit=load_in_4bit,
            trust_remote_code=True,
        )
    except Exception as e:
        print(f"Unsloth load failed: {e}")
        return False

    print("Model loaded successfully")

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

    # RTX 3060 has limited bf16 — use fp16 for 4bit, bf16 for full precision
    use_bf16 = not load_in_4bit
    use_fp16 = load_in_4bit

    training_args = SFTConfig(
        output_dir=str(args.output_dir),
        num_train_epochs=args.epochs,
        per_device_train_batch_size=args.batch_size,
        gradient_accumulation_steps=args.gradient_accumulation,
        learning_rate=args.lr,
        weight_decay=0.01,
        warmup_ratio=0.1,
        lr_scheduler_type="cosine",
        logging_steps=5,
        save_strategy="epoch",
        save_total_limit=2,
        fp16=use_fp16,
        bf16=use_bf16,
        max_length=args.max_seq_length,
        optim="adamw_8bit",
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

    print(f"\n=== Starting training ===")
    print(f"  Examples: {len(examples)}")
    print(f"  Epochs: {args.epochs}")
    print(f"  Effective batch: {args.batch_size * args.gradient_accumulation}")
    print(f"  LoRA rank: {args.lora_r}, alpha: {args.lora_alpha}")
    print(f"  Quantization: {quant_label}")

    trainer.train()

    print(f"\nSaving adapter to {args.output_dir}")
    model.save_pretrained(str(args.output_dir))
    tokenizer.save_pretrained(str(args.output_dir))
    return True


def train_peft_trl(args, examples, load_in_4bit: bool):
    """Fallback: PEFT + TRL training."""
    import torch
    from transformers import AutoModelForCausalLM, AutoTokenizer, BitsAndBytesConfig
    from peft import LoraConfig, get_peft_model, prepare_model_for_kbit_training
    from trl import SFTTrainer, SFTConfig
    from datasets import Dataset

    quant_label = "4-bit QLoRA" if load_in_4bit else "bf16"
    print(f"\n=== PEFT + TRL {quant_label} path ===")

    load_kwargs = {"trust_remote_code": True}
    if load_in_4bit:
        load_kwargs["quantization_config"] = BitsAndBytesConfig(
            load_in_4bit=True,
            bnb_4bit_quant_type="nf4",
            bnb_4bit_compute_dtype=torch.float16,
            bnb_4bit_use_double_quant=True,
        )
        load_kwargs["torch_dtype"] = torch.float16
    else:
        load_kwargs["torch_dtype"] = torch.bfloat16

    # Use sequential device map to keep all layers on GPU if they fit,
    # spilling to CPU only for embeddings/lm_head. This avoids gradient
    # device mismatches during backprop.
    load_kwargs["device_map"] = {"": "cuda:0"}
    load_kwargs["attn_implementation"] = "eager"

    tokenizer = AutoTokenizer.from_pretrained(
        args.base_model, trust_remote_code=True, padding_side="right"
    )
    if tokenizer.pad_token is None:
        tokenizer.pad_token = tokenizer.eos_token

    model = AutoModelForCausalLM.from_pretrained(args.base_model, **load_kwargs)

    if load_in_4bit:
        model = prepare_model_for_kbit_training(
            model, use_gradient_checkpointing=True
        )
    else:
        model.gradient_checkpointing_enable()

    lora_config = LoraConfig(
        r=args.lora_r,
        lora_alpha=args.lora_alpha,
        lora_dropout=LORA_DROPOUT,
        bias="none",
        task_type="CAUSAL_LM",
        target_modules=detect_target_modules(model),
    )

    model = get_peft_model(model, lora_config)
    model.print_trainable_parameters()

    dataset = Dataset.from_list(examples)

    def formatting_func(example):
        return tokenizer.apply_chat_template(
            example["messages"], tokenize=False, add_generation_prompt=False
        )

    use_bf16 = not load_in_4bit
    use_fp16 = load_in_4bit

    training_args = SFTConfig(
        output_dir=str(args.output_dir),
        num_train_epochs=args.epochs,
        per_device_train_batch_size=args.batch_size,
        gradient_accumulation_steps=args.gradient_accumulation,
        learning_rate=args.lr,
        weight_decay=0.01,
        warmup_ratio=0.1,
        lr_scheduler_type="cosine",
        logging_steps=5,
        save_strategy="epoch",
        save_total_limit=2,
        fp16=use_fp16,
        bf16=use_bf16,
        max_length=args.max_seq_length,
        gradient_checkpointing=True,
        gradient_checkpointing_kwargs={"use_reentrant": False},
        optim="paged_adamw_8bit",
        max_grad_norm=0.3,
        seed=42,
        report_to="none",
    )

    trainer = SFTTrainer(
        model=model, processing_class=tokenizer, train_dataset=dataset,
        args=training_args, formatting_func=formatting_func,
    )

    print(f"\n=== Starting training (PEFT) ===")
    trainer.train()

    print(f"\nSaving adapter to {args.output_dir}")
    model.save_pretrained(str(args.output_dir))
    tokenizer.save_pretrained(str(args.output_dir))


def merge_and_export(args):
    """Merge LoRA adapter into base model."""
    import torch
    from transformers import AutoModelForCausalLM, AutoTokenizer
    from peft import PeftModel

    merged_dir = DEFAULT_MERGED_DIR
    print(f"\n=== Merging adapter ===")

    tokenizer = AutoTokenizer.from_pretrained(
        str(args.output_dir), trust_remote_code=True
    )
    base_model = AutoModelForCausalLM.from_pretrained(
        args.base_model,
        torch_dtype=torch.float16,
        device_map="cpu",
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
    """Convert merged model to GGUF Q4_K_M for Ollama."""
    gguf_dir = DEFAULT_GGUF_DIR
    gguf_dir.mkdir(parents=True, exist_ok=True)
    gguf_path = gguf_dir / "cogito-8b-csharp-Q4_K_M.gguf"
    f16_path = gguf_dir / "cogito-8b-csharp-f16.gguf"

    print(f"\n=== Exporting to GGUF Q4_K_M ===")

    # Try llama.cpp convert + quantize (we have llama.cpp at C:\adl\Programs\llama-cpp)
    llama_cpp_dir = Path(r"C:\adl\Programs\llama-cpp")
    quantize_bin = llama_cpp_dir / "llama-quantize.exe"

    # Method 1: Unsloth's built-in GGUF export
    try:
        from unsloth import FastLanguageModel
        print("Trying Unsloth GGUF export...")
        model, tokenizer = FastLanguageModel.from_pretrained(
            str(merged_dir), dtype=None, load_in_4bit=False,
        )
        model.save_pretrained_gguf(
            str(gguf_dir), tokenizer, quantization_method="q4_k_m",
        )
        # Unsloth names the file differently — find it
        for f in gguf_dir.glob("*.gguf"):
            if "q4_k_m" in f.name.lower() or "Q4_K_M" in f.name:
                gguf_path = f
                break
        print(f"GGUF export successful: {gguf_path}")
        create_ollama_modelfile(gguf_path)
        return gguf_path
    except Exception as e:
        print(f"Unsloth GGUF export failed: {e}")

    # Method 2: convert_hf_to_gguf.py + llama-quantize
    convert_script = llama_cpp_dir / "convert_hf_to_gguf.py"
    if not convert_script.exists():
        # Try finding it in llama.cpp clone
        for candidate in [
            Path("llama.cpp") / "convert_hf_to_gguf.py",
            Path.home() / "llama.cpp" / "convert_hf_to_gguf.py",
        ]:
            if candidate.exists():
                convert_script = candidate
                break

    if convert_script.exists() and quantize_bin.exists():
        try:
            print(f"Converting via {convert_script}...")
            subprocess.run(
                [sys.executable, str(convert_script), str(merged_dir),
                 "--outfile", str(f16_path), "--outtype", "f16"],
                check=True,
            )
            subprocess.run(
                [str(quantize_bin), str(f16_path), str(gguf_path), "Q4_K_M"],
                check=True,
            )
            f16_path.unlink(missing_ok=True)
            print(f"GGUF export successful: {gguf_path}")
            create_ollama_modelfile(gguf_path)
            return gguf_path
        except subprocess.CalledProcessError as e:
            print(f"Conversion failed: {e}")

    print("\nManual GGUF conversion needed:")
    print(f"  python convert_hf_to_gguf.py {merged_dir} --outfile {f16_path} --outtype f16")
    print(f"  llama-quantize {f16_path} {gguf_path} Q4_K_M")
    return None


def create_ollama_modelfile(gguf_path: Path):
    """Generate Ollama Modelfile for the fine-tuned model."""
    modelfile_path = gguf_path.parent / "Modelfile"
    content = f"""FROM {gguf_path.name}

PARAMETER temperature 0.2
PARAMETER top_p 0.9
PARAMETER stop "<|endoftext|>"
PARAMETER stop "<|im_end|>"
PARAMETER num_ctx 8192

SYSTEM "{SYSTEM_PROMPT}"
"""
    modelfile_path.write_text(content, encoding="utf-8")
    print(f"\nOllama Modelfile: {modelfile_path}")
    print(f"Import: cd {gguf_path.parent} && ollama create cogito-8b-csharp -f Modelfile")


def main():
    args = parse_args()
    examples = load_training_data(args.training_data)
    if not examples:
        sys.exit(1)

    # Determine quantization strategy
    if args.quantize == "4bit":
        strategies = [(True, "QLoRA 4-bit")]
    elif args.quantize == "none":
        strategies = [(False, "bf16 LoRA")]
    else:
        # Auto: try bf16 first (better quality), fall back to 4bit
        strategies = [
            (False, "bf16 LoRA"),
            (True, "QLoRA 4-bit (fallback)"),
        ]

    trained = False
    # bitsandbytes 4-bit segfaults on Windows with this model.
    # Force fp16 LoRA with CPU/GPU split — proven to work.
    strategies = [(False, "fp16 LoRA (CPU/GPU split)")]

    for load_in_4bit, label in strategies:
        print(f"\n{'='*60}")
        print(f"Attempting: {label}")
        print(f"{'='*60}")
        try:
            train_peft_trl(args, examples, load_in_4bit)
            trained = True
            break
        except RuntimeError as e:
            if "out of memory" in str(e).lower() or "CUDA" in str(e):
                print(f"\nOOM with {label}: {e}")
                if load_in_4bit:
                    print("Both strategies failed. Consider:")
                    print("  - Reduce --max-seq-length to 2048")
                    print("  - Reduce --lora-r to 16")
                    print("  - Train on Strix (128GB unified memory, CPU mode)")
                    sys.exit(1)
                print("Falling back to 4-bit QLoRA...")
                import torch
                torch.cuda.empty_cache()
                continue
            raise

    if not trained:
        print("Training failed.")
        sys.exit(1)

    print("\n=== Training complete ===")

    if not args.skip_merge:
        merged_dir = merge_and_export(args)
        if not args.skip_gguf:
            export_gguf(merged_dir)

    print("\n=== All done ===")
    print("\nNext steps:")
    print("  1. Import to Ollama: ollama create cogito-8b-csharp -f Modelfile")
    print("  2. Benchmark: python scripts/benchmark_coding.py --models cogito-8b-csharp --layers 3")
    print("  3. Quality: python scripts/benchmark_quality.py --models cogito-8b-csharp")


if __name__ == "__main__":
    main()
