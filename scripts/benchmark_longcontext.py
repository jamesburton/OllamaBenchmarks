"""Layer 4 — T5: long-context VRAM & throughput scaling.

Answers the headline question for the Qwen3.5/Qwythos model class: *does this
support long context without massive VRAM?* For a ladder of ``num_ctx`` values
it measures, per context length:

- **KV-cache VRAM** via the Ollama ``/api/ps`` ``size_vram`` field. A probe
  confirmed (see docs/agentic-tool-longcontext-benchmark-plan.md) that
  ``size_vram`` *includes* the KV cache and that KV is **pre-allocated by
  ``num_ctx``** (not grown by actual tokens). So VRAM is measured with a *tiny*
  prompt at each ``num_ctx`` — no large prefill, no risk of a catastrophic
  partial-offload prefill on a CPU-bound host.
- **Fit / offload** via ``size_vram < size`` (model's own bytes that spilled to
  CPU RAM). The largest fully-resident ``num_ctx`` is the practical ctx limit
  for that host's GPU.
- **Decode throughput** (``eval_count / eval_duration``) and **prefill
  throughput** — measured *only* where the context fits fully on the GPU, using
  a real ~L-token prompt, since a partially-offloaded prefill on an SSE4.2-only
  host can take hours.

Derived: GB-per-10k-tokens of KV (slope of size_vram vs num_ctx over the
fully-resident points) and the max fully-resident context.

Runs against ``OLLAMA_HOST`` (default the local server). ``size_vram`` is read
over HTTP, so the VRAM sweep works cross-machine; pass ``--nvidia-smi`` only
when running *on* the GPU host to also record total board VRAM for context.

NOTE: T4 (needle-in-haystack + long-C# comprehension) is planned for this file
too but is NOT implemented yet — this module is T5 only.
"""

import argparse
import datetime
import json
import os
import subprocess
import sys
import time
import urllib.error
import urllib.request

sys.path.insert(0, os.path.dirname(__file__))
from coding_tasks.task_runner import model_slug


# ---------------------------------------------------------------------------
# Ollama HTTP helpers
# ---------------------------------------------------------------------------

def _host() -> str:
    return os.environ.get("OLLAMA_HOST", "http://127.0.0.1:11434").rstrip("/")


