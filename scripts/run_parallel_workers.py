"""Run a Layer 2/3 coding benchmark stage across N concurrent worker processes.

Exploits the OLLAMA_NUM_PARALLEL>1 batching win discovered in the perf-lever
sweep (2026-07-05): concurrent decode requests share the same weight-sweep
bandwidth cost, giving ~1.8x aggregate throughput at concurrency=2 even
though each individual stream's own tok/s doesn't improve. The underlying
benchmark scripts (benchmark_coding_layer2.py, benchmark_coding_layer2_chat.py,
benchmark_coding_layer3.py) are single-threaded and were not modified; this
wrapper splits the dataset/tasks round-robin across N worker subprocesses,
each writing to an isolated checkpoint dir, then merges the results.

Usage:
    python scripts/run_parallel_workers.py --stage l2chat --model <model> \
        --dataset-path scripts/coding_tasks/datasets/data/humaneval-cs-reworded.json
    python scripts/run_parallel_workers.py --stage l2raw --model <model> \
        --dataset-path scripts/coding_tasks/datasets/data/humaneval-cs-reworded.json
    python scripts/run_parallel_workers.py --stage l3think --model <model>
"""

import argparse
import datetime
import glob
import json
import os
import shutil
import subprocess
import sys

REPO_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(0, os.path.join(REPO_ROOT, "scripts"))
from coding_tasks.task_runner import model_slug, setup_template_cache  # noqa: E402


def split_round_robin(items, n):
    return [items[i::n] for i in range(n)]


def read_json(path):
    if not os.path.isfile(path):
        return {}
    with open(path, "r", encoding="utf-8") as fh:
        return json.load(fh)


def write_json(path, payload):
    os.makedirs(os.path.dirname(path) or ".", exist_ok=True)
    with open(path, "w", encoding="utf-8") as fh:
        fh.write(json.dumps(payload, indent=2) + "\n")


def base_env():
    # OLLAMA_HOST=0.0.0.0:11434 (the server-bind setting in this box's user env)
    # is not a fetchable client URL -- see PLATFORM_QUIRKS.md. Always override.
    env = os.environ.copy()
    if env.get("OLLAMA_HOST", "").startswith("0.0.0.0"):
        env["OLLAMA_HOST"] = "http://127.0.0.1:11434"
    # Running N concurrent workers means N concurrent dotnet build/test
    # processes competing for host RAM -- a "busy host" per CLAUDE.md's
    # build-timeout guidance. Bump defaults unless the caller already set
    # them, to avoid spurious "Process terminated" mis-scores.
    env.setdefault("L2_RUN_TIMEOUT_S", "150")
    env.setdefault("L3_BUILD_TIMEOUT_S", "150")
    env.setdefault("L3_TEST_TIMEOUT_S", "150")
    # Each task builds in a FRESH temp dir, so MSBuild's build-server/node-reuse
    # (meant to speed up repeated builds of the SAME project) just accumulates
    # orphaned dotnet.exe processes across hundreds of one-shot tasks instead of
    # ever being reused -- observed 40+ resident dotnet.exe after ~16 tasks
    # during cross-machine testing (2026-07-10), plausibly colliding with fresh
    # builds and causing sporadic "Process terminated" failures. Disable both
    # the MSBuild server and the older VBCSCompiler node-reuse mechanism so
    # every build fully exits, leaving nothing behind.
    env.setdefault("DOTNET_CLI_USE_MSBUILD_SERVER", "0")
    env.setdefault("MSBUILDDISABLENODEREUSE", "1")
    # Steady-state decode is ~1.0-1.2 tok/s (see MODEL_QUIRKS.md perf-lever
    # entry), not the 2.93 tok/s cold number -- give generation more room.
    env.setdefault("L2_GEN_TIMEOUT_S", "300")
    # Chat-mode's per-request timeout was hardcoded 600s; under concurrency=2
    # each stream's effective tok/s roughly halves, so multi-hundred-token
    # completions routinely exceeded it (observed 500-at-exactly-600s during
    # cross-machine testing). 1200s gives real headroom.
    env.setdefault("L2_CHAT_TIMEOUT_S", "1200")
    # L3's per-task generation call was hardcoded 600s; same slow-model +
    # concurrency reasoning as L2_CHAT_TIMEOUT_S above, worse under think:high
    # (extra reasoning tokens). Observed 100% TimeoutError at 600s cross-machine
    # (2026-07-13). Give it the same headroom as L2 chat.
    env.setdefault("L3_GEN_TIMEOUT_S", "1200")
    return env


