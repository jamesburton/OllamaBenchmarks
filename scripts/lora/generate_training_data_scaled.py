#!/usr/bin/env python3
"""Generate ~1,000 C#/.NET training examples using a frontier model.

Uses Ollama (local or cloud model) to:
1. Generate varied task prompts across 8 .NET categories
2. Generate gold-standard C# completions for each prompt

The output is chat-format JSONL suitable for LoRA fine-tuning.

Usage:
    python scripts/lora/generate_training_data_scaled.py
    python scripts/lora/generate_training_data_scaled.py --model glm-5:cloud --target 1000
    python scripts/lora/generate_training_data_scaled.py --resume  # resume from checkpoint
"""

import argparse
import json
import os
import re
import sys
import time
import urllib.request
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
REPO_ROOT = SCRIPT_DIR.parent.parent
REFERENCES_DIR = REPO_ROOT / "scripts" / "coding_tasks" / "references"
OUTPUT_FILE = SCRIPT_DIR / "training_data_1k.jsonl"
CHECKPOINT_FILE = SCRIPT_DIR / "training_data_1k_checkpoint.jsonl"
PROMPTS_CACHE = SCRIPT_DIR / "generated_prompts.json"

OLLAMA_HOST = os.environ.get("OLLAMA_HOST", "http://127.0.0.1:11434")

SYSTEM_PROMPT = (
    "You are an expert C#/.NET developer. When asked to write code, "
    "return ONLY valid C# code in a single file. Do not include markdown "
    "fences, explanations, or commentary — just the raw C# source code."
)

# Reference files loaded once and injected into prompts
REFERENCE_MAP = {
    "aspnet": ["aspnet_net10.md", "oneof.md"],
    "efcore": ["efcore_10.md"],
    "blazor": ["blazor_net10.md"],
    "masstransit": ["masstransit_v8.md"],
    "xunit": ["xunit_v3.md", "awesome_assertions.md", "nsubstitute.md"],
    "linq": [],
    "async": [],
    "vertical": ["aspnet_net10.md", "efcore_10.md", "masstransit_v8.md"],
}

# Task variation templates — each category has seed patterns that the frontier
# model expands into diverse prompts.
CATEGORY_SEEDS = {
    "aspnet": [
        "Create an ASP.NET Core 10 controller for {domain} that uses OneOf<T> for result handling with GET and POST endpoints",
        "Create an ASP.NET Core 10 minimal API for {domain} with validation, dependency injection, and proper HTTP status codes",
        "Create an ASP.NET Core 10 controller with pagination, filtering, and sorting for a {domain} resource",
        "Create an ASP.NET Core 10 endpoint that accepts a {domain} command, validates it with FluentValidation, and returns appropriate responses",
        "Create an ASP.NET Core 10 API with middleware that handles {domain} authentication and authorization",
    ],
    "efcore": [
        "Create an EF Core 10 DbContext and entities for a {domain} system with relationships and JSON columns",
        "Create EF Core 10 queries for a {domain} system using left joins, includes, and projection",
        "Create an EF Core 10 repository for {domain} using ExecuteUpdate/ExecuteDelete for bulk operations",
        "Create EF Core 10 value converters and owned entities for a {domain} domain model",
        "Create EF Core 10 interceptors and query filters for a {domain} multi-tenant system",
    ],
    "blazor": [
        "Create a Blazor .NET 10 interactive component for a {domain} dashboard with real-time updates",
        "Create a Blazor .NET 10 form component for {domain} with validation, cascading dropdowns, and error handling",
        "Create a Blazor .NET 10 streaming render component that loads {domain} data progressively",
        "Create a Blazor .NET 10 component with cascading authentication state for {domain} role-based UI",
        "Create a Blazor .NET 10 component using RenderFragment and EventCallback for a reusable {domain} widget",
    ],
    "masstransit": [
        "Create a MassTransit v8 consumer for {domain} that publishes events and uses consumer definitions with retry",
        "Create a MassTransit v8 saga state machine for a {domain} workflow with multiple states and transitions",
        "Create a MassTransit v8 request/response pattern for {domain} with timeout handling",
        "Create a MassTransit v8 routing slip for a {domain} multi-step process with compensation",
        "Create MassTransit v8 message contracts and consumers for a {domain} event-driven architecture",
    ],
    "xunit": [
        "Create xUnit v3 tests for a {domain} service using NSubstitute mocks and AwesomeAssertions",
        "Create xUnit v3 theory tests with inline and member data for {domain} validation logic",
        "Create xUnit v3 integration tests for a {domain} API using WebApplicationFactory",
        "Create xUnit v3 tests with custom fixtures and collection fixtures for {domain} database testing",
        "Create xUnit v3 tests for {domain} using the builder pattern for test data and fluent assertions",
    ],
    "linq": [
        "Create complex LINQ queries for {domain} reporting with grouping, aggregation, and projection",
        "Create LINQ extension methods for {domain} that implement custom filtering and sorting",
        "Create LINQ queries for {domain} that use SelectMany, GroupJoin, and aggregate operations",
        "Create a LINQ-based query builder for {domain} with dynamic predicate composition",
        "Create LINQ queries for {domain} data transformation with anonymous types and tuples",
    ],
    "async": [
        "Create async C# code for {domain} with proper CancellationToken handling and timeout patterns",
        "Create a {domain} background service using IHostedService with graceful shutdown",
        "Create async pipeline processing for {domain} using Channel<T> and parallel consumers",
        "Create a {domain} retry policy using Polly with circuit breaker and fallback patterns",
        "Create async coordination for {domain} using SemaphoreSlim and async enumerable",
    ],
    "vertical": [
        "Create a complete vertical slice for {domain} with controller, service, repository, and EF Core entities",
        "Create a CQRS vertical slice for {domain} with MediatR command/query handlers and validation",
        "Create a vertical feature for {domain} with Blazor UI, API endpoint, and domain logic",
        "Create an event-sourced vertical slice for {domain} with event store and projection",
        "Create a vertical slice for {domain} with MassTransit consumer, service, and API endpoint",
    ],
}

