#!/usr/bin/env bash
# Watcher for the iteration-2 50k T5500 run. Polls every 5 min via short ssh
# calls (NOT a held connection — held connections drop on long silent runs).
# Breaks on EITHER the first checkpoint appearing (success gate: step-50) OR a
# death signal. Death = the trainer PROCESS is gone (no python running
# train_vibethinker_lora.py). GPU-idle is NOT a death signal: the packing /
# tokenizing phase legitimately sits at GPU 0% for minutes with the process
# very much alive (that false-fired the first watcher).
SSH="ssh t5500"
DIR='C:\Development\vibethinker-train\scripts\lora\output\vibethinker-csharp-p2-50k-lora'
LOG='C:\Development\vibethinker-train\scripts\lora\train_p2_50k.log'
clean() { grep -v "post-quantum\|store now\|may need to be upgraded\|openssh.com\|This session"; }
alive_count() {
  $SSH "powershell -NoProfile -Command \"(Get-CimInstance Win32_Process -Filter \\\"name='python.exe'\\\" | Where-Object { \$_.CommandLine -match 'train_vibethinker' } | Measure-Object).Count\"" 2>&1 | clean | tr -d '\r '
}
i=0
while true; do
  i=$((i+1))
  ckpt=$($SSH "dir /b \"$DIR\" 2>nul | findstr checkpoint" 2>&1 | clean | tr -d '\r')
  util=$($SSH "nvidia-smi --query-gpu=utilization.gpu --format=csv,noheader,nounits" 2>&1 | clean | tr -d '\r ')
  last=$($SSH "powershell -NoProfile -Command \"Get-Content '$LOG' -Tail 1\"" 2>&1 | clean | tr -d '\r')
  echo "[poll $i] util=${util}% ckpt='${ckpt}' last='${last}'"
  if [ -n "$ckpt" ]; then
    echo "GATE_PASSED: checkpoint written -> $ckpt"
    break
  fi
  alive=$(alive_count)
  if [ "$alive" = "0" ]; then
    sleep 30
    alive2=$(alive_count)   # one confirmation to avoid a transient wmi/cim hiccup
    if [ "$alive2" = "0" ]; then
      echo "DIED: trainer process gone (no python running train_vibethinker); no checkpoint yet"
      break
    fi
  fi
  sleep 300
done