def run_workers(cmds, envs, log_paths, on_tick=None, tick_interval=60):
    """Launch len(cmds) subprocesses concurrently, wait for all, return exit codes.

    If on_tick is given, it's called every tick_interval seconds while any
    worker is still running (and once more after they all exit) -- used to
    write a live merged-results snapshot so progress is observable and a
    crash/reboot mid-run only loses the current tick's window, not the whole
    run (worker checkpoints themselves are per-task; this just re-merges them).
    """
    import time

    procs = []
    handles = []
    for cmd, env, log_path in zip(cmds, envs, log_paths):
        os.makedirs(os.path.dirname(log_path), exist_ok=True)
        handle = open(log_path, "w", encoding="utf-8")
        print(f"[launch] {' '.join(cmd)} (log: {log_path})")
        proc = subprocess.Popen(cmd, cwd=REPO_ROOT, env=env, stdout=handle, stderr=subprocess.STDOUT)
        procs.append(proc)
        handles.append(handle)

    while True:
        all_done = all(proc.poll() is not None for proc in procs)
        if on_tick is not None:
            try:
                on_tick()
            except Exception as exc:
                print(f"[merge-tick] Error during periodic merge (non-fatal): {exc}")
        if all_done:
            break
        time.sleep(tick_interval)

    codes = [proc.wait() for proc in procs]
    for handle in handles:
        handle.close()
    return codes


def load_dataset_list(path):
    with open(path, "r", encoding="utf-8") as fh:
        content = fh.read().strip()
    if not content:
        return []
    # JSONL: one object per line
    if content[0] not in "[{":
        return []
    try:
        data = json.loads(content)
    except json.JSONDecodeError:
        return [json.loads(line) for line in content.splitlines() if line.strip()]
    if isinstance(data, list):
        return data
    if isinstance(data, dict):
        for value in data.values():
            if isinstance(value, list):
                return value
    return []


