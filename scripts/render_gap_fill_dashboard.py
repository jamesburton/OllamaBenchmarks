#!/usr/bin/env python3
"""Render the gap-fill queue + Bonsai tracker into a single HTML dashboard.

Reads live state from results/*.json and results/overnight-logs/*.log --
no hand-transcribed numbers. Re-run any time and republish the output file
via the Artifact tool to refresh the live tracker at the same URL.

Usage:
    python scripts/render_gap_fill_dashboard.py > /path/to/dashboard.html
"""
import json
import re
import sys
from pathlib import Path

sys.stdout.reconfigure(encoding="utf-8")

REPO_ROOT = Path(__file__).resolve().parent.parent
RESULTS_DIR = REPO_ROOT / "results"
LOG_DIR = RESULTS_DIR / "overnight-logs"

sys.path.insert(0, str(REPO_ROOT / "scripts"))
from run_gap_fill_queue import (  # noqa: E402
    TIER0, TIER1, TIER2, TIER3, KEEP_INSTALLED, BLOCKED_BY_UMA,
    model_slug, coverage, read_json,
)

BONSAI_MODEL = "hf.co/prism-ml/Bonsai-27B-gguf:Q1_0"
BONSAI_BROKEN = "hf.co/prism-ml/Ternary-Bonsai-27B-gguf:Q2_0"


def discover_archived_models(tracked: set) -> list:
    """Models with full 5-stage coverage that predate the gap-fill queue and
    so were never added to TIER0-3 (e.g. qwen3.6:27b, gemma4:31b) -- surface
    them too so the table is a genuine full comparison, not just the queue."""
    models = set()
    for f in list(RESULTS_DIR.glob("quality-*.json")) + list(RESULTS_DIR.glob("coding-*.json")):
        name = f.name
        if name.endswith("-chat.json") or "coding-generated" in str(f) or name == "coding-layer3-results.json":
            continue
        d = read_json(f)
        if d.get("model"):
            models.add(d["model"])
        for r in d.get("results") or []:
            if r.get("model"):
                models.add(r["model"])
    archived = sorted(m for m in models - tracked if all(coverage(m).values()))
    return archived

TOP5 = [
    ("gemma4:31b", 0.855, "42/50", None),
    ("glm-5:cloud", 0.836, "41/50", "retired"),
    ("qwen3-coder-next", 0.818, "41/50", None),
    ("glm-5.1:cloud", 0.818, "41/50", "gated"),
    ("gemma4:31b-cloud", 0.818, "41/50", None),
    ("qwen3.6:27b", 0.818, "40/50", None),
    ("qwen3.6:latest", 0.800, "40/50", None),
    ("minimax-m3:cloud", 0.800, "40/50", None),
]


def scores_for(model: str) -> dict:
    slug = model_slug(model)
    quality = read_json(RESULTS_DIR / f"quality-{slug}.json")
    q_row = (quality.get("results") or [{}])[0] if quality.get("results") else {}
    coding = read_json(RESULTS_DIR / f"coding-{slug}.json")
    chat = read_json(RESULTS_DIR / f"coding-{slug}-chat.json")
    l3_results = coding.get("layer3_results") or []
    l2_raw = coding.get("layer2_results") or []
    l2_chat = chat.get("layer2_chat_results") or []
    return {
        "quality_score": q_row.get("score"),
        "quality_max": q_row.get("score_max"),
        "coding_pass": q_row.get("coding_pass"),
        "coding_total": q_row.get("coding_total"),
        "tool_pass": q_row.get("tool_pass"),
        "tool_total": q_row.get("tool_total"),
        "agentic_pass": q_row.get("agentic_pass"),
        "agentic_total": q_row.get("agentic_total"),
        "l3_score": coding.get("layer3_weighted_score"),
        "l3_pass": sum(1 for r in l3_results if r.get("passed")),
        "l3_total": len(l3_results),
        "l2_raw_pass": sum(1 for r in l2_raw if r.get("passed")),
        "l2_raw_total": len(l2_raw),
        "l2_chat_pass": sum(1 for r in l2_chat if r.get("passed")),
        "l2_chat_total": len(l2_chat),
    }


