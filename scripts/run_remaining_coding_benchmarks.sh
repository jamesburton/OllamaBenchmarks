#!/bin/bash
# Pulls remaining 5/5 models and benchmarks each on Layer 3.
# Run from repo root: bash scripts/run_remaining_coding_benchmarks.sh

set -e
cd "$(dirname "$0")/.."

MODELS=(
  "qwen3:14b"
  "qwen3-coder-next"
  "nemotron-cascade-2"
  "glm-4.7-flash"
  "granite4:32b-a9b-h"
  "nemotron-3-nano:30b-a3b-q8_0"
  "qwen3.5:35b-a3b"
  "qwen3-coder:30b"
  "nemotron-3-super"
  "qwen3-coder-next:q8_0"
  "devstral-small-2:24b-instruct-2512-q8_0"
  "gpt-oss:120b"
  "qwen3.5:122b"
  "qwen3.5:122b-a10b"
  "qwen3.5:35b-a3b-q2_k_l"
)

for model in "${MODELS[@]}"; do
  slug=$(echo "$model" | sed 's/:latest$//' | sed 's/[:/\\]/_/g' | sed 's/[^a-zA-Z0-9._-]/_/g')
  result="results/coding-${slug}.json"

  if [ -f "$result" ]; then
    echo "=== SKIP $model (already benchmarked: $result) ==="
    continue
  fi

  echo "=== PULLING $model ==="
  ollama pull "$model" || { echo "PULL FAILED for $model, skipping"; continue; }

  echo "=== BENCHMARKING $model ==="
  python scripts/benchmark_coding_layer3.py \
    --models "$model" \
    --task-dir scripts/coding_tasks/tasks \
    --checkpoint-dir results || { echo "BENCHMARK FAILED for $model"; continue; }

  echo "=== DONE $model ==="
done

echo "=== Running composite scorer ==="
python scripts/benchmark_coding_composite.py --checkpoint-dir results

echo "=== All done. Committing results ==="
git add results/coding-*.json results/coding-generated/ results/coding-layer3-results.json
git commit -m "feat(coding-bench): Layer 3 coding benchmark — full suite results

Co-Authored-By: Claude Opus 4.6 (1M context) <noreply@anthropic.com>"
git push origin main