def run_l2_stage(args, chat: bool):
    script_name = "benchmark_coding_layer2_chat.py" if chat else "benchmark_coding_layer2.py"
    problems = load_dataset_list(args.dataset_path)
    if args.limit > 0:
        problems = problems[: args.limit]
    total = len(problems)
    print(f"[setup] {total} problem(s), splitting across {args.workers} worker(s)")

    # Pre-warm the shared template cache ONCE before spawning workers.
    # setup_template_cache() is check-then-act, not atomic -- if two workers
    # both find the cache missing and race to create it, the loser crashes
    # with FileExistsError (hit live during cross-machine testing, 2026-07-10).
    template_dir = os.path.join(args.template_base, "layer2_project")
    cache_dir = os.path.join(args.template_base, ".cache", "layer2_project")
    print(f"[setup] Pre-warming template cache: layer2_project -> {cache_dir}")
    setup_template_cache(template_dir, cache_dir)

    work_root = os.path.join(args.work_root, "l2chat" if chat else "l2raw")
    # NOTE: no rmtree here -- if a prior run left per-worker checkpoints in
    # this work_root, benchmark_coding_layer2[_chat].py resumes from them
    # (skips already-completed problem names). split_round_robin is a
    # deterministic function of (problems, workers), so re-splitting onto
    # the same worker{i} dirs lines shards up with the right resume state
    # as long as --dataset-path and --workers are unchanged across runs.
    shards = split_round_robin(problems, args.workers)

    cmds, envs, log_paths, worker_dirs = [], [], [], []
    for i, shard in enumerate(shards):
        worker_dir = os.path.join(work_root, f"worker{i}")
        os.makedirs(worker_dir, exist_ok=True)
        shard_path = os.path.join(worker_dir, "dataset.json")
        write_json(shard_path, shard)
        cmd = [
            sys.executable, os.path.join(REPO_ROOT, "scripts", script_name),
            "--models", args.model,
            "--dataset-path", shard_path,
            "--checkpoint-dir", worker_dir,
        ]
        env = base_env()
        cmds.append(cmd)
        envs.append(env)
        log_paths.append(os.path.join(worker_dir, "log.txt"))
        worker_dirs.append(worker_dir)

    slug = model_slug(args.model)
    suffix = "-chat" if chat else ""
    results_key = "layer2_chat_results" if chat else "layer2_results"
    passed_key = "layer2_chat_passed" if chat else "layer2_passed"
    total_key = "layer2_chat_total" if chat else "layer2_total"
    rate_key = "layer2_chat_pass_rate" if chat else "layer2_pass_rate"
    benchmark_name = "coding-layer2-chat" if chat else "coding"

    started = datetime.datetime.now(datetime.timezone.utc)

    def merge_and_write(finished_at=None):
        all_records = []
        for worker_dir in worker_dirs:
            worker_checkpoint = os.path.join(worker_dir, f"coding-{slug}{suffix}.json")
            data = read_json(worker_checkpoint)
            all_records.extend(data.get(results_key, []))

        merged_passed = sum(1 for r in all_records if r.get("passed"))
        merged_total = len(all_records)
        merged_rate = merged_passed / merged_total if merged_total else 0.0

        if chat:
            # Dedicated file, no other layer writes here -- plain overwrite is safe.
            out_path = os.path.join(args.checkpoint_dir, f"coding-{slug}{suffix}.json")
            payload = {
                "model": args.model,
                "benchmark": benchmark_name,
                "layer2_chat_run_started_at": started.isoformat(),
                "layer2_chat_run_finished_at": finished_at.isoformat() if finished_at else None,
                rate_key: merged_rate,
                passed_key: merged_passed,
                total_key: merged_total,
                results_key: all_records,
                "merged_from_workers": args.workers,
            }
            write_json(out_path, payload)
        else:
            # Shared coding-{slug}.json -- read-then-update to preserve layer3_*.
            out_path = os.path.join(args.checkpoint_dir, f"coding-{slug}.json")
            checkpoint = read_json(out_path)
            checkpoint.update({
                "model": args.model,
                "benchmark": benchmark_name,
                "layer2_run_started_at": started.isoformat(),
                "layer2_run_finished_at": finished_at.isoformat() if finished_at else None,
                rate_key: merged_rate,
                passed_key: merged_passed,
                total_key: merged_total,
                results_key: all_records,
                "layer2_merged_from_workers": args.workers,
            })
            write_json(out_path, checkpoint)
        print(f"[merge-tick] {rate_key}={merged_rate:.4f} ({merged_passed}/{merged_total}) -> {out_path}", flush=True)
        return out_path

    codes = run_workers(cmds, envs, log_paths, on_tick=merge_and_write, tick_interval=90)
    finished = datetime.datetime.now(datetime.timezone.utc)
    print(f"[done] Worker exit codes: {codes}")

    out_path = merge_and_write(finished_at=finished)
    with open(out_path, "r", encoding="utf-8") as fh:
        final_payload = json.load(fh)
    print(f"[score] {rate_key}={final_payload[rate_key]:.4f} ({final_payload[passed_key]}/{final_payload[total_key]})")
    print(f"[checkpoint] Merged result written to {out_path}")


