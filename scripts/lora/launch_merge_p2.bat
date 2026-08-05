@echo off
REM Merge the p2 LoRA adapter into the base, output to merged-p2. Output
REM redirected to a file (no ssh pipe) and run via Task Scheduler so long
REM silent load/merge/save stretches can't idle-drop a held ssh connection.
cd /d C:\Development\vibethinker-train
set PYTHONUTF8=1
set HF_HOME=E:\.cache\huggingface
C:\Python311\python.exe -u merge_p2.py > merge_p2.log 2>&1
echo MERGE_BAT_EXIT %ERRORLEVEL% >> merge_p2.log
