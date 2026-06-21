#!/usr/bin/env python3
"""Verify the LoRA fine-tuning environment is ready.

Checks: torch + CUDA, datasets, peft, trl, transformers, accelerate, and the
HF_TOKEN environment variable (needed to stream bigcode/the-stack-dedup).

Exit 0 if all green, 1 if any check fails.
"""
import importlib
import os
import sys

REQUIRED = ["torch", "datasets", "peft", "trl", "transformers", "accelerate"]


def main() -> int:
    ok = True
    for mod in REQUIRED:
        try:
            m = importlib.import_module(mod)
            print(f"  OK   {mod} {getattr(m, '__version__', '?')}")
        except Exception as e:  # noqa: BLE001 - report any import failure
            print(f"  FAIL {mod}: {e}")
            ok = False

    try:
        import torch
        if torch.cuda.is_available():
            print(f"  OK   CUDA available: {torch.cuda.get_device_name(0)}")
        else:
            print("  FAIL CUDA not available")
            ok = False
    except Exception as e:  # noqa: BLE001
        print(f"  FAIL CUDA check: {e}")
        ok = False

    if os.environ.get("HF_TOKEN") or os.environ.get("HUGGING_FACE_HUB_TOKEN"):
        print("  OK   HF token present")
    else:
        print("  WARN HF_TOKEN not set — public Stack access is rate-limited")

    print("READY" if ok else "NOT READY")
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main())
