import argparse
import datetime
import json
import os
import re
import subprocess
import time
from typing import Any
import urllib.request

from collect_host_info import build_host_info


def model_slug(model: str) -> str:
    model = re.sub(r":latest$", "", model)
    return re.sub(r"[^\w\.-]", "_", model.replace(":", "_").replace("/", "_").replace("\\", "_"))


def write_json(path: str, payload: dict[str, Any]) -> None:
    os.makedirs(os.path.dirname(path) or ".", exist_ok=True)
    with open(path, "w", encoding="utf-8") as handle:
        handle.write(json.dumps(payload, indent=2) + "\n")


def post_json(base_url: str, path: str, payload: dict[str, Any], api_key: str | None = None, timeout: int = 1200) -> dict[str, Any]:
    req = urllib.request.Request(
        f"{base_url.rstrip('/')}{path}",
        data=json.dumps(payload).encode('utf-8'),
        headers={'Content-Type': 'application/json'},
        method='POST',
    )
    if api_key:
        req.add_header('Authorization', f'Bearer {api_key}')
    with urllib.request.urlopen(req, timeout=timeout) as response:
        return json.loads(response.read().decode('utf-8'))


def get_server_process() -> subprocess.Popen | None:
    return None


def get_process_metrics(process_name: str) -> dict[str, float]:
    script = f"Get-Process -Name '{process_name}' -ErrorAction SilentlyContinue | Select-Object -Property CPU,WorkingSet64,Id | ConvertTo-Json"
    completed = subprocess.run(['powershell', '-NoProfile', '-Command', script], capture_output=True, text=True, timeout=20)
    if completed.returncode != 0 or not completed.stdout.strip():
        return {'cpu': 0.0, 'ram': 0.0}
    data = json.loads(completed.stdout)
    if isinstance(data, list):
        cpu = sum(float(item.get('CPU') or 0) for item in data)
        ram = sum(float(item.get('WorkingSet64') or 0) for item in data)
    else:
        cpu = float(data.get('CPU') or 0)
        ram = float(data.get('WorkingSet64') or 0)
    return {'cpu': cpu, 'ram': ram}


