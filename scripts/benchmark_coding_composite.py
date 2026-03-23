"""Composite scorer for the coding benchmark.

Combines Layer 1 (EvalPlus HumanEval+), Layer 2 (MultiPL-E C#), and
Layer 3 (Custom .NET Practical Suite) into final per-model rankings and
writes results/coding-current.json.
"""

import argparse
import datetime
import glob
import json
import os
import sys


def load_checkpoint(path: str) -> dict | None:
    """Load and return a checkpoint JSON file, or None on error."""
    try:
        with open(path, "r", encoding="utf-8") as fh:
            return json.load(fh)
    except (OSError, json.JSONDecodeError) as exc:
        print(f"Warning: could not read {path}: {exc}", file=sys.stderr)
        return None


def compute_composite(layer1: float, layer2: float, layer3: float) -> float:
    """Compute weighted composite score from the three layer scores."""
    return 0.15 * layer1 + 0.25 * layer2 + 0.60 * layer3


def update_checkpoint(path: str, composite_score: float) -> None:
    """Write composite_score back into the per-model checkpoint file."""
    data = load_checkpoint(path)
    if data is None:
        return
    data["composite_score"] = round(composite_score, 6)
    try:
        with open(path, "w", encoding="utf-8") as fh:
            json.dump(data, fh, indent=2)
    except OSError as exc:
        print(f"Warning: could not update {path}: {exc}", file=sys.stderr)


def print_leaderboard(results: list[dict]) -> None:
    """Print a formatted leaderboard table to stdout."""
    header = f"{'Rank':>4}  {'Model':<32}  {'Composite':>9}  {'L1(Py)':>6}  {'L2(C#)':>6}  {'L3(.NET)':>8}"
    separator = f"{'----':>4}  {'-----':<32}  {'---------':>9}  {'------':>6}  {'------':>6}  {'--------':>8}"
    print(header)
    print(separator)
    for entry in results:
        rank = entry["rank"]
        model = entry["model"]
        composite = entry["composite_score"]
        l1 = entry["layer1_pass_rate"]
        l2 = entry["layer2_pass_rate"]
        l3 = entry["layer3_weighted_score"]
        print(
            f"{rank:>4}   {model:<32}  {composite:>9.3f}  {l1:>6.3f}  {l2:>6.3f}  {l3:>8.3f}"
        )


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Combine coding benchmark layers into final rankings."
    )
    parser.add_argument(
        "--checkpoint-dir",
        default="results",
        help="Directory containing per-model coding-*.json checkpoint files (default: results)",
    )
    parser.add_argument(
        "--output",
        default="results/coding-current.json",
        help="Output path for the aggregate rankings file (default: results/coding-current.json)",
    )
    args = parser.parse_args()

    checkpoint_dir = args.checkpoint_dir
    output_path = args.output

    # Normalise output path for exclusion comparison
    output_abs = os.path.abspath(output_path)

    # Glob all coding-*.json files in the checkpoint dir
    pattern = os.path.join(checkpoint_dir, "coding-*.json")
    all_files = sorted(glob.glob(pattern))

    # Filter out the aggregate output file and anything under coding-generated/
    checkpoint_files = []
    for path in all_files:
        abs_path = os.path.abspath(path)
        if abs_path == output_abs:
            continue
        # Exclude files inside coding-generated/ subdirectory
        norm = abs_path.replace("\\", "/")
        if "/coding-generated/" in norm:
            continue
        checkpoint_files.append(path)

    if not checkpoint_files:
        print(
            f"No checkpoint files found matching '{pattern}' (excluding output and coding-generated/).",
            file=sys.stderr,
        )
        sys.exit(1)

    print(f"Found {len(checkpoint_files)} checkpoint file(s).", file=sys.stderr)

    model_entries: list[dict] = []

    for path in checkpoint_files:
        data = load_checkpoint(path)
        if data is None:
            continue

        model = data.get("model")
        if not model:
            print(f"Warning: no 'model' field in {path}, skipping.", file=sys.stderr)
            continue

        layer1 = float(data.get("layer1_pass_rate", 0.0))
        layer2 = float(data.get("layer2_pass_rate", 0.0))
        layer3 = float(data.get("layer3_weighted_score", 0.0))

        composite = compute_composite(layer1, layer2, layer3)

        model_entries.append(
            {
                "model": model,
                "composite_score": round(composite, 6),
                "layer1_pass_rate": layer1,
                "layer2_pass_rate": layer2,
                "layer3_weighted_score": layer3,
                "_checkpoint_path": path,
            }
        )

    if not model_entries:
        print("No valid model entries found. Exiting.", file=sys.stderr)
        sys.exit(1)

    # Sort descending by composite score, then alphabetically by model name for stability
    model_entries.sort(key=lambda e: (-e["composite_score"], e["model"]))

    # Build ranked results list (strip internal _checkpoint_path)
    results: list[dict] = []
    for rank, entry in enumerate(model_entries, start=1):
        results.append(
            {
                "rank": rank,
                "model": entry["model"],
                "composite_score": entry["composite_score"],
                "layer1_pass_rate": entry["layer1_pass_rate"],
                "layer2_pass_rate": entry["layer2_pass_rate"],
                "layer3_weighted_score": entry["layer3_weighted_score"],
            }
        )

    # Update each per-model checkpoint with the computed composite_score
    for entry in model_entries:
        update_checkpoint(entry["_checkpoint_path"], entry["composite_score"])

    # Build aggregate output document
    run_finished_at = datetime.datetime.now(datetime.timezone.utc).isoformat()

    aggregate = {
        "benchmark": "coding",
        "scoring": {
            "layer1_weight": 0.15,
            "layer2_weight": 0.25,
            "layer3_weight": 0.60,
            "description": "EvalPlus HumanEval+ (Python) / MultiPL-E C# / Custom .NET 10 Suite",
        },
        "run_finished_at": run_finished_at,
        "results": results,
    }

    # Ensure output directory exists
    output_dir = os.path.dirname(output_path)
    if output_dir:
        os.makedirs(output_dir, exist_ok=True)

    with open(output_path, "w", encoding="utf-8") as fh:
        json.dump(aggregate, fh, indent=2)

    print(f"\nWrote {len(results)} model(s) to {output_path}", file=sys.stderr)
    print()
    print_leaderboard(results)


if __name__ == "__main__":
    main()
