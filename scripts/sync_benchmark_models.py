import json
import pathlib
import subprocess
from datetime import datetime, timezone


REPO_ROOT = pathlib.Path(__file__).resolve().parent.parent
INVENTORY_PATH = REPO_ROOT / "benchmark-models.json"


def load_inventory() -> dict:
    with INVENTORY_PATH.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def save_inventory(payload: dict) -> None:
    with INVENTORY_PATH.open("w", encoding="utf-8") as handle:
        json.dump(payload, handle, indent=2)
        handle.write("\n")


def list_local_models() -> list[str]:
    completed = subprocess.run(
        ["ollama", "list"],
        check=True,
        capture_output=True,
        text=True,
    )
    models = []
    for line in completed.stdout.splitlines():
        stripped = line.strip()
        if not stripped or stripped.startswith("NAME"):
            continue
        models.append(stripped.split()[0])
    return models


def build_effective_models(benchmark_suite: list[str], local_models: list[str]) -> list[str]:
    seen: set[str] = set()
    ordered: list[str] = []
    for model in [*benchmark_suite, *sorted(local_models)]:
        if model not in seen:
            seen.add(model)
            ordered.append(model)
    return ordered


def main() -> None:
    inventory = load_inventory()
    benchmark_suite = inventory.get("benchmark_suite") or []
    local_models = list_local_models()
    effective_models = build_effective_models(benchmark_suite, local_models)
    missing_from_local = [model for model in benchmark_suite if model not in local_models]

    updated = {
        "updated_at": datetime.now(timezone.utc).isoformat(),
        "benchmark_suite": benchmark_suite,
        "local_installed": local_models,
        "effective_models": effective_models,
        "missing_from_local": missing_from_local,
    }
    backend_notes = inventory.get("backend_notes")
    if backend_notes:
        updated["backend_notes"] = backend_notes
    save_inventory(updated)
    print(json.dumps(updated, indent=2))


if __name__ == "__main__":
    main()
