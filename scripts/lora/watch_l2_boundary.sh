#!/usr/bin/env bash
# Wait until the suite reaches the L3 phase (===== [3/4]), then exit so the
# parent can stop the chain — pausing between L2 and L3 to free the GPU.
LOG="C:\Users\james\AppData\Local\Temp\claude\c--Development-OllamaBenchmarks\2194d865-a399-4414-8bd4-cceefd02038b\tasks\b3bqtk801.output"
i=0
while true; do
  i=$((i+1))
  cur=$(cat "$LOG" 2>/dev/null | tr '\r' '\n' | grep -oE "\[[0-9]+/158\]" | tail -1)
  if cat "$LOG" 2>/dev/null | tr '\r' '\n' | grep -q "\[3/4\] L3"; then
    echo "L3_PHASE_STARTED (was at $cur) — stop the chain now"
    break
  fi
  echo "[boundary poll $i] L2-raw at $cur (waiting for L3 phase)"
  sleep 45
done
