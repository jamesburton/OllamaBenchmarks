#!/usr/bin/env python3
"""LoRA fine-tune WeiboAI/VibeThinker-3B for C#/.NET (phase 1, code-only).

Adapted from train_qwen35_lora.py. VibeThinker is Qwen2.5 arch — standard
target modules. At 3B, bf16 LoRA fits the RTX 3060 12GB on-GPU; we try that
first and fall back to an fp16 CPU/GPU split if it OOMs.
"""
import argparse
import sys
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
DEFAULT_TRAINING_DATA = SCRIPT_DIR / "data" / "stack_csharp_train.jsonl"
DEFAULT_OUTPUT_DIR = SCRIPT_DIR / "output" / "vibethinker-csharp-p1-lora"

BASE_MODEL = "WeiboAI/VibeThinker-3B"
LORA_DROPOUT = 0.05
TARGET_MODULES = [
    "q_proj", "k_proj", "v_proj", "o_proj",
    "gate_proj", "up_proj", "down_proj",
]
SYSTEM_PROMPT = (
    "You are an expert C#/.NET developer. When asked to write code, "
    "return ONLY valid C# code in a single file. Do not include markdown "
    "fences, explanations, or commentary — just the raw C# source code."
)


def parse_args():
    p = argparse.ArgumentParser(description="LoRA fine-tune VibeThinker-3B for C#")
    p.add_argument("--training-data", type=Path, default=DEFAULT_TRAINING_DATA)
    p.add_argument("--output-dir", type=Path, default=DEFAULT_OUTPUT_DIR)
    p.add_argument("--base-model", default=BASE_MODEL)
    p.add_argument("--epochs", type=int, default=3)
    p.add_argument("--batch-size", type=int, default=1)
    p.add_argument("--gradient-accumulation", type=int, default=8)
    p.add_argument("--lr", type=float, default=2e-4)
    p.add_argument("--max-seq-length", type=int, default=4096)
    p.add_argument("--lora-r", type=int, default=32)
    p.add_argument("--lora-alpha", type=int, default=64)
    p.add_argument("--max-steps", type=int, default=-1,
                   help="cap training steps (smoke runs); -1 = full")
    p.add_argument("--phase", type=int, choices=[1, 2], default=1)
    return p.parse_args()


def load_examples(path: Path) -> list[dict]:
    import json
    if not path.exists():
        print(f"ERROR: training data not found at {path}")
        print("Run build_stack_csharp_dataset.py first.")
        sys.exit(1)
    rows = []
    with open(path, "r", encoding="utf-8") as fh:
        for line in fh:
            if line.strip():
                rows.append(json.loads(line))
    print(f"Loaded {len(rows)} training examples")
    return rows


def train(args, examples, bf16_gpu_only: bool):
    import torch
    from transformers import AutoModelForCausalLM, AutoTokenizer
    from peft import LoraConfig, get_peft_model
    from trl import SFTTrainer, SFTConfig
    from datasets import Dataset

    label = "bf16 GPU-only" if bf16_gpu_only else "fp16 + CPU offload (device_map=auto)"
    print(f"\n=== Loading {args.base_model} ({label}) ===")

    tokenizer = AutoTokenizer.from_pretrained(
        args.base_model, trust_remote_code=True, padding_side="right"
    )
    if tokenizer.pad_token is None:
        tokenizer.pad_token = tokenizer.eos_token

    load_kwargs = {"trust_remote_code": True, "attn_implementation": "eager"}
    if bf16_gpu_only:
        load_kwargs["torch_dtype"] = torch.bfloat16
        load_kwargs["device_map"] = {"": "cuda:0"}
    else:
        load_kwargs["torch_dtype"] = torch.float16
        load_kwargs["device_map"] = "auto"

    model = AutoModelForCausalLM.from_pretrained(args.base_model, **load_kwargs)
    model.gradient_checkpointing_enable()

    lora_config = LoraConfig(
        r=args.lora_r, lora_alpha=args.lora_alpha, lora_dropout=LORA_DROPOUT,
        bias="none", task_type="CAUSAL_LM", target_modules=TARGET_MODULES,
    )
    model = get_peft_model(model, lora_config)
    model.print_trainable_parameters()

    dataset = Dataset.from_list(examples)

    def formatting_func(example):
        messages = example["messages"]
        if not messages or messages[0].get("role") != "system":
            messages = [{"role": "system", "content": SYSTEM_PROMPT}] + messages
        return tokenizer.apply_chat_template(
            messages, tokenize=False, add_generation_prompt=False
        )

    training_args = SFTConfig(
        output_dir=str(args.output_dir),
        num_train_epochs=args.epochs,
        max_steps=args.max_steps,
        per_device_train_batch_size=args.batch_size,
        gradient_accumulation_steps=args.gradient_accumulation,
        learning_rate=args.lr,
        weight_decay=0.01,
        warmup_ratio=0.1,
        lr_scheduler_type="cosine",
        logging_steps=5,
        save_strategy="epoch",
        save_total_limit=2,
        bf16=bf16_gpu_only,
        fp16=not bf16_gpu_only,
        max_length=args.max_seq_length,
        gradient_checkpointing=True,
        gradient_checkpointing_kwargs={"use_reentrant": False},
        optim="adamw_8bit",
        max_grad_norm=0.3,
        seed=42,
        report_to="none",
    )

    trainer = SFTTrainer(
        model=model, processing_class=tokenizer, train_dataset=dataset,
        args=training_args, formatting_func=formatting_func,
    )
    print(f"\n=== Training (phase {args.phase}, {label}) ===")
    trainer.train()
    print(f"\nSaving adapter to {args.output_dir}")
    model.save_pretrained(str(args.output_dir))
    tokenizer.save_pretrained(str(args.output_dir))


def main():
    args = parse_args()
    examples = load_examples(args.training_data)
    if not examples:
        sys.exit(1)
    try:
        train(args, examples, bf16_gpu_only=True)
    except RuntimeError as e:
        if "out of memory" in str(e).lower():
            print(f"\nOOM in bf16 GPU-only: {e}\nRetrying fp16 split...")
            import torch
            torch.cuda.empty_cache()
            train(args, examples, bf16_gpu_only=False)
        else:
            raise
    print("\n=== Training complete ===")
    print(f"Adapter: {args.output_dir}")


if __name__ == "__main__":
    main()