def get_gpu_stats() -> dict[str, float]:
    script = r"""
$gpuCtr = @((Get-Counter '\GPU Engine(*)\Utilization Percentage' -ErrorAction SilentlyContinue).CounterSamples | Where-Object { $_.Status -eq 0 })
$gpuMemCtr = @((Get-Counter '\GPU Process Memory(*)\Dedicated Usage' -ErrorAction SilentlyContinue).CounterSamples | Where-Object { $_.Status -eq 0 })
$result = [pscustomobject]@{
  gpu_util = [double](($gpuCtr | Measure-Object CookedValue -Sum).Sum)
  gpu_mem = [double](($gpuMemCtr | Measure-Object CookedValue -Sum).Sum)
}
$result | ConvertTo-Json
"""
    completed = subprocess.run(['powershell', '-NoProfile', '-Command', script], capture_output=True, text=True, timeout=30)
    if completed.returncode != 0 or not completed.stdout.strip():
        return {'gpu_util': 0.0, 'gpu_mem': 0.0}
    data = json.loads(completed.stdout)
    return {'gpu_util': float(data.get('gpu_util') or 0.0), 'gpu_mem': float(data.get('gpu_mem') or 0.0)}


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument('--model', required=True)
    parser.add_argument('--base-url', required=True)
    parser.add_argument('--api-key')
    parser.add_argument('--prompt', default='Write a concise explanation of dependency injection with one short Python example.')
    parser.add_argument('--num-predict', type=int, default=192)
    parser.add_argument('--output')
    parser.add_argument('--process-name', default='llama-server')
    args = parser.parse_args()

    run_started_at = datetime.datetime.now(datetime.timezone.utc)
    if not args.output:
        args.output = os.path.join('.', 'results', f'throughput-openai-{run_started_at.strftime("%Y%m%d-%H%M%S")}.json')
    host_details = build_host_info()

    post_json(args.base_url, '/v1/chat/completions', {
        'model': args.model,
        'messages': [{'role': 'user', 'content': 'Warmup: one short sentence.'}],
        'temperature': 0,
        'max_tokens': 16,
    }, api_key=args.api_key)

    payload = {
        'model': args.model,
        'messages': [{'role': 'user', 'content': args.prompt}],
        'temperature': 0,
        'max_tokens': args.num_predict,
    }

    cpu_samples: list[float] = []
    ram_samples: list[float] = []
    gpu_util_samples: list[float] = []
    gpu_mem_samples: list[float] = []
    prev_cpu = None
    prev_time = None

    start = time.perf_counter()
    proc = subprocess.Popen(['python', '-c', (
        'import json, urllib.request; '
        f'payload = {json.dumps(payload)}; '
        'req = urllib.request.Request(' + repr(args.base_url.rstrip('/') + '/v1/chat/completions') + ', data=json.dumps(payload).encode("utf-8"), headers={"Content-Type":"application/json"}, method="POST"); '
        'print(urllib.request.urlopen(req, timeout=1800).read().decode("utf-8"))'
    )], stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)

    logical = os.cpu_count() or 1
    while proc.poll() is None:
        now = time.perf_counter()
        metrics = get_process_metrics(args.process_name)
        gpu = get_gpu_stats()
        ram_samples.append(metrics['ram'])
        gpu_util_samples.append(gpu['gpu_util'])
        gpu_mem_samples.append(gpu['gpu_mem'])
        if prev_cpu is not None and prev_time is not None:
            dt = now - prev_time
            if dt > 0:
                cpu_pct = (((metrics['cpu'] - prev_cpu) / dt) * 100.0) / logical
                if cpu_pct < 0:
                    cpu_pct = 0.0
                cpu_samples.append(cpu_pct)
        prev_cpu = metrics['cpu']
        prev_time = now
        time.sleep(1)

    stdout, stderr = proc.communicate(timeout=30)
    if proc.returncode != 0:
        raise RuntimeError(stderr.strip() or 'chat completion failed')
    end = time.perf_counter()
    response = json.loads(stdout)
    choice = response.get('choices', [{}])[0].get('message', {})
    usage = response.get('usage', {})
    completion_tokens = int(usage.get('completion_tokens') or 0)
    total_s = end - start
    tokps = (completion_tokens / total_s) if total_s > 0 and completion_tokens > 0 else 0.0

    result = {
        'benchmark': 'throughput_openai',
        'run_started_at': run_started_at.isoformat(),
        'run_finished_at': datetime.datetime.now(datetime.timezone.utc).isoformat(),
        'output_path': os.path.abspath(args.output),
        'base_url': args.base_url,
        'host_details': host_details,
        'models': [args.model],
        'results': [{
            'model': args.model,
            'completion_tokens': completion_tokens,
            'toks_per_s': round(tokps, 2),
            'total_s': round(total_s, 3),
            'cpu_avg_pct': round(sum(cpu_samples)/len(cpu_samples), 1) if cpu_samples else 0,
            'cpu_peak_pct': round(max(cpu_samples), 1) if cpu_samples else 0,
            'ram_peak_gb': round(max(ram_samples)/ (1024**3), 2) if ram_samples else 0,
            'gpu_util_avg': round(sum(gpu_util_samples)/len(gpu_util_samples), 1) if gpu_util_samples else 0,
            'gpu_util_peak': round(max(gpu_util_samples), 1) if gpu_util_samples else 0,
            'gpu_mem_peak_gb': round(max(gpu_mem_samples) / (1024**3), 2) if gpu_mem_samples else 0,
            'response_preview': (choice.get('content') or '')[:200],
        }],
    }
    write_json(args.output, result)
    per_model_path = os.path.join(os.path.dirname(args.output) or '.', f'throughput-resource-{model_slug(args.model)}.json')
    write_json(per_model_path, {
        'benchmark': 'throughput_openai',
        'run_started_at': run_started_at.isoformat(),
        'run_finished_at': datetime.datetime.now(datetime.timezone.utc).isoformat(),
        'output_path': os.path.abspath(per_model_path),
        'base_url': args.base_url,
        'host_details': host_details,
        'models': [args.model],
        'results': result['results'],
    })
    print(json.dumps(result, indent=2))


if __name__ == '__main__':
    main()
