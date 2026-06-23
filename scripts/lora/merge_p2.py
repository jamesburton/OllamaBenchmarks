import os, torch
from transformers import AutoModelForCausalLM, AutoTokenizer
from peft import PeftModel
base = "WeiboAI/VibeThinker-3B"
adp = r"C:\Development\vibethinker-train\scripts\lora\output\vibethinker-csharp-p2-50k-lora"
out = r"C:\Development\vibethinker-train\merged-p2"
print("loading tokenizer from adapter dir...")
tok = AutoTokenizer.from_pretrained(adp, trust_remote_code=True)
print("loading base model (fp16, cpu)...")
m = AutoModelForCausalLM.from_pretrained(base, dtype=torch.float16, device_map="cpu", trust_remote_code=True)
print("applying + merging adapter...")
m = PeftModel.from_pretrained(m, adp).merge_and_unload()
os.makedirs(out, exist_ok=True)
m.save_pretrained(out, safe_serialization=True)
tok.save_pretrained(out)
print("MERGED_OK ->", out)