# Domains to vary the prompts — each seed x domain = unique example
DOMAINS = [
    "e-commerce order management",
    "healthcare patient records",
    "financial trading platform",
    "inventory warehouse system",
    "HR employee onboarding",
    "IoT sensor monitoring",
    "content management system",
    "logistics shipment tracking",
    "real estate property listing",
    "education course enrollment",
    "restaurant reservation system",
    "subscription billing platform",
    "project management tool",
    "customer support ticketing",
    "social media feed aggregator",
    "fleet vehicle management",
    "insurance claims processing",
    "hotel booking platform",
    "gym membership management",
    "supply chain procurement",
    "event ticketing system",
    "pharmacy prescription management",
    "library book lending system",
    "airline flight booking",
    "recruitment job application tracking",
]


def load_references(category: str) -> str:
    """Load reference files for a category."""
    ref_files = REFERENCE_MAP.get(category, [])
    texts = []
    for ref_file in ref_files:
        ref_path = REFERENCES_DIR / ref_file
        if ref_path.exists():
            texts.append(ref_path.read_text(encoding="utf-8"))
    return "\n\n".join(texts)


def call_ollama(model: str, messages: list[dict], max_tokens: int = 4096,
                timeout: int = 600) -> str:
    """Call Ollama chat API. Returns response text or empty string on error."""
    payload = {
        "model": model,
        "messages": messages,
        "stream": False,
        "options": {
            "num_predict": max_tokens,
            "temperature": 0.7,
            "top_p": 0.9,
        },
    }
    data = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(
        f"{OLLAMA_HOST}/api/chat",
        data=data,
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=timeout) as resp:
            body = json.loads(resp.read().decode("utf-8"))
        content = body.get("message", {}).get("content", "")
        if "<think>" in content:
            content = re.sub(r"<think>.*?</think>", "", content, flags=re.S).strip()
        return content
    except Exception as exc:
        print(f"    [error] {type(exc).__name__}: {exc}")
        return ""


def generate_detailed_prompt(model: str, seed: str, domain: str,
                             references: str) -> str:
    """Use the frontier model to expand a seed into a detailed, specific prompt."""
    base_task = seed.format(domain=domain)
    ref_section = ""
    if references:
        # Truncate references but keep enough for context
        ref_text = references[:3000]
        ref_section = f"""
The following API reference documentation is available and MUST be included at the start of the prompt:

{ref_text}"""

    meta_prompt = f"""Write a detailed C# coding task prompt for this requirement:

{base_task}
{ref_section}

Your output must be a self-contained task prompt that:
1. Starts with "Given the following API reference:" if references were provided above, followed by the reference text
2. Lists specific class/record/interface names to create
3. Specifies property names and types explicitly
4. Includes implementation constraints (e.g. "use record types", "implement IConsumer<T>")
5. States "Return only valid C# code in a single file." at the end
6. Is at least 200 words long

Write the complete task prompt now, nothing else:"""

    result = call_ollama(model, [{"role": "user", "content": meta_prompt}],
                         max_tokens=2048, timeout=120)
    return result.strip()


def generate_completion(model: str, prompt: str) -> str:
    """Generate a gold-standard C# completion for a prompt."""
    messages = [
        {"role": "system", "content": SYSTEM_PROMPT},
        {"role": "user", "content": prompt},
    ]
    result = call_ollama(model, messages, max_tokens=4096, timeout=300)
    return strip_markdown_fences(result)


def strip_markdown_fences(code: str) -> str:
    """Remove ```csharp / ``` wrappers if present."""
    if not code:
        return ""
    lines = code.strip().splitlines()
    if lines and lines[0].strip().startswith("```"):
        lines = lines[1:]
    if lines and lines[-1].strip() == "```":
        lines = lines[:-1]
    return "\n".join(lines).strip()