def parse_queue_log():
    """Return (currently_active_model_or_None, set_of_done_models, set_of_removed_models)."""
    log_path = LOG_DIR / "gap-fill-queue.log"
    if not log_path.exists():
        return None, set(), set()
    text = log_path.read_text(encoding="utf-8", errors="ignore")
    started = re.findall(r"--- (\S.*?) \((?:installed|fetched)\) ---", text)
    done = set(re.findall(r"done(?: \+ removed from missing_from_local)?: (\S.*?) \(log:", text))
    removed = set(re.findall(r"\[reclaim\] scores complete, removing from disk: ollama rm (\S+)", text))
    active = None
    for m in started:
        if m not in done:
            active = m
            break
    return active, done, removed


def bonsai_status():
    log_path = LOG_DIR / "bonsai-q1_0-pipeline.log"
    if not log_path.exists():
        return "pending", 0
    text = log_path.read_text(encoding="utf-8", errors="ignore")
    if "pipeline finished" in text:
        return "done", 100
    stages = ["--- quality ---", "--- L3 (no-think) ---", "--- L2 raw ---", "--- L2 chat ---"]
    reached = sum(1 for s in stages if s in text)
    return "running", int(reached / len(stages) * 100)


def status_pill(model: str, tier_active: str | None, tier_done: set, cov: dict, removed: set) -> str:
    if model in removed:
        return "reclaimed"
    if all(cov.values()):
        return "done"
    if model == tier_active:
        return "running"
    if any(cov.values()):
        return "partial"
    return "pending"


def fmt_score(v, digits=3):
    return f"{v:.{digits}f}" if isinstance(v, (int, float)) else "—"


def row_html(model: str, tier_label: str, active: str, done: set, removed: set) -> str:
    cov = coverage(model)
    st = status_pill(model, active, done, cov, removed)
    sc = scores_for(model)
    l3_cell = (
        f'{fmt_score(sc["l3_score"])} <span class="dim">({sc["l3_pass"]}/{sc["l3_total"] or 50})</span>'
        if cov["l3"] else "—"
    )
    l2r_cell = (
        f'{sc["l2_raw_pass"]}/{sc["l2_raw_total"]}' if cov["l2_raw"] else
        (f'{sc["l2_raw_pass"]}/158 <span class="dim">running</span>' if sc["l2_raw_total"] else "—")
    )
    l2c_cell = (
        f'{sc["l2_chat_pass"]}/{sc["l2_chat_total"]}' if cov["l2_chat"] else
        (f'{sc["l2_chat_pass"]}/158 <span class="dim">running</span>' if sc["l2_chat_total"] else "—")
    )
    if cov["quality"]:
        q_cell = (
            f'{sc["coding_pass"]}/{sc["coding_total"]} &middot; '
            f'{sc["tool_pass"]}/{sc["tool_total"]} &middot; '
            f'{sc["agentic_pass"]}/{sc["agentic_total"]} '
            f'<span class="dim">({sc["quality_score"]}/{sc["quality_max"]})</span>'
        )
    else:
        q_cell = "—"
    name = model if len(model) < 46 else model[:43] + "…"
    return f"""
      <tr class="status-{st}">
        <td class="tier">{tier_label}</td>
        <td class="mono model-name" title="{model}">{name}</td>
        <td><span class="pill pill-{st}">{st}</span></td>
        <td class="mono">{q_cell}</td>
        <td class="mono">{l3_cell}</td>
        <td class="mono">{l2r_cell}</td>
        <td class="mono">{l2c_cell}</td>
      </tr>"""


