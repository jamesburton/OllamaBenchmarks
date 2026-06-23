#!/usr/bin/env bash
SSH="ssh t5500"
LOG='C:\Development\vibethinker-train\merge_p2.log'
clean() { grep -v "post-quantum\|store now\|may need to be upgraded\|openssh.com\|This session"; }
i=0
while true; do
  i=$((i+1))
  tail=$($SSH "powershell -NoProfile -Command \"if (Test-Path '$LOG') { Get-Content '$LOG' -Tail 2 } else { 'NO_LOG' }\"" 2>&1 | clean | tr -d '\r')
  echo "[merge poll $i] $tail"
  echo "$tail" | grep -q "MERGED_OK\|MERGE_BAT_EXIT" && { echo "MERGE_FINISHED"; break; }
  sleep 60
done
