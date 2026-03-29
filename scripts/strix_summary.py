#!/usr/bin/env python3
"""Generate a full performance summary table for Strix machine."""
import json
import os
import glob
import re


def main():
    # Collect quality results
    quality = {}
    for f in glob.glob("results/quality-*.json"):
        try:
            d = json.load(open(f))
            host = d.get("host_details", {}).get("hostname", "")
            if not host.upper().startswith("STRIX"):
                continue
            for r in d.get("results", []):
                m = r["model"]
                quality[m] = {"score": r.get("score", 0), "max": r.get("score_max", 5)}
        except Exception:
            pass

    # Collect throughput results
    throughput = {}
    for f in glob.glob("results/throughput-resource-*.json"):
        try:
            d = json.load(open(f))
            host = d.get("host_details", {}).get("hostname", "") or d.get("host", "")
            if not host.upper().startswith("STRIX"):
                continue
            for r in d.get("results", []):
                m = r["model"]
                throughput[m] = {
                    "toks": r.get("toks_per_s", 0),
                    "ram": r.get("ram_peak_gb", 0),
                    "gpu": r.get("gpu_mem_peak_gb", 0),
                    "load_s": r.get("load_s", 0),
                }
        except Exception:
            pass

    # Collect coding L3 results
    coding = {}
    for f in glob.glob("results/coding-*.json"):
        if "generated" in f or "current" in f or "layer3-results" in f:
            continue
        try:
            d = json.load(open(f))
            # Per-model checkpoint format: top-level model + layer3_results
            m = d.get("model", "")
            l3_results = d.get("layer3_results", [])
            l2 = d.get("layer2_pass_rate")
            if m and l3_results:
                passed = sum(1 for r in l3_results if isinstance(r, dict) and r.get("passed"))
                total = len(l3_results)
                coding[m] = {"passed": passed, "total": total, "l2": l2}
            # Composite format: results[] array with per-model summaries
            for r in d.get("results", []):
                if not isinstance(r, dict) or "model" not in r:
                    continue
                rm = r["model"]
                l3s = r.get("layer3_weighted_score", 0)
                # Estimate passed from weighted score (20 tasks)
                # weighted score ~= passed/total for weight-1 tasks
                est_passed = round(l3s * 20 / 1.0) if l3s else 0
                l2r = r.get("layer2_pass_rate")
                if rm not in coding or coding[rm].get("total", 0) == 0:
                    coding[rm] = {"passed": est_passed, "total": 20, "l2": l2r, "estimated": True}
        except Exception:
            pass

    # Collect PPL results
    ppl = {}
    for f in glob.glob("results/ppl-*.json"):
        try:
            d = json.load(open(f))
            host = d.get("host_details", {}).get("hostname", "")
            if not host.upper().startswith("STRIX"):
                continue
            for r in d.get("results", []):
                m = r["model"]
                p = r.get("ppl_proxy")
                if p is not None:
                    ppl[m] = p
        except Exception:
            pass

    # Merge all models
    all_models = set()
    all_models.update(quality.keys())
    all_models.update(throughput.keys())
    all_models.update(coding.keys())

    # Build rows
    rows = []
    for m in all_models:
        q = quality.get(m, {})
        t = throughput.get(m, {})
        c = coding.get(m, {})
        p = ppl.get(m)

        q_str = f"{q['score']}/{q['max']}" if q else "-"
        t_str = f"{t['toks']:.1f}" if t.get("toks") else "-"
        ram_str = f"{t['ram']:.1f}" if t.get("ram") else "-"
        gpu_str = f"{t['gpu']:.1f}" if t.get("gpu") else "-"
        c_str = f"{c['passed']}/{c['total']}" if c.get("total") else "-"
        l2_str = f"{c['l2']:.0%}" if c.get("l2") is not None else "-"
        p_str = f"{p:.2f}" if p else "-"

        sort_key = c.get("passed", -1)
        rows.append((sort_key, m, q_str, t_str, ram_str, gpu_str, c_str, l2_str, p_str))

    # Print
    hdr = f"{'Model':<55} {'Qual':>5} {'tok/s':>7} {'RAM':>6} {'GPU':>6} {'L3':>7} {'L2':>5} {'PPL':>6}"
    print(hdr)
    print("-" * len(hdr))
    for _, m, q_str, t_str, ram_str, gpu_str, c_str, l2_str, p_str in sorted(rows, key=lambda x: -x[0]):
        print(f"{m:<55} {q_str:>5} {t_str:>7} {ram_str:>6} {gpu_str:>6} {c_str:>7} {l2_str:>5} {p_str:>6}")

    print(f"\n{len(rows)} models on Strix")


if __name__ == "__main__":
    main()
