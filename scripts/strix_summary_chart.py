#!/usr/bin/env python3
"""Build a single Strix benchmark chart: speed (desc), then L3 (desc), then L2 (desc), then quality.

Models without Strix throughput data go at the end with only their test scores.
Reads all results/*.json composite, throughput-resource-*.json and quality-*.json
files for installed Ollama models (plus a few llama-server-backended ones).
"""
import json
import re
from pathlib import Path

RESULTS = Path(r"C:\Development\OllamaBenchmarks\results")


def slug(model: str) -> str:
    if model.endswith(":latest"):
        model = model[: -len(":latest")]
    s = re.sub(r"[:/\\]", "_", model)
    return re.sub(r"[^\w.\-]", "_", s)


def load_json(p: Path):
    if not p.exists():
        return None
    try:
        return json.loads(p.read_text(encoding="utf-8"))
    except Exception:
        return None


def is_strix(throughput_blob) -> bool:
    """Throughput blob includes host_details with hostname; Strix host is 'STRIX'."""
    if not throughput_blob:
        return False
    host = throughput_blob.get("host") or (throughput_blob.get("host_details") or {}).get("hostname")
    return (host or "").upper() == "STRIX"


def get_throughput_row(model: str):
    s = slug(model)
    p = RESULTS / f"throughput-resource-{s}.json"
    d = load_json(p)
    if not d or not is_strix(d):
        return None
    rows = d.get("results", [])
    for r in rows:
        if r.get("model") == model:
            return r
    return rows[0] if rows else None


def get_quality(model: str):
    s = slug(model)
    d = load_json(RESULTS / f"quality-{s}.json")
    if not d:
        return None
    for r in d.get("results", []):
        if r.get("model") == model:
            return r
    rs = d.get("results", [])
    return rs[0] if rs else None


def get_coding(model: str):
    s = slug(model)
    d = load_json(RESULTS / f"coding-{s}.json")
    # Fallback: standalone L3 file
    if not d:
        l3 = load_json(RESULTS / f"coding-layer3-{s}.json")
        if l3:
            return {"layer3_results": l3.get("layer3_results") or (l3.get("results", [{}])[0].get("layer3_results", [])),
                    "layer3_weighted_score": l3.get("layer3_weighted_score") or (l3.get("results", [{}])[0].get("layer3_weighted_score")),
                    "layer2_results": []}
    return d or {}


MODELS = [
    # Strix-tested set (installed locally)
    "qwen3.6", "qwen3.6:27b",
    "qwen3-coder-next", "qwen3:14b",
    "qwen3.5",  # = 9b
    "qwen3.5:4b",
    "gemma4", "gemma4:e2b", "gemma4:26b", "gemma4:31b",
    "gemma3:27b", "gemma3:4b",
    "granite4:7b-a1b-h", "granite4:32b-a9b-h",
    "glm-4.7-flash", "glm-4.7-flash-reap-toolfix",
    "nemotron-3-nano", "nemotron-3-nano:4b", "nemotron-3-nano:30b-a3b-q8_0",
    "nemotron-3-super", "nemotron-cascade-2",
    "nemotron-nano-9b-v2-toolfix",
    "gpt-oss:20b", "gpt-oss:120b",
    "llama4:16x17b",
    "ministral-3:14b", "mistral-small",
    "omnicoder:9b-q4_k_m",
    "hf.co/bartowski/Tesslate_OmniCoder-9B-GGUF:Q4_K_M",
    "trinity-mini:Q4_K_M",
    "lfm2.5-thinking:1.2b", "LFM2-2.6b-tools", "sam860/LFM2:2.6b",
    "phi4-mini", "zac/phi4-tools",
    "RogerBen/qwen3.5-35b-opus-distill",
    "hf.co/mradermacher/shenwen-coderV2-Instruct-GGUF:Q8_0",
    "hf.co/bartowski/kai-os_Carnice-V2-27b-GGUF:Q4_K_M",
    "hf.co/bartowski/cerebras_GLM-4.5-Air-REAP-82B-A12B-GGUF:IQ4_XS",
    "hf.co/mradermacher/Qwen3-235B-A22B-abliterated-i1-GGUF:i1-Q2_K",
    "devstral-small-2:24b-instruct-2512-q4_K_M",
    "devstral-small-2:24b-instruct-2512-q8_0",
]

def fmt_score(passed, total, score):
    if total is None or total == 0:
        if score is None:
            return "-"
        return f"{score:.4f}"
    return f"{passed}/{total} ({score:.3f})" if score is not None else f"{passed}/{total}"


rows = []
for model in MODELS:
    t = get_throughput_row(model)
    q = get_quality(model)
    c = get_coding(model)

    toks_per_s = (t or {}).get("toks_per_s")
    ram_peak = (t or {}).get("ram_peak_gb")
    gpu_mem = (t or {}).get("gpu_mem_peak_gb")

    l3_results = (c or {}).get("layer3_results", []) or []
    l3_pass = sum(1 for r in l3_results if r.get("passed"))
    l3_tot = len(l3_results)
    l3_score = (c or {}).get("layer3_weighted_score")

    l2_pass = (c or {}).get("layer2_passed")
    l2_tot  = (c or {}).get("layer2_total")
    l2_score = (c or {}).get("layer2_pass_rate")

    quality_score = (q or {}).get("score")
    quality_max = (q or {}).get("score_max")

    rows.append({
        "model": model,
        "toks_per_s": toks_per_s,
        "ram_peak": ram_peak,
        "gpu_mem": gpu_mem,
        "l3_pass": l3_pass, "l3_tot": l3_tot, "l3_score": l3_score,
        "l2_pass": l2_pass, "l2_tot": l2_tot, "l2_score": l2_score,
        "quality": quality_score, "quality_max": quality_max,
    })


def sort_key(r):
    # Speed desc — None speed sinks to end via large negative for sort-desc trick.
    # Then L3 desc, L2 desc, quality desc.
    spd = r["toks_per_s"]
    has_speed = spd is not None
    return (
        0 if has_speed else 1,                 # speed-known first
        -(spd if has_speed else 0),            # speed desc
        -(r["l3_score"] or 0),                 # L3 desc
        -(r["l2_score"] or 0),                 # L2 desc
        -(r["quality"] or 0),                  # quality desc
    )


rows.sort(key=sort_key)

# Render
print(f"{'Model':<58} {'tok/s':>7}  {'GPU GB':>7}  {'L3':<14} {'L2':<16} {'Quality':<10}")
print("-" * 130)
for r in rows:
    name = r["model"]
    spd = f"{r['toks_per_s']:.2f}" if r["toks_per_s"] is not None else "-"
    gm = f"{r['gpu_mem']:.1f}" if r["gpu_mem"] is not None else "-"
    l3 = fmt_score(r["l3_pass"], r["l3_tot"], r["l3_score"])
    l2 = fmt_score(r["l2_pass"], r["l2_tot"], r["l2_score"])
    qs = f"{r['quality']}/{r['quality_max']}" if r["quality"] is not None else "-"
    print(f"{name:<58} {spd:>7}  {gm:>7}  {l3:<14} {l2:<16} {qs:<10}")