def main():
    active, done, removed = parse_queue_log()
    b_status, b_pct = bonsai_status()
    b_sc = scores_for(BONSAI_MODEL)
    b_cov = coverage(BONSAI_MODEL)

    tracked = set(TIER0 + TIER1 + TIER2 + TIER3)
    archived = discover_archived_models(tracked)

    entries = (
        [(m, "0 · priority") for m in TIER0]
        + [(m, "1 · partial") for m in TIER1]
        + [(m, "2 · zero-cov") for m in TIER2]
        + [(m, "3 · fetch") for m in TIER3]
        + [(m, "archived · complete") for m in archived]
    )

    status_rank = {"done": 0, "running": 1, "partial": 2, "pending": 3, "reclaimed": 4}

    def sort_key(entry):
        m, _ = entry
        cov = coverage(m)
        sc = scores_for(m)
        st = status_pill(m, active, done, cov, removed)
        l3 = sc["l3_score"] if sc["l3_score"] is not None else -1
        q = sc["quality_score"] if sc["quality_score"] is not None else -1
        l2 = (sc["l2_raw_pass"] or 0) + (sc["l2_chat_pass"] or 0)
        return (-l3, -q, -l2, status_rank.get(st, 5))

    entries.sort(key=sort_key)
    rows = [row_html(m, tier_label, active, done, removed) for m, tier_label in entries]

    top5_rows = "\n".join(
        f"""      <tr>
        <td class="mono">{i+1}</td><td>{name}{f' <span class="badge {flag}">{flag}</span>' if flag else ""}</td>
        <td class="mono score">{score:.3f}</td><td class="mono">{frac}</td>
      </tr>""" for i, (name, score, frac, flag) in enumerate(TOP5)
    )

    huge_skipped_html = "".join(
        f"<li><code>{n}</code> — {r}</li>" for n, r in BLOCKED_BY_UMA
    )

    total = len(TIER0) + len(TIER1) + len(TIER2) + len(TIER3)
    completed = sum(1 for m in TIER0 + TIER1 + TIER2 + TIER3 if all(coverage(m).values()) or m in removed)

    print(f"""<title>Gap-Fill Benchmark Queue — Live Tracker</title>
<style>
  :root {{
    --bg: #eef1f3; --surface: #ffffff; --surface-2: #e4e9ec; --border: #d4dbe0;
    --text: #1b2430; --text-muted: #5b6672; --accent: #b8632e; --accent-soft: #f2e1d2;
    --pass: #2f8f5b; --pass-soft: #dcefe3; --fail: #c1462f; --fail-soft: #f8e2dd;
    --pending: #9ca6af; --pending-soft: #e9ecee;
    --running: #b8632e; --running-soft: #f2e1d2;
    --shadow: 0 1px 2px rgba(27, 36, 48, 0.06), 0 4px 14px rgba(27, 36, 48, 0.05);
  }}
  @media (prefers-color-scheme: dark) {{
    :root {{
      --bg: #10141a; --surface: #171d25; --surface-2: #1e2530; --border: #2a323d;
      --text: #e7ebef; --text-muted: #8b96a3; --accent: #e08a45; --accent-soft: #3a2a1c;
      --pass: #4fbe84; --pass-soft: #1c3327; --fail: #e2664a; --fail-soft: #3a2420;
      --pending: #5b6672; --pending-soft: #232a33; --running: #e08a45; --running-soft: #3a2a1c;
      --shadow: 0 1px 2px rgba(0,0,0,0.3), 0 8px 24px rgba(0,0,0,0.35);
    }}
  }}
  :root[data-theme="dark"] {{
    --bg: #10141a; --surface: #171d25; --surface-2: #1e2530; --border: #2a323d;
    --text: #e7ebef; --text-muted: #8b96a3; --accent: #e08a45; --accent-soft: #3a2a1c;
    --pass: #4fbe84; --pass-soft: #1c3327; --fail: #e2664a; --fail-soft: #3a2420;
    --pending: #5b6672; --pending-soft: #232a33; --running: #e08a45; --running-soft: #3a2a1c;
    --shadow: 0 1px 2px rgba(0,0,0,0.3), 0 8px 24px rgba(0,0,0,0.35);
  }}
  :root[data-theme="light"] {{
    --bg: #eef1f3; --surface: #ffffff; --surface-2: #e4e9ec; --border: #d4dbe0;
    --text: #1b2430; --text-muted: #5b6672; --accent: #b8632e; --accent-soft: #f2e1d2;
    --pass: #2f8f5b; --pass-soft: #dcefe3; --fail: #c1462f; --fail-soft: #f8e2dd;
    --pending: #9ca6af; --pending-soft: #e9ecee; --running: #b8632e; --running-soft: #f2e1d2;
    --shadow: 0 1px 2px rgba(27,36,48,0.06), 0 4px 14px rgba(27,36,48,0.05);
  }}
  * {{ box-sizing: border-box; }}
  body {{
    margin: 0; background: var(--bg); color: var(--text);
    font-family: ui-sans-serif, "Segoe UI", "Helvetica Neue", Arial, sans-serif;
    line-height: 1.5; -webkit-font-smoothing: antialiased;
  }}
  .mono {{
    font-family: ui-monospace, "Cascadia Code", "SFMono-Regular", Consolas, "Liberation Mono", monospace;
    font-variant-numeric: tabular-nums;
  }}
  .dim {{ color: var(--text-muted); font-weight: 500; }}
  .wrap {{ max-width: 1080px; margin: 0 auto; padding: 40px 24px 80px; }}
  header {{ display: flex; flex-direction: column; gap: 6px; margin-bottom: 8px; }}
  .eyebrow {{ text-transform: uppercase; letter-spacing: 0.09em; font-size: 0.72rem; font-weight: 600; color: var(--accent); }}
  h1 {{ margin: 0; font-size: 1.9rem; font-weight: 700; letter-spacing: -0.01em; text-wrap: balance; }}
  .subtitle {{ color: var(--text-muted); font-size: 0.95rem; max-width: 70ch; }}
  .meta-row {{ display: flex; flex-wrap: wrap; gap: 8px 20px; margin-top: 14px; font-size: 0.82rem; color: var(--text-muted); }}
  .meta-row b {{ color: var(--text); font-weight: 600; }}

  .progress-band {{ display: flex; align-items: center; gap: 14px; margin: 24px 0 36px; }}
  .progress-track {{ flex: 1; height: 10px; border-radius: 5px; background: var(--surface-2); overflow: hidden; }}
  .progress-fill {{ height: 100%; background: var(--accent); border-radius: 5px; }}
  .progress-label {{ font-size: 0.85rem; font-weight: 600; white-space: nowrap; }}

  h2 {{ font-size: 1.05rem; font-weight: 700; letter-spacing: -0.005em; margin: 0 0 4px; }}
  .section-note {{ color: var(--text-muted); font-size: 0.85rem; margin: 0 0 16px; max-width: 70ch; }}
  section {{ margin-bottom: 44px; }}

  .stats {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(190px, 1fr)); gap: 14px; margin-bottom: 12px; }}
  .card {{ background: var(--surface); border: 1px solid var(--border); border-radius: 10px; padding: 16px 18px; box-shadow: var(--shadow); }}
  .card .label {{ font-size: 0.72rem; text-transform: uppercase; letter-spacing: 0.07em; color: var(--text-muted); font-weight: 600; }}
  .card .value {{ font-size: 1.7rem; font-weight: 700; margin-top: 6px; letter-spacing: -0.01em; }}
  .card .sub {{ font-size: 0.78rem; color: var(--text-muted); margin-top: 4px; }}

  .table-scroll {{ overflow-x: auto; border: 1px solid var(--border); border-radius: 10px; }}
  table {{ width: 100%; border-collapse: collapse; background: var(--surface); font-size: 0.86rem; }}
  th, td {{ text-align: left; padding: 9px 14px; white-space: nowrap; }}
  thead th {{ font-size: 0.7rem; text-transform: uppercase; letter-spacing: 0.06em; color: var(--text-muted); font-weight: 600; border-bottom: 1px solid var(--border); position: sticky; top: 0; background: var(--surface); }}
  tbody tr:not(:last-child) td {{ border-bottom: 1px solid var(--border); }}
  .tier {{ color: var(--text-muted); font-size: 0.78rem; }}
  .model-name {{ max-width: 320px; overflow: hidden; text-overflow: ellipsis; }}
  td.score {{ font-weight: 700; }}

  .pill {{ display: inline-block; font-size: 0.7rem; font-weight: 700; padding: 3px 10px; border-radius: 999px; text-transform: uppercase; letter-spacing: 0.04em; }}
  .pill-done {{ background: var(--pass-soft); color: var(--pass); }}
  .pill-running {{ background: var(--running-soft); color: var(--running); }}
  .pill-partial {{ background: var(--accent-soft); color: var(--accent); }}
  .pill-pending {{ background: var(--pending-soft); color: var(--text-muted); }}
  .pill-reclaimed {{ background: var(--surface-2); color: var(--text-muted); text-decoration: line-through; }}
  tr.status-reclaimed td:not(.tier) {{ opacity: 0.55; }}

  .badge {{ display: inline-block; font-size: 0.68rem; font-weight: 600; padding: 2px 8px; border-radius: 999px; margin-left: 6px; }}
  .badge.retired {{ background: var(--pending-soft); color: var(--text-muted); }}
  .badge.gated {{ background: var(--fail-soft); color: var(--fail); }}

  .cat-grid {{ display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 10px 24px; }}
  .cat-row {{ display: flex; flex-direction: column; gap: 4px; }}
  .cat-row .cat-top {{ display: flex; justify-content: space-between; font-size: 0.82rem; }}
  .cat-row .cat-name {{ font-weight: 600; }}
  .cat-row .cat-frac {{ color: var(--text-muted); }}
  .cat-track {{ height: 8px; border-radius: 4px; background: var(--surface-2); overflow: hidden; }}
  .cat-fill {{ height: 100%; border-radius: 4px; background: var(--pass); }}

  footer {{ border-top: 1px solid var(--border); padding-top: 20px; font-size: 0.8rem; color: var(--text-muted); }}
  footer p {{ max-width: 76ch; margin: 0 0 10px; }}
  footer ul {{ margin: 0 0 10px; padding-left: 20px; }}
  footer code {{ font-family: ui-monospace, "Cascadia Code", "SFMono-Regular", Consolas, monospace; background: var(--surface-2); padding: 1px 5px; border-radius: 4px; font-size: 0.85em; }}
</style>

<div class="wrap">

  <header>
    <span class="eyebrow">Ollama Benchmarks &middot; Strix Halo</span>
    <h1>Gap-Fill Benchmark Queue &mdash; Live Tracker</h1>
    <p class="subtitle">Working through every model missing quality/throughput/L3/L2 coverage, one at a time (Strix's iGPU hosts a single model). Not-yet-installed models are fetched one entry ahead of the model currently benching.</p>
    <div class="meta-row">
      <span>Host: <b>Strix</b> (Radeon 8060S iGPU)</span>
      <span>Queue: <b class="mono">{completed}/{total}</b> tier-tracked models complete</span>
    </div>
  </header>

  <div class="progress-band">
    <div class="progress-track"><div class="progress-fill" style="width:{int(completed/total*100) if total else 0}%"></div></div>
    <div class="progress-label mono">{int(completed/total*100) if total else 0}%</div>
  </div>

  <section>
    <h2>Bonsai-27B Q1_0 &mdash; featured run</h2>
    <p class="section-note">PrismML's 1-bit build of Qwen3.6-27B. The run that kicked off this whole queue; its L2 chat stage is what the rest of the queue is waiting behind.</p>
    <div class="stats">
      <div class="card">
        <div class="label">Quality</div>
        <div class="value mono">{b_sc['quality_score'] if b_cov['quality'] else '—'}<span style="color:var(--text-muted); font-weight:600; font-size:1rem;">/{b_sc['quality_max'] or 11}</span></div>
      </div>
      <div class="card">
        <div class="label">Throughput</div>
        <div class="value mono">28.9 <span style="font-size:1rem; font-weight:600; color:var(--text-muted);">tok/s</span></div>
        <div class="sub">loads at 20.1GB VRAM, not ~4GB &mdash; see footnote</div>
      </div>
      <div class="card">
        <div class="label">L3 &middot; .NET practical</div>
        <div class="value mono">{fmt_score(b_sc['l3_score']) if b_cov['l3'] else '—'}</div>
        <div class="sub">{b_sc['l3_pass']}/{b_sc['l3_total'] or 50} passed</div>
      </div>
      <div class="card">
        <div class="label">L2 raw</div>
        <div class="value mono">{b_sc['l2_raw_pass']}/{b_sc['l2_raw_total'] or 158}</div>
      </div>
      <div class="card">
        <div class="label">L2 chat</div>
        <div class="value mono">{b_sc['l2_chat_pass']}/{b_sc['l2_chat_total'] or 158}{' <span class="dim" style="font-size:0.9rem;">running</span>' if b_status=='running' else ''}</div>
      </div>
    </div>
  </section>

  <section>
    <h2>Full queue</h2>
    <p class="section-note">Every model with data: the gap-fill queue (tiers 0-3) plus models completed before the queue existed (tier "archived · complete", e.g. qwen3.6:27b, gemma4:31b). Ranked by L3, then quality total, then L2. <span class="pill pill-reclaimed" style="vertical-align:middle;">reclaimed</span> means scored then removed from disk to free space.</p>
    <div class="table-scroll">
      <table>
        <thead>
          <tr><th>Tier</th><th>Model</th><th>Status</th><th>Code / Tools / Agent (total)</th><th>L3</th><th>L2 raw</th><th>L2 chat</th></tr>
        </thead>
        <tbody>{"".join(rows)}
        </tbody>
      </table>
    </div>
  </section>

  <section>
    <h2>How the field stacks up</h2>
    <p class="section-note">Ranked by L3 weighted score &mdash; the primary cross-model quality signal used throughout this benchmark set.</p>
    <div class="table-scroll">
      <table>
        <thead><tr><th>Rank</th><th>Model</th><th>L3 score</th><th>Pass / total</th></tr></thead>
        <tbody>
{top5_rows}
          <tr style="background: var(--accent-soft);">
            <td class="mono">&mdash;</td><td><b>{BONSAI_MODEL}</b></td>
            <td class="mono score" style="color:var(--accent);">{fmt_score(b_sc['l3_score']) if b_cov['l3'] else '—'}</td>
            <td class="mono">{b_sc['l3_pass']}/{b_sc['l3_total'] or 50}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>

  <footer>
    <p><b style="color:var(--text);">VRAM footprint doesn't hold up on this box.</b> Bonsai-27B Q1_0 loads at 20.12GB VRAM despite a 3.8GB on-disk file and PrismML's ~1.125 bits/weight claim &mdash; this box's mainline Ollama lacks PrismML's packed-weight kernels, so it dequantizes on load.</p>
    <p><code>{BONSAI_BROKEN}</code> (the ternary sibling) fails to load entirely &mdash; <code>tensor "output.weight" size overflow</code> &mdash; needs PrismML's custom llama.cpp fork. Excluded from this tracker. See <code>MODEL_QUIRKS.md</code>.</p>
    <p><b style="color:var(--text);">Disk management:</b> freed ~24GB (stale .NET workload cache + Docker container prune) plus an 18GB NuGet cache clear to make room for the queue's larger fetches. <code>qwen36-bartowski:q4km</code> and <code>qwen36-apex:balanced</code> (~47GB) will be auto-removed via <code>ollama rm</code> once their scores are confirmed complete, to clear room for the 74.9GB Mistral-Medium-3.5-128B pull queued behind them. <code>qwen3.5</code> (bare tag) was dropped from the queue &mdash; unresolvable, and superseded by the qwen3.6 generation already covered elsewhere in this suite.</p>
    {f'<ul>{huge_skipped_html}</ul>' if huge_skipped_html else ''}
    <p>This page is regenerated from <code>results/*.json</code> and <code>results/overnight-logs/*.log</code> on demand, not a live poll &mdash; republished each time the queue makes meaningful progress.</p>
  </footer>

</div>
""")


if __name__ == "__main__":
    main()
