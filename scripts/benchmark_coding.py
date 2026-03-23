#!/usr/bin/env python3
"""Top-level orchestrator for the three-layer coding benchmark."""

import argparse
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(
        description="Run all coding benchmark layers and compute composite score."
    )
    parser.add_argument("--models", nargs="+", required=True, help="Models to benchmark")
    parser.add_argument(
        "--layers",
        default="1,2,3",
        help="Comma-separated list of layers to run (default: 1,2,3)",
    )
    parser.add_argument("--checkpoint-dir", default="results", help="Checkpoint directory")
    parser.add_argument(
        "--dataset-path",
        default="scripts/coding_tasks/datasets/data/humaneval-cs-reworded.json",
        help="Dataset path for Layer 2",
    )
    parser.add_argument(
        "--task-dir",
        default="scripts/coding_tasks/tasks",
        help="Task directory for Layer 3",
    )
    parser.add_argument(
        "--template-base",
        default="scripts/coding_tasks/templates",
        help="Template base directory",
    )
    parser.add_argument(
        "--references-dir",
        default="scripts/coding_tasks/references",
        help="References directory for Layer 3",
    )
    parser.add_argument(
        "--output",
        default="results/coding-current.json",
        help="Output file for composite results",
    )
    args = parser.parse_args()

    layers = {int(l.strip()) for l in args.layers.split(",") if l.strip()}

    layer_commands = {
        1: [
            sys.executable,
            "scripts/benchmark_coding_layer1.py",
            "--models",
        ]
        + args.models
        + ["--checkpoint-dir", args.checkpoint_dir],
        2: [
            sys.executable,
            "scripts/benchmark_coding_layer2.py",
            "--models",
        ]
        + args.models
        + [
            "--checkpoint-dir",
            args.checkpoint_dir,
            "--dataset-path",
            args.dataset_path,
            "--template-base",
            args.template_base,
        ],
        3: [
            sys.executable,
            "scripts/benchmark_coding_layer3.py",
            "--models",
        ]
        + args.models
        + [
            "--checkpoint-dir",
            args.checkpoint_dir,
            "--task-dir",
            args.task_dir,
            "--references-dir",
            args.references_dir,
            "--template-base",
            args.template_base,
        ],
    }

    for layer_num in sorted(layers):
        if layer_num not in layer_commands:
            print(f"WARNING: Unknown layer {layer_num}, skipping.")
            continue

        cmd = layer_commands[layer_num]
        print(f"=== Running Layer {layer_num} ===")
        try:
            subprocess.run(cmd, check=True)
            print(f"=== Layer {layer_num} complete ===")
        except subprocess.CalledProcessError as e:
            print(
                f"WARNING: Layer {layer_num} failed with exit code {e.returncode}. "
                "Continuing to next layer."
            )

    composite_cmd = [
        sys.executable,
        "scripts/benchmark_coding_composite.py",
        "--checkpoint-dir",
        args.checkpoint_dir,
        "--output",
        args.output,
    ]
    print("=== Running composite scorer ===")
    try:
        subprocess.run(composite_cmd, check=True)
    except subprocess.CalledProcessError as e:
        print(f"WARNING: Composite scorer failed with exit code {e.returncode}.")

    print(f"=== Coding benchmark complete. Results at {args.output} ===")


if __name__ == "__main__":
    main()
