# Sequential base-model comparison suite for qwen3.5:9b (gen on T5500, build on Framework).
# Runs the same four coding tests used for Qwythos so the base-vs-finetune is apples-to-apples.
$ErrorActionPreference = 'Continue'
$env:OLLAMA_HOST = 'http://t5500:11434'
$env:L2_RUN_TIMEOUT_S = '150'
$env:L3_BUILD_TIMEOUT_S = '150'
$env:L3_TEST_TIMEOUT_S = '150'
$M = 'qwen3.5:9b'
$DS = 'scripts/coding_tasks/datasets/data/humaneval-cs-reworded.json'
Set-Location 'c:\Development\OllamaBenchmarks'

Write-Output '===== [1/4] L2-chat ====='
$env:CODING_BENCH_THINK = ''
python -u scripts/benchmark_coding_layer2_chat.py --models $M --dataset-path $DS

Write-Output '===== [2/4] L2-raw ====='
python -u scripts/benchmark_coding_layer2.py --models $M --dataset-path $DS

Write-Output '===== [3/4] L3 no-think ====='
python -u scripts/benchmark_coding_layer3.py --models $M --output 'results/coding-layer3-qwen3.5_9b.json'

Write-Output '===== [4/4] L3 think ====='
$env:CODING_BENCH_THINK = 'true'
python -u scripts/benchmark_coding_layer3.py --models $M --output 'results/coding-layer3-qwen3.5_9b-think.json'

Write-Output '===== SUITE COMPLETE ====='
