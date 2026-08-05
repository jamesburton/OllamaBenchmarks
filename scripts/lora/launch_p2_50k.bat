@echo off
REM Robust T5500 launcher for the iteration-2 50k LoRA run.
REM Output is redirected to a file on T5500 (NOT an ssh pipe) so a dropped
REM ssh connection can never trigger a broken-pipe-on-flush death. Run via
REM Task Scheduler so the process lives outside the ssh session's job object.
cd /d C:\Development\vibethinker-train\scripts\lora
set PYTHONUTF8=1
set HF_HOME=E:\.cache\huggingface
C:\Python311\python.exe -u train_vibethinker_lora.py ^
  --training-data data_50k\stack_csharp_train.jsonl ^
  --output-dir output\vibethinker-csharp-p2-50k-lora ^
  --epochs 1 --lora-r 16 --lora-alpha 32 --lr 1e-4 ^
  --lora-dropout 0.1 --save-steps 50 ^
  > train_p2_50k.log 2>&1