def make_chat_example(prompt: str, code: str) -> dict:
    """Create a chat-format training example."""
    return {
        "messages": [
            {"role": "system", "content": SYSTEM_PROMPT},
            {"role": "user", "content": prompt.strip()},
            {"role": "assistant", "content": code.strip()},
        ]
    }


def load_checkpoint() -> list[dict]:
    """Load existing checkpoint if present."""
    if CHECKPOINT_FILE.exists():
        examples = []
        with open(CHECKPOINT_FILE, "r", encoding="utf-8") as fh:
            for line in fh:
                if line.strip():
                    examples.append(json.loads(line))
        return examples
    return []


def append_checkpoint(example: dict):
    """Append a single example to checkpoint file."""
    with open(CHECKPOINT_FILE, "a", encoding="utf-8") as fh:
        fh.write(json.dumps(example, ensure_ascii=False) + "\n")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--model", default="glm-5:cloud",
                        help="Frontier model for generation")
    parser.add_argument("--target", type=int, default=1000,
                        help="Target number of examples")
    parser.add_argument("--resume", action="store_true",
                        help="Resume from checkpoint")
    args = parser.parse_args()

    # Calculate how many examples per category
    categories = list(CATEGORY_SEEDS.keys())
    per_category = args.target // len(categories)  # ~125 each
    remainder = args.target % len(categories)

    print(f"=== Scaled Training Data Generator ===")
    print(f"Model: {args.model}")
    print(f"Target: {args.target} examples")
    print(f"Categories: {len(categories)} x ~{per_category} = {args.target}")
    print()

    # Load checkpoint
    existing = load_checkpoint() if args.resume else []
    if existing:
        print(f"Resuming from checkpoint: {len(existing)} examples already generated")
    elif CHECKPOINT_FILE.exists():
        CHECKPOINT_FILE.unlink()

    generated = len(existing)
    errors = 0
    start_time = time.time()

    for cat_idx, category in enumerate(categories):
        cat_target = per_category + (1 if cat_idx < remainder else 0)
        seeds = CATEGORY_SEEDS[category]
        references = load_references(category)

        # Calculate how many already done for this category
        cat_done = sum(1 for ex in existing
                       if ex.get("metadata", {}).get("category") == category)
        cat_remaining = cat_target - cat_done

        if cat_remaining <= 0:
            print(f"[{category}] Already complete ({cat_done}/{cat_target})")
            continue

        print(f"\n[{category}] Generating {cat_remaining} examples "
              f"(have {cat_done}/{cat_target})")

        example_idx = 0
        for domain in DOMAINS:
            if example_idx >= cat_remaining:
                break
            for seed in seeds:
                if example_idx >= cat_remaining:
                    break

                print(f"  [{generated+1}/{args.target}] {category}/{domain[:30]}...",
                      end="", flush=True)

                # Step 1: Generate detailed prompt
                prompt = generate_detailed_prompt(
                    args.model, seed, domain, references
                )
                if not prompt or len(prompt) < 100:
                    print(" skip (bad prompt)")
                    errors += 1
                    continue

                # Step 2: Generate gold-standard completion
                code = generate_completion(args.model, prompt)
                if not code or len(code) < 100:
                    print(" skip (bad completion)")
                    errors += 1
                    continue

                # Quality filter: must contain 'class' or 'record' or 'interface'
                if not re.search(r'\b(class|record|interface|enum)\b', code):
                    print(" skip (no C# types)")
                    errors += 1
                    continue

                example = make_chat_example(prompt, code)
                example["metadata"] = {
                    "category": category,
                    "domain": domain,
                    "seed_idx": seeds.index(seed),
                }
                append_checkpoint(example)

                generated += 1
                example_idx += 1
                elapsed = time.time() - start_time
                rate = generated / elapsed if elapsed > 0 else 0
                eta = (args.target - generated) / rate if rate > 0 else 0
                print(f" ok ({rate:.1f}/min, ETA {eta/60:.0f}m)")

    # Write final output (strip metadata)
    all_examples = load_checkpoint()
    with open(OUTPUT_FILE, "w", encoding="utf-8") as fh:
        for ex in all_examples:
            clean = {k: v for k, v in ex.items() if k != "metadata"}
            fh.write(json.dumps(clean, ensure_ascii=False) + "\n")

    elapsed = time.time() - start_time
    print(f"\n=== Complete ===")
    print(f"Generated: {len(all_examples)} examples")
    print(f"Errors/skips: {errors}")
    print(f"Time: {elapsed/60:.1f} minutes")
    print(f"Output: {OUTPUT_FILE}")

    # Category breakdown
    from collections import Counter
    cats = Counter(ex.get("metadata", {}).get("category", "?")
                   for ex in all_examples)
    print("\nBy category:")
    for cat, count in cats.most_common():
        print(f"  {cat}: {count}")


if __name__ == "__main__":
    main()