def run_l3_stage(args):
    task_paths = sorted(
        p for p in glob.glob(os.path.join(args.task_dir, "*.yaml"))
        if not os.path.basename(p).startswith("_")
    )
    if args.limit > 0:
        task_paths = task_paths[: args.limit]
    total = len(task_paths)
    print(f"[setup] {total} task(s), splitting across {args.workers} worker(s)")

    # Pre-warm both template caches ONCE before spawning workers -- see the
    # matching comment in run_l2_stage for why this must not be left to each
    # worker's own (non-atomic) setup_template_cache() call.
    cache_base = os.path.join(args.template_base, ".cache")
    for template_name in ("test_project", "blazor_project"):
        template_dir = os.path.join(args.template_base, template_name)
        cache_dir = os.path.join(cache_base, template_name)
        print(f"[setup] Pre-warming template cache: {template_name} -> {cache_dir}")
        setup_template_cache(template_dir, cache_dir)

    work_root = os.path.join(args.work_root, "l3think" if args.think else "l3nothink")
    # NOTE: no rmtree here -- see the matching comment in run_l2_stage. Task
    # shards are deterministic given the same task_dir/workers, so re-copying
    # them into worker{i}/tasks is safe and benchmark_coding_layer3.py resumes
    # from any existing worker checkpoint (skips already-completed task names).
    shards = split_round_robin(task_paths, args.workers)

    cmds, envs, log_paths, worker_dirs = [], [], [], []
    for i, shard in enumerate(shards):
        worker_dir = os.path.join(work_root, f"worker{i}")
        worker_task_dir = os.path.join(worker_dir, "tasks")
        os.makedirs(worker_task_dir, exist_ok=True)
        for task_path in shard:
            shutil.copy(task_path, worker_task_dir)
        cmd = [
            sys.executable, os.path.join(REPO_ROOT, "scripts", "benchmark_coding_layer3.py"),
            "--models", args.model,
            "--task-dir", worker_task_dir,
            "--checkpoint-dir", worker_dir,
            "--output", os.path.join(worker_dir, "combined.json"),
        ]
        env = base_env()
        if args.think:
            env["CODING_BENCH_THINK"] = "high"
        cmds.append(cmd)
        envs.append(env)
        log_paths.append(os.path.join(worker_dir, "log.txt"))
        worker_dirs.append(worker_dir)

    slug = model_slug(args.model)
    suffix = "-think" if args.think else ""
    started = datetime.datetime.now(datetime.timezone.utc)

    def merge_and_write(finished_at=None):
        all_results = []
        for worker_dir in worker_dirs:
            worker_checkpoint = os.path.join(worker_dir, f"coding-{slug}{suffix}.json")
            data = read_json(worker_checkpoint)
            all_results.extend(data.get("layer3_results", []))

        numerator = sum(r.get("weight", 1) * (1 if r.get("passed") else 0) for r in all_results)
        denominator = sum(r.get("weight", 1) for r in all_results)
        merged_score = numerator / denominator if denominator else 0.0

        out_path = os.path.join(args.checkpoint_dir, f"coding-{slug}{suffix}.json")
        checkpoint = read_json(out_path)
        checkpoint.update({
            "model": args.model,
            "benchmark": "coding",
            "run_started_at": started.isoformat(),
            "run_finished_at": finished_at.isoformat() if finished_at else None,
            "layer3_results": all_results,
            "layer3_weighted_score": merged_score,
            "think_setting": "high" if args.think else "false",
            "layer3_merged_from_workers": args.workers,
        })
        write_json(out_path, checkpoint)
        passed = sum(1 for r in all_results if r.get("passed"))
        print(f"[merge-tick] layer3_weighted_score={merged_score:.4f} ({passed}/{len(all_results)} tasks passed) -> {out_path}", flush=True)
        return out_path

    codes = run_workers(cmds, envs, log_paths, on_tick=merge_and_write, tick_interval=90)
    finished = datetime.datetime.now(datetime.timezone.utc)
    print(f"[done] Worker exit codes: {codes}")

    out_path = merge_and_write(finished_at=finished)
    with open(out_path, "r", encoding="utf-8") as fh:
        final_payload = json.load(fh)
    print(f"[score] layer3_weighted_score={final_payload['layer3_weighted_score']:.4f} ({sum(1 for r in final_payload['layer3_results'] if r.get('passed'))}/{len(final_payload['layer3_results'])} tasks passed)")
    print(f"[checkpoint] Merged result written to {out_path}")


def main():
    parser = argparse.ArgumentParser(description="Run L2/L3 benchmarks split across N concurrent workers.")
    parser.add_argument("--stage", required=True, choices=["l2chat", "l2raw", "l3think", "l3nothink"])
    parser.add_argument("--model", required=True)
    parser.add_argument("--dataset-path", help="Required for l2chat/l2raw")
    parser.add_argument("--task-dir", default="scripts/coding_tasks/tasks", help="For l3think/l3nothink")
    parser.add_argument("--template-base", default="scripts/coding_tasks/templates")
    parser.add_argument("--checkpoint-dir", default="results")
    parser.add_argument("--work-root", default="results/perf-parallel")
    parser.add_argument("--workers", type=int, default=2)
    parser.add_argument("--limit", type=int, default=0, help="Limit total problems/tasks before splitting (0 = all); useful for a smoke test")
    args = parser.parse_args()
    args.think = args.stage == "l3think"

    if args.stage in ("l2chat", "l2raw"):
        if not args.dataset_path:
            parser.error("--dataset-path is required for l2chat/l2raw")
        run_l2_stage(args, chat=(args.stage == "l2chat"))
    else:
        run_l3_stage(args)


if __name__ == "__main__":
    main()
