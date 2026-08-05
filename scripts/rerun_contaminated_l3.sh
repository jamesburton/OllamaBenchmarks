#!/usr/bin/env bash
# Re-run Layer 3 results invalidated by the test_project NuGet-restore failure
# (private fnz-qhub feed 401 + evicted global-cache packages -> NETSDK1064 ->
# all test_project tasks build-failed, zeroing 44/50 of L3). Fixed by the
# per-template nuget.config bypass; this re-runs every affected model so its L3
# reflects model quality, not the broken build env.
#
# Affected models were identified by: test_project tasks built == 0 while the
# build environment was demonstrably working (e.g. Blazor tasks built, or the
# same model's other run scored normally). See the session handoff.
#
# Usage:
#   OLLAMA_HOST=http://t5500:11434 bash scripts/rerun_contaminated_l3.sh installed
#   OLLAMA_HOST=http://strix:11434 bash scripts/rerun_contaminated_l3.sh all
#
#   arg1 = "installed" (only models already pulled on the host) or "all"
#          (pull-run-delete the rest too; needs disk + network).
#
# Generation targets $OLLAMA_HOST; the dotnet build/test runs locally. Build
# timeouts are raised for loaded hosts. Each model writes coding-layer3-<slug>.json
# (+ -think.json) and merges layer3_* into coding-<slug>.json.
set -u
: "${OLLAMA_HOST:?set OLLAMA_HOST to the GPU host, e.g. http://t5500:11434}"
MODE="${1:-installed}"
export OLLAMA_HOST PYTHONUTF8=1 L3_BUILD_TIMEOUT_S=150 L3_TEST_TIMEOUT_S=150
SSH_HOST="${OLLAMA_HOST#http://}"; SSH_HOST="${SSH_HOST%%:*}"

slug() { python -c "import sys;sys.path.insert(0,'scripts');from coding_tasks.task_runner import model_slug;print(model_slug(sys.argv[1]))" "$1"; }

run_l3() {  # model  variants("nothink"|"think"|"both")
  local m="$1" v="${2:-nothink}" s; s="$(slug "$m")"
  if [ "$v" = nothink ] || [ "$v" = both ]; then
    echo "#### L3 no-think: $m ####"
    python scripts/benchmark_coding_layer3.py --models "$m" --output "results/coding-layer3-$s.json" 2>&1 | tail -3
  fi
  if [ "$v" = think ] || [ "$v" = both ]; then
    echo "#### L3 think: $m ####"
    CODING_BENCH_THINK=true python scripts/benchmark_coding_layer3.py --models "$m" --output "results/coding-layer3-$s-think.json" 2>&1 | tail -3
  fi
}

# model | variants-to-rerun | installed-on-T5500?
# (variants = only the contaminated ones; e.g. glm-4.7-flash's -think run was clean)
INSTALLED=(
  "qwen3.5:9b|both"
  "hf.co/empero-ai/Qwythos-9B-Claude-Mythos-5-1M-GGUF:Q4_K_M|both"
  "vibethinker-3b-cs:50k-q4|nothink"
)
NEEDS_PULL=(
  "gemma4:12b|both"
  "glm-4.7-flash|nothink"
  "qwen3:8b|nothink"
  "cogito:14b|nothink"
  "minicpm-v:8b|nothink"
  "hf.co/prithivMLmods/VibeThinker-3B-GGUF:Q4_K_M|nothink"
  "trinity-mini:Q4_K_M|nothink"
)
# vibethinker-csharp-p1-5k is a local-only fine-tune (not on a registry); re-run it
# wherever it is hosted (dev box / Strix) by adding it to the list there.

for entry in "${INSTALLED[@]}"; do run_l3 "${entry%%|*}" "${entry##*|}"; done

if [ "$MODE" = all ]; then
  for entry in "${NEEDS_PULL[@]}"; do
    m="${entry%%|*}"; v="${entry##*|}"
    free=$(ssh "$SSH_HOST" "powershell -NoProfile -Command \"[int]((Get-PSDrive E).Free/1GB)\"" 2>/dev/null | tr -dc '0-9')
    echo "#### pull $m (E: ${free:-?}GB free) ####"
    ssh "$SSH_HOST" "ollama pull \"$m\"" 2>&1 | tail -1
    run_l3 "$m" "$v"
    ssh "$SSH_HOST" "ollama rm \"$m\"" 2>&1 | tail -1   # free disk for the next pull
  done
fi
echo "#### CONTAMINATED-L3 RE-RUN DONE ($MODE) ####"
