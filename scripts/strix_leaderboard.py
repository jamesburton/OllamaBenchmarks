#!/usr/bin/env python3
"""Combined Strix leaderboard: tok/s, L3, L2 (best of raw/chat), Quality.

Sort: by best-L2 desc (raw or chat, whichever is higher), then L3 desc, then tok/s desc.
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


def load(p: Path):
    if not p.exists():
        return None
    try:
        return json.loads(p.read_text(encoding="utf-8"))
    except Exception:
        return None


def host_is_strix(blob) -> bool:
    if not blob:
        return False
    h = blob.get("host") or (blob.get("host_details") or {}).get("hostname")
    return (h or "").upper() == "STRIX"


def strix_tps(model: str):
    p = RESULTS / f"throughput-resource-{slug(model)}.json"
    d = load(p)
    if not d or not host_is_strix(d):
        return None
    rs = d.get("results", [])
    for r in rs:
        if r.get("model") == model:
            return r.get("toks_per_s")
    return rs[0].get("toks_per_s") if rs else None


def l3_score(model: str):
    s = slug(model)
    d = load(RESULTS / f"coding-{s}.json")
    if d and d.get("layer3_results"):
        return d.get("layer3_weighted_score"), len(d["layer3_results"]), sum(1 for r in d["layer3_results"] if r.get("passed"))
    d = load(RESULTS / f"coding-layer3-{s}.json")
    if d:
        if d.get("results"):
            r = d["results"][0]
            return r.get("layer3_weighted_score"), len(r.get("layer3_results", [])), sum(1 for x in r.get("layer3_results", []) if x.get("passed"))
        if d.get("layer3_results"):
            return d.get("layer3_weighted_score"), len(d["layer3_results"]), sum(1 for r in d["layer3_results"] if r.get("passed"))
    return None, 0, 0


def l3_think_score(model: str):
    s = slug(model)
    d = load(RESULTS / f"coding-{s}-think.json")
    if d and d.get("layer3_results"):
        return d.get("layer3_weighted_score"), len(d["layer3_results"]), sum(1 for r in d["layer3_results"] if r.get("passed"))
    return None, 0, 0


def l2_raw(model: str):
    d = load(RESULTS / f"coding-{slug(model)}.json")
    if not d:
        return None, 0, 0
    rate = d.get("layer2_pass_rate")
    total = d.get("layer2_total", 0)
    passed = d.get("layer2_passed", 0)
    if rate is None and not total:
        return None, 0, 0
    return rate, total, passed


def l2_chat(model: str):
    d = load(RESULTS / f"coding-{slug(model)}-chat.json")
    if not d:
        return None, 0, 0
    rate = d.get("layer2_chat_pass_rate")
    total = d.get("layer2_chat_total", 0)
    passed = d.get("layer2_chat_passed", 0)
    if rate is None and not total:
        return None, 0, 0
    return rate, total, passed


def quality(model: str):
    d = load(RESULTS / f"quality-{slug(model)}.json")
    if not d:
        return None, 0
    for r in d.get("results", []):
        if r.get("model") == model:
            return r.get("score"), r.get("score_max")
    rs = d.get("results", [])
    if rs:
        return rs[0].get("score"), rs[0].get("score_max")
    return None, 0


MODELS = [
    "qwen3.6", "qwen3.6:27b", "qwen3-coder-next", "qwen3:14b",
    "qwen3.5", "qwen3.5:4b",
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


rows = []
for m in MODELS:
    tps = strix_tps(m)
    l3s, l3t, l3p = l3_score(m)
    l3ts, _, l3tp = l3_think_score(m)
    l2rs, l2rt, l2rp = l2_raw(m)
    l2cs, l2ct, l2cp = l2_chat(m)
    qs, qm = quality(m)

    # Best L2 (raw vs chat, whichever has a real score)
    best_l2 = max((l2rs or -1), (l2cs or -1))
    if best_l2 < 0:
        best_l2 = None
    rows.append({
        "model": m, "tps": tps,
        "l3_score": l3s, "l3_p": l3p, "l3_t": l3t,
        "l3_think_score": l3ts, "l3_think_p": l3tp,
        "l2_raw_rate": l2rs, "l2_raw_p": l2rp, "l2_raw_t": l2rt,
        "l2_chat_rate": l2cs, "l2_chat_p": l2cp, "l2_chat_t": l2ct,
        "best_l2": best_l2,
        "quality": qs, "quality_max": qm,
    })


def sort_key(r):
    return (
        -(r["best_l2"] or -1),         # best L2 desc
        -(r["l3_score"] or -1),        # L3 desc
        -(r["tps"] or -1),             # tok/s desc
    )


rows.sort(key=sort_key)


def fmt_score_pct(p, t, rate, asterisk=""):
    if rate is None or t == 0:
        return "-"
    return f"{p:>3}/{t} ({rate*100:>4.1f}%){asterisk}"


def fmt_tps(v):
    if v is None:
        return "      -"
    return f"{v:6.2f}"


print(f"{'Model':<58} {'tok/s':>7}  {'L3 no-think':<14} {'L3 think':<14} {'L2 raw':<17} {'L2 chat':<17} {'Quality':<10}")
print("-" * 145)
for r in rows:
    name = r["model"]
    tps = fmt_tps(r["tps"])
    l3 = fmt_score_pct(r["l3_p"], r["l3_t"], r["l3_score"])
    l3t = fmt_score_pct(r["l3_think_p"], r.get("l3_think_p", 0) and r["l3_t"] or 0, r["l3_think_score"])
    l2r = fmt_score_pct(r["l2_raw_p"], r["l2_raw_t"], r["l2_raw_rate"], "")
    l2c = fmt_score_pct(r["l2_chat_p"], r["l2_chat_t"], r["l2_chat_rate"], "")
    qs = f"{r['quality']}/{r['quality_max']}" if r["quality"] is not None else "-"
    print(f"{name:<58} {tps:>7}  {l3:<14} {l3t:<14} {l2r:<17} {l2c:<17} {qs:<10}")
