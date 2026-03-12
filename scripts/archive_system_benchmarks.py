import argparse
import datetime as dt
import json
import pathlib
import shutil
from typing import Any

from collect_host_info import build_host_info


DEFAULT_RESULTS = pathlib.Path("results")
DEFAULT_FILES = {
    "throughput": DEFAULT_RESULTS / "throughput-resource-current.json",
    "quality": DEFAULT_RESULTS / "quality-current.json",
    "backend": DEFAULT_RESULTS / "backend-comparison-current.json",
    "sweep": DEFAULT_RESULTS / "optimization-sweep-current.json",
}
PRIMARY_MODEL_PREFERENCE = [
    "qwen3-coder-next:latest",
    "lfm2:24b",
    "granite4:32b-a9b-h",
]


def load_json(path: pathlib.Path) -> dict[str, Any]:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def choose_top_models(throughput: dict[str, Any], quality: dict[str, Any]) -> dict[str, Any]:
    quality_map = {row["model"]: row for row in quality["results"]}
    rows = []
    for row in throughput["results"]:
        quality_row = quality_map.get(row["model"], {})
        rows.append(
            {
                "model": row["model"],
                "toks_per_s": row["toks_per_s"],
                "score": quality_row.get("score", 0),
                "score_max": quality_row.get("score_max", 0),
            }
        )
    rows_by_speed = sorted(rows, key=lambda item: item["toks_per_s"], reverse=True)
    full_pass = [row for row in rows if row["score"] == row["score_max"] and row["score_max"]]
    best_balanced = sorted(
        full_pass,
        key=lambda item: (item["score"], item["toks_per_s"]),
        reverse=True,
    )
    primary = None
    for preferred_model in PRIMARY_MODEL_PREFERENCE:
        primary = next((row for row in full_pass if row["model"] == preferred_model), None)
        if primary:
            break
    if not primary and best_balanced:
        primary = best_balanced[0]
    return {
        "fastest_model": rows_by_speed[0] if rows_by_speed else None,
        "primary_coding_model": primary,
        "best_balanced_model": best_balanced[0] if best_balanced else None,
        "full_quality_models": [row["model"] for row in best_balanced],
    }


def summarize_backend(backend: dict[str, Any]) -> str | None:
    ok = [row for row in backend["results"] if row.get("status") == "ok"]
    if not ok:
        return None
    ranked = sorted(ok, key=lambda item: item.get("toks_per_s", 0), reverse=True)
    leader = ranked[0]
    runner_up = ranked[1] if len(ranked) > 1 else None
    if runner_up:
        return (
            f"{leader['lib']} led at {leader['toks_per_s']} tok/s; "
            f"{runner_up['lib']} followed at {runner_up['toks_per_s']} tok/s."
        )
    return f"{leader['lib']} led at {leader['toks_per_s']} tok/s."


def find_latest_session_summary(results_root: pathlib.Path) -> pathlib.Path | None:
    session_summaries = sorted(results_root.glob("session-*.md"))
    if not session_summaries:
        return None
    return session_summaries[-1]


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--results-root", default=str(DEFAULT_RESULTS))
    parser.add_argument("--systems-root", default=str(DEFAULT_RESULTS / "systems"))
    parser.add_argument("--label")
    args = parser.parse_args()

    results_root = pathlib.Path(args.results_root)
    systems_root = pathlib.Path(args.systems_root)
    host_info = build_host_info()
    label = args.label or dt.datetime.now().strftime("%Y%m%d")
    system_dir = systems_root / f"{host_info['host_slug']}-{label}"
    system_dir.mkdir(parents=True, exist_ok=True)

    copied_files: dict[str, str] = {}
    loaded_json: dict[str, dict[str, Any]] = {}
    source_files = dict(DEFAULT_FILES)
    latest_session_summary = find_latest_session_summary(results_root)
    if latest_session_summary:
        source_files["session_summary"] = latest_session_summary

    for key, relative_path in source_files.items():
        source = relative_path if relative_path.is_absolute() else results_root / relative_path.name
        if not source.exists():
            continue
        destination = system_dir / relative_path.name
        shutil.copy2(source, destination)
        copied_files[key] = destination.name
        if source.suffix.lower() == ".json":
            loaded_json[key] = load_json(source)

    host_info_path = system_dir / "host-info.json"
    host_info_path.write_text(json.dumps(host_info, indent=2) + "\n", encoding="utf-8")

    throughput = loaded_json.get("throughput")
    quality = loaded_json.get("quality")
    backend = loaded_json.get("backend")
    summary = choose_top_models(throughput, quality) if throughput and quality else {}
    manifest = {
        "captured_at": dt.datetime.now(dt.timezone.utc).isoformat(),
        "system_id": system_dir.name,
        "system_dir": str(system_dir.resolve()),
        "host_info_file": host_info_path.name,
        "artifacts": copied_files,
        "host": host_info,
        "summary": {
            **summary,
            "backend_summary": summarize_backend(backend) if backend else None,
        },
    }
    manifest_path = system_dir / "manifest.json"
    manifest_path.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(manifest, indent=2))


if __name__ == "__main__":
    main()
