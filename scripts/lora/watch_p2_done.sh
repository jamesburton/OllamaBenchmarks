#!/usr/bin/env bash
# Completion watcher for the iteration-2 50k T5500 run. The final
# model.save_pretrained() writes adapter_model.safetensors to the output ROOT
# (checkpoints go to checkpoint-N subdirs). Break when the root adapter appears
# (DONE) or the trainer process dies before that (DIED — resumable from the
# latest checkpoint-N). Polls every 5 min via short ssh calls.
SSH="ssh t5500"
ROOT='C:\Development\vibethinker-train\scripts\lora\output\vibethinker-csharp-p2-50k-lora'
LOG='C:\Development\vibethinker-train\scripts\lora\train_p2_50k.log'
clean() { grep -v "post-quantum\|store now\|may need to be upgraded\|openssh.com\|This session"; }
alive_count() {
  $SSH "powershell -NoProfile -Command \"(Get-CimInstance Win32_Process -Filter \\\"name='python.exe'\\\" | Where-Object { \$_.CommandLine -match 'train_vibethinker' } | Measure-Object).Count\"" 2>&1 | clean | tr -d '\r '
}
i=0
while true; do
  i=$((i+1))
  done_file=$($SSH "if exist \"$ROOT\\adapter_model.safetensors\" echo YES" 2>&1 | clean | tr -d '\r ')
  last=$($SSH "powershell -NoProfile -Command \"Get-Content '$LOG' -Tail 1\"" 2>&1 | clean | tr -d '\r')
  latest_ckpt=$($SSH "dir /b /ad /o-d \"$ROOT\" 2>nul | findstr checkpoint" 2>&1 | clean | tr -d '\r' | head -1)
  echo "[poll $i] last='${last}' latest_ckpt='${latest_ckpt}'"
  if [ "$done_file" = "YES" ]; then
    echo "DONE: final adapter written to output root -> $ROOT"
    break
  fi
  alive=$(alive_count)
  if [ "$alive" = "0" ]; then
    sleep 30
    alive2=$(alive_count)
    if [ "$alive2" = "0" ]; then
      echo "DIED: trainer process gone before final save; resume from latest checkpoint: $latest_ckpt"
      break
    fi
  fi
  sleep 300
done