def _post(path: str, payload: dict, timeout: int) -> dict:
    req = urllib.request.Request(
        f"{_host()}{path}",
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(req, timeout=timeout) as resp:
        return json.loads(resp.read().decode("utf-8"))


def _get(path: str, timeout: int = 30) -> dict:
    with urllib.request.urlopen(f"{_host()}{path}", timeout=timeout) as resp:
        return json.loads(resp.read().decode("utf-8"))


def ps_entry(model: str) -> dict:
    """Return the /api/ps record for ``model`` (or {} if not currently loaded)."""
    for m in _get("/api/ps").get("models", []):
        if m.get("name", "").startswith(model) or m.get("model", "").startswith(model):
            return m
    return {}


def nvidia_smi_used_mib(gpu_index: int) -> int | None:
    """Total used VRAM on the local board, or None if nvidia-smi is unavailable."""
    try:
        out = subprocess.run(
            ["nvidia-smi", f"--id={gpu_index}",
             "--query-gpu=memory.used", "--format=csv,noheader,nounits"],
            capture_output=True, text=True, timeout=15,
        )
        return int(out.stdout.strip().splitlines()[0])
    except (OSError, ValueError, subprocess.SubprocessError, IndexError):
        return None


# ---------------------------------------------------------------------------
# Prompt sizing
# ---------------------------------------------------------------------------

# Varied prose (not pure repetition, which some tokenizers collapse). Measured
# ~5.95 chars/token for qwen3.5; one correction pass below nails the target.
_FILLER_UNIT = (
    "the quick brown fox jumps over a lazy dog while parsing tokens and filling "
    "the cache with realistic prose for measurement purposes across many lines "
)
_CHARS_PER_TOKEN = 5.95


def _filler(num_chars: int) -> str:
    reps = num_chars // len(_FILLER_UNIT) + 1
    return (_FILLER_UNIT * reps)[:num_chars]


def build_prompt_for_tokens(target_tokens: int) -> str:
    """Build a filler prompt of roughly ``target_tokens`` tokens.

    Estimate-only (no calibration round-trip): for the T5 throughput metric the
    exact token count is not critical (decode tok/s is flat to within a few %),
    and a calibration pass would mean a second full prefill — costly at 128k.
    The real generate reports the actual ``prompt_eval_count``, which we record.
    """
    return _filler(int(target_tokens * _CHARS_PER_TOKEN))


# ---------------------------------------------------------------------------
# Per-context-length measurement
# ---------------------------------------------------------------------------

def measure_vram(model: str, num_ctx: int, gpu_index: int, gen_timeout: int) -> dict:
    """Load the model at ``num_ctx`` with a tiny prompt; read size/size_vram.

    Safe at any num_ctx: KV is pre-allocated by num_ctx but the prompt is tiny,
    so even a partially-offloaded load only does a trivial prefill.
    """
    smi_before = nvidia_smi_used_mib(gpu_index)
    _post("/api/generate", {
        "model": model, "prompt": "Hello.", "stream": False, "raw": True,
        "keep_alive": "10m",
        "options": {"num_ctx": num_ctx, "num_predict": 4, "temperature": 0, "seed": 42},
    }, timeout=gen_timeout)
    time.sleep(1.0)  # let /api/ps settle after (re)load
    ps = ps_entry(model)
    size = ps.get("size")
    size_vram = ps.get("size_vram")
    fits = bool(size and size_vram is not None and size_vram >= size)
    return {
        "num_ctx": num_ctx,
        "size_bytes": size,
        "size_vram_bytes": size_vram,
        "gpu_fraction": (size_vram / size) if (size and size_vram is not None) else None,
        "fits_fully": fits,
        "nvidia_smi_used_mib_before": smi_before,
        "nvidia_smi_used_mib_after": nvidia_smi_used_mib(gpu_index),
    }


def measure_throughput(model: str, num_ctx: int, num_predict: int, gen_timeout: int) -> dict:
    """Send a ~num_ctx-token prompt and time prefill + decode. Fully-resident only."""
    target = max(num_ctx - num_predict - 128, 64)
    prompt = build_prompt_for_tokens(target)
    body = _post("/api/generate", {
        "model": model, "prompt": prompt, "stream": False, "raw": True,
        "keep_alive": "10m",
        "options": {"num_ctx": num_ctx, "num_predict": num_predict,
                    "temperature": 0, "seed": 42},
    }, timeout=gen_timeout)
    pe_n = body.get("prompt_eval_count")
    pe_d = body.get("prompt_eval_duration")
    ev_n = body.get("eval_count")
    ev_d = body.get("eval_duration")
    return {
        "prompt_tokens": pe_n,
        "prefill_tok_s": (pe_n / (pe_d / 1e9)) if (pe_n and pe_d) else None,
        "gen_tokens": ev_n,
        "decode_tok_s": (ev_n / (ev_d / 1e9)) if (ev_n and ev_d) else None,
    }


# ---------------------------------------------------------------------------
# Derivations
# ---------------------------------------------------------------------------

def derive_summary(rows: list[dict]) -> dict:
    resident = [r for r in rows if r["fits_fully"] and r.get("size_vram_bytes")]
    max_fit = max((r["num_ctx"] for r in resident), default=None)
    # KV slope: GB per 10k tokens from the two extreme fully-resident points.
    kv_gb_per_10k = None
    if len(resident) >= 2:
        lo, hi = resident[0], resident[-1]
        dt = hi["num_ctx"] - lo["num_ctx"]
        dv = hi["size_vram_bytes"] - lo["size_vram_bytes"]
        if dt > 0:
            kv_gb_per_10k = (dv / dt) * 10000 / 1e9
    # Weights estimate: smallest-ctx resident size_vram minus its own KV.
    weights_gb = None
    if resident and kv_gb_per_10k is not None:
        lo = resident[0]
        weights_gb = (lo["size_vram_bytes"] / 1e9) - kv_gb_per_10k * lo["num_ctx"] / 10000
    return {
        "max_fully_resident_ctx": max_fit,
        "kv_gb_per_10k_tokens": round(kv_gb_per_10k, 4) if kv_gb_per_10k else None,
        "model_weights_gb_est": round(weights_gb, 3) if weights_gb else None,
    }


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main() -> None:
    p = argparse.ArgumentParser(description="Layer 4 T5: long-context VRAM/throughput sweep.")
    p.add_argument("--model", required=True)
    p.add_argument("--output", default="")
    p.add_argument("--ctx-lengths", default="4096,8192,16384,32768,65536,131072,262144",
                   help="Comma-separated num_ctx ladder")
    p.add_argument("--num-predict", type=int, default=64,
                   help="Tokens to generate for the throughput measurement")
    p.add_argument("--gpu-index", type=int, default=0)
    p.add_argument("--gen-timeout", type=int, default=600,
                   help="Per-request HTTP timeout (s); raise for big prefills")
    p.add_argument("--throughput", action="store_true",
                   help="Also measure decode tok/s with a real ~L-token prompt "
                        "(fully-resident ctx only). Off by default — VRAM-only is fast and safe.")
    p.add_argument("--smoke", action="store_true",
                   help="Only run the two smallest ctx steps to shake out parsing/output")
    args = p.parse_args()

    ladder = [int(x) for x in args.ctx_lengths.split(",") if x.strip()]
    if args.smoke:
        ladder = ladder[:2]
    slug = model_slug(args.model)
    out_path = args.output or f"results/longcontext-{slug}.json"

    print(f"[setup] model={args.model} host={_host()} ladder={ladder}")
    print(f"[setup] throughput={'on' if args.throughput else 'off'} smoke={args.smoke}")

    started = datetime.datetime.now(datetime.timezone.utc)
    rows: list[dict] = []
    for num_ctx in ladder:
        print(f"\n[ctx {num_ctx}] measuring VRAM (tiny prompt) ...", flush=True)
        try:
            row = measure_vram(args.model, num_ctx, args.gpu_index, args.gen_timeout)
        except (urllib.error.URLError, OSError, TimeoutError) as exc:
            print(f"  [error] load failed: {type(exc).__name__}: {exc}")
            rows.append({"num_ctx": num_ctx, "error": f"{type(exc).__name__}: {exc}",
                         "fits_fully": False})
            print("  [stop] treating as the fit ceiling; ending ladder.")
            break

        sv = row.get("size_vram_bytes")
        sz = row.get("size_bytes")
        frac = row.get("gpu_fraction")
        print(f"  size={sz/1e9:.2f}GB size_vram={sv/1e9:.2f}GB "
              f"({frac*100:.0f}% GPU) fits={row['fits_fully']}"
              if sv and sz else "  [warn] /api/ps returned no size; model unloaded?")

        if args.throughput and row["fits_fully"]:
            print(f"  measuring throughput (~{num_ctx}-token prompt) ...", flush=True)
            try:
                row["throughput"] = measure_throughput(
                    args.model, num_ctx, args.num_predict, args.gen_timeout)
                t = row["throughput"]
                print(f"  prompt_tokens={t['prompt_tokens']} "
                      f"prefill={t['prefill_tok_s']:.1f} tok/s "
                      f"decode={t['decode_tok_s']:.1f} tok/s"
                      if t.get("decode_tok_s") else "  [warn] no throughput numbers")
            except (urllib.error.URLError, OSError, TimeoutError) as exc:
                row["throughput"] = {"error": f"{type(exc).__name__}: {exc}"}
                print(f"  [error] throughput failed: {exc}")
        elif args.throughput and not row["fits_fully"]:
            print("  [skip] offloaded — skipping throughput (CPU prefill would be slow)")

        rows.append(row)
        # Once it offloads, larger ctx only offloads more; stop after recording it.
        if sv is not None and not row["fits_fully"]:
            print("  [stop] context no longer fully resident; ending ladder.")
            break

    summary = derive_summary(rows)
    payload = {
        "benchmark": "layer4-longcontext-t5",
        "model": args.model,
        "ollama_host": _host(),
        "run_started_at": started.isoformat(),
        "run_finished_at": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "ctx_ladder": ladder,
        "throughput_measured": args.throughput,
        "results": rows,
        "summary": summary,
    }
    os.makedirs(os.path.dirname(out_path) or ".", exist_ok=True)
    with open(out_path, "w", encoding="utf-8") as fh:
        fh.write(json.dumps(payload, indent=2) + "\n")

    print(f"\n[summary] max_fully_resident_ctx={summary['max_fully_resident_ctx']} "
          f"kv_gb_per_10k={summary['kv_gb_per_10k_tokens']} "
          f"weights_gb_est={summary['model_weights_gb_est']}")
    print(f"[done] written to {out_path}")


if __name__ == "__main__":
    main()
