import argparse
import datetime
import json
import os
import statistics
import urllib.request

from collect_host_info import build_host_info

DEFAULT_SWEEP = [
    ("baseline", {}),
    ("threads_8", {"num_thread": 8}),
    ("threads_16", {"num_thread": 16}),
    ("batch_1024", {"num_batch": 1024}),
    ("force_gpu_99", {"num_gpu": 99}),
]


def post_json(payload: dict, timeout: int = 1800) -> dict:
    request = urllib.request.Request(
        "http://127.0.0.1:11434/api/generate",
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(request, timeout=timeout) as response:
        return json.loads(response.read().decode("utf-8"))


def run_once(model: str, options: dict) -> dict:
    payload = {
        "model": model,
        "prompt": "Write a concise explanation of dependency injection with one short Python example.",
        "stream": False,
        "options": {
            "num_predict": 192,
            "temperature": 0,
            "top_p": 1,
            "seed": 42,
            **options,
        },
    }
    response = post_json(payload)
    eval_seconds = response["eval_duration"] / 1e9
    toks_per_s = response["eval_count"] / eval_seconds if eval_seconds > 0 else 0
    return {
        "tps": toks_per_s,
        "total_s": response["total_duration"] / 1e9,
        "eval_count": response["eval_count"],
    }


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--model", required=True)
    parser.add_argument("--runs", type=int, default=2)
    parser.add_argument("--output")
    args = parser.parse_args()
    run_started_at = datetime.datetime.now(datetime.timezone.utc)
    if not args.output:
        args.output = os.path.join(
            ".",
            "results",
            f"sweep-{run_started_at.strftime('%Y%m%d-%H%M%S')}.json",
        )

    post_json(
        {
            "model": args.model,
            "prompt": "Warmup short response.",
            "stream": False,
            "options": {"num_predict": 24, "temperature": 0, "seed": 42},
        }
    )

    results = []
    for name, options in DEFAULT_SWEEP:
        runs = [run_once(args.model, options) for _ in range(args.runs)]
        results.append(
            {
                "perm": name,
                "opts": options,
                "tps_avg": round(statistics.mean(item["tps"] for item in runs), 2),
                "tps_min": round(min(item["tps"] for item in runs), 2),
                "tps_max": round(max(item["tps"] for item in runs), 2),
                "eval_count_avg": round(statistics.mean(item["eval_count"] for item in runs), 1),
                "total_s_avg": round(statistics.mean(item["total_s"] for item in runs), 3),
            }
        )

    payload_obj = {
        "benchmark": "sweep",
        "run_started_at": run_started_at.isoformat(),
        "run_finished_at": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "output_path": os.path.abspath(args.output),
        "ollama_host": "http://127.0.0.1:11434",
        "host_details": build_host_info(),
        "model": args.model,
        "runs": args.runs,
        "variants": results,
    }
    payload = json.dumps(payload_obj, indent=2)
    os.makedirs(os.path.dirname(args.output) or ".", exist_ok=True)
    with open(args.output, "w", encoding="utf-8") as handle:
        handle.write(payload + "\n")
    print(payload)


if __name__ == "__main__":
    main()
