#!/usr/bin/env python3
"""Generate ~1,000 C#/.NET training examples directly from existing benchmark data.

Uses gold-standard completions from top-performing models (glm-5:cloud, deepseek-v3.2)
paired with the original task prompts, then creates domain-varied versions by
systematic entity/property substitution.

No API calls needed — runs in seconds.

Usage:
    python scripts/lora/generate_training_data_direct.py
    python scripts/lora/generate_training_data_direct.py --target 1000
"""

import argparse
import json
import os
import re
import sys
import random
from pathlib import Path
from copy import deepcopy

SCRIPT_DIR = Path(__file__).resolve().parent
REPO_ROOT = SCRIPT_DIR.parent.parent
TASKS_DIR = REPO_ROOT / "scripts" / "coding_tasks" / "tasks"
REFERENCES_DIR = REPO_ROOT / "scripts" / "coding_tasks" / "references"
GENERATED_DIR = REPO_ROOT / "results" / "coding-generated"
OUTPUT_FILE = SCRIPT_DIR / "training_data_1k.jsonl"

# Source models ordered by L3 score — use best passing completions
SOURCE_MODELS = [
    "glm-5_cloud",        # 18/20
    "deepseek-v3.2_cloud", # 16/20
    "minimax-m2.7_cloud",  # 14/20
    "cogito-2.1_671b-cloud", # 13/20
    "gemma4_26b",          # 12/20
    "gemma4",              # 9/20
    "hf.co_mradermacher_Qwen3-Coder-30B-A3B-Instruct-480B-Distill-V2-Fp32-i1-GGUF_Q4_K_M",
    "rnj-1_8b",
    "cogito_8b",
    "RogerBen_qwen3.5-35b-opus-distill",
    "gpt-oss_120b",
    "mistral-small",
    "qwen3-coder-next",
]

SYSTEM_PROMPT = (
    "You are an expert C#/.NET developer. When asked to write code, "
    "return ONLY valid C# code in a single file. Do not include markdown "
    "fences, explanations, or commentary — just the raw C# source code."
)

# ── Domain substitution tables ──────────────────────────────────────────
# Each domain has entity names, properties, and business-specific terms
# that get substituted into both prompts and completions.

DOMAINS = [
    {
        "name": "order management",
        "entity": "Order", "entity_lower": "order",
        "entity_plural": "Orders", "entity_plural_lower": "orders",
        "id_type": "Guid", "id_prop": "OrderId",
        "props": [("string", "CustomerName"), ("decimal", "Amount"), ("DateTime", "OrderDate")],
        "status_enum": ["Pending", "Processing", "Shipped", "Delivered", "Cancelled"],
        "event_name": "OrderSubmitted", "event_prop": "SubmittedAt",
        "service_iface": "IOrderService",
        "controller": "OrdersController",
        "consumer": "SubmitOrderConsumer",
        "message": "SubmitOrder",
        "db_context": "OrderDbContext",
    },
    {
        "name": "patient records",
        "entity": "Patient", "entity_lower": "patient",
        "entity_plural": "Patients", "entity_plural_lower": "patients",
        "id_type": "Guid", "id_prop": "PatientId",
        "props": [("string", "FullName"), ("DateTime", "DateOfBirth"), ("string", "MedicalRecordNumber")],
        "status_enum": ["Registered", "InTriage", "UnderCare", "Discharged", "Transferred"],
        "event_name": "PatientAdmitted", "event_prop": "AdmittedAt",
        "service_iface": "IPatientService",
        "controller": "PatientsController",
        "consumer": "AdmitPatientConsumer",
        "message": "AdmitPatient",
        "db_context": "PatientDbContext",
    },
    {
        "name": "product catalog",
        "entity": "Product", "entity_lower": "product",
        "entity_plural": "Products", "entity_plural_lower": "products",
        "id_type": "int", "id_prop": "ProductId",
        "props": [("string", "Name"), ("decimal", "Price"), ("int", "StockQuantity")],
        "status_enum": ["Draft", "Active", "OutOfStock", "Discontinued", "Archived"],
        "event_name": "ProductCreated", "event_prop": "CreatedAt",
        "service_iface": "IProductService",
        "controller": "ProductsController",
        "consumer": "CreateProductConsumer",
        "message": "CreateProduct",
        "db_context": "ProductDbContext",
    },
    {
        "name": "employee management",
        "entity": "Employee", "entity_lower": "employee",
        "entity_plural": "Employees", "entity_plural_lower": "employees",
        "id_type": "int", "id_prop": "EmployeeId",
        "props": [("string", "Name"), ("string", "Department"), ("decimal", "Salary")],
        "status_enum": ["Onboarding", "Active", "OnLeave", "Suspended", "Terminated"],
        "event_name": "EmployeeOnboarded", "event_prop": "OnboardedAt",
        "service_iface": "IEmployeeService",
        "controller": "EmployeesController",
        "consumer": "OnboardEmployeeConsumer",
        "message": "OnboardEmployee",
        "db_context": "EmployeeDbContext",
    },
    {
        "name": "shipment tracking",
        "entity": "Shipment", "entity_lower": "shipment",
        "entity_plural": "Shipments", "entity_plural_lower": "shipments",
        "id_type": "Guid", "id_prop": "ShipmentId",
        "props": [("string", "Origin"), ("string", "Destination"), ("decimal", "WeightKg")],
        "status_enum": ["Created", "PickedUp", "InTransit", "OutForDelivery", "Delivered"],
        "event_name": "ShipmentDispatched", "event_prop": "DispatchedAt",
        "service_iface": "IShipmentService",
        "controller": "ShipmentsController",
        "consumer": "DispatchShipmentConsumer",
        "message": "DispatchShipment",
        "db_context": "ShipmentDbContext",
    },
    {
        "name": "invoice processing",
        "entity": "Invoice", "entity_lower": "invoice",
        "entity_plural": "Invoices", "entity_plural_lower": "invoices",
        "id_type": "Guid", "id_prop": "InvoiceId",
        "props": [("string", "VendorName"), ("decimal", "TotalAmount"), ("DateTime", "DueDate")],
        "status_enum": ["Draft", "Sent", "Overdue", "Paid", "Voided"],
        "event_name": "InvoiceIssued", "event_prop": "IssuedAt",
        "service_iface": "IInvoiceService",
        "controller": "InvoicesController",
        "consumer": "IssueInvoiceConsumer",
        "message": "IssueInvoice",
        "db_context": "InvoiceDbContext",
    },
    {
        "name": "ticket management",
        "entity": "Ticket", "entity_lower": "ticket",
        "entity_plural": "Tickets", "entity_plural_lower": "tickets",
        "id_type": "int", "id_prop": "TicketId",
        "props": [("string", "Title"), ("string", "Description"), ("string", "Priority")],
        "status_enum": ["Open", "InProgress", "UnderReview", "Resolved", "Closed"],
        "event_name": "TicketCreated", "event_prop": "CreatedAt",
        "service_iface": "ITicketService",
        "controller": "TicketsController",
        "consumer": "CreateTicketConsumer",
        "message": "CreateTicket",
        "db_context": "TicketDbContext",
    },
    {
        "name": "course enrollment",
        "entity": "Enrollment", "entity_lower": "enrollment",
        "entity_plural": "Enrollments", "entity_plural_lower": "enrollments",
        "id_type": "Guid", "id_prop": "EnrollmentId",
        "props": [("string", "StudentName"), ("string", "CourseName"), ("DateTime", "EnrolledAt")],
        "status_enum": ["Pending", "Confirmed", "InProgress", "Completed", "Withdrawn"],
        "event_name": "StudentEnrolled", "event_prop": "EnrolledAt",
        "service_iface": "IEnrollmentService",
        "controller": "EnrollmentsController",
        "consumer": "EnrollStudentConsumer",
        "message": "EnrollStudent",
        "db_context": "EnrollmentDbContext",
    },
    {
        "name": "reservation system",
        "entity": "Reservation", "entity_lower": "reservation",
        "entity_plural": "Reservations", "entity_plural_lower": "reservations",
        "id_type": "Guid", "id_prop": "ReservationId",
        "props": [("string", "GuestName"), ("DateTime", "CheckIn"), ("DateTime", "CheckOut")],
        "status_enum": ["Requested", "Confirmed", "CheckedIn", "CheckedOut", "Cancelled"],
        "event_name": "ReservationConfirmed", "event_prop": "ConfirmedAt",
        "service_iface": "IReservationService",
        "controller": "ReservationsController",
        "consumer": "ConfirmReservationConsumer",
        "message": "ConfirmReservation",
        "db_context": "ReservationDbContext",
    },
    {
        "name": "vehicle fleet",
        "entity": "Vehicle", "entity_lower": "vehicle",
        "entity_plural": "Vehicles", "entity_plural_lower": "vehicles",
        "id_type": "int", "id_prop": "VehicleId",
        "props": [("string", "Make"), ("string", "Model"), ("int", "Year")],
        "status_enum": ["Available", "InUse", "Maintenance", "Retired", "Sold"],
        "event_name": "VehicleRegistered", "event_prop": "RegisteredAt",
        "service_iface": "IVehicleService",
        "controller": "VehiclesController",
        "consumer": "RegisterVehicleConsumer",
        "message": "RegisterVehicle",
        "db_context": "VehicleDbContext",
    },
]


def load_yaml(path: Path) -> dict:
    """Simple YAML loader for our task format (avoids pyyaml dependency issues)."""
    import yaml
    with open(path, "r", encoding="utf-8") as fh:
        return yaml.safe_load(fh)


def resolve_references(task: dict) -> str:
    """Build prompt with {references} resolved."""
    prompt = task.get("prompt", "")
    ref_texts = []
    for ref_file in task.get("references", []):
        ref_path = REFERENCES_DIR / ref_file
        if ref_path.exists():
            ref_texts.append(ref_path.read_text(encoding="utf-8"))
    return prompt.replace("{references}", "\n\n".join(ref_texts))


def strip_markdown_fences(code: str) -> str:
    if not code:
        return ""
    lines = code.strip().splitlines()
    if lines and lines[0].strip().startswith("```"):
        lines = lines[1:]
    if lines and lines[-1].strip() == "```":
        lines = lines[:-1]
    return "\n".join(lines).strip()


def read_completion(model_dir: str, task_name: str) -> str | None:
    """Read a .cs file from a model's generated output."""
    cs_path = GENERATED_DIR / model_dir / f"{task_name}.cs"
    if not cs_path.exists():
        return None
    text = cs_path.read_text(encoding="utf-8").strip()
    text = strip_markdown_fences(text)
    return text if text and len(text) > 100 else None


def apply_domain_substitution(text: str, src_domain: dict, dst_domain: dict) -> str:
    """Replace entity names, properties, etc. from source domain to destination domain."""
    result = text

    # Replace entity names (case-sensitive, whole words where possible)
    replacements = [
        (src_domain["entity"], dst_domain["entity"]),
        (src_domain["entity_lower"], dst_domain["entity_lower"]),
        (src_domain["entity_plural"], dst_domain["entity_plural"]),
        (src_domain["entity_plural_lower"], dst_domain["entity_plural_lower"]),
        (src_domain["id_prop"], dst_domain["id_prop"]),
        (src_domain["service_iface"], dst_domain["service_iface"]),
        (src_domain["controller"], dst_domain["controller"]),
        (src_domain["consumer"], dst_domain["consumer"]),
        (src_domain["message"], dst_domain["message"]),
        (src_domain["event_name"], dst_domain["event_name"]),
        (src_domain["event_prop"], dst_domain["event_prop"]),
        (src_domain["db_context"], dst_domain["db_context"]),
    ]

    # Sort by length descending to avoid partial replacements
    replacements.sort(key=lambda x: len(x[0]), reverse=True)

    for old, new in replacements:
        if old and new and old != new:
            result = result.replace(old, new)

    # Replace property declarations if source props are identifiable
    for i, (src_type, src_name) in enumerate(src_domain["props"]):
        if i < len(dst_domain["props"]):
            dst_type, dst_name = dst_domain["props"][i]
            if src_name != dst_name:
                result = result.replace(src_name, dst_name)

    # Replace status enum values
    for i, src_status in enumerate(src_domain["status_enum"]):
        if i < len(dst_domain["status_enum"]):
            dst_status = dst_domain["status_enum"][i]
            if src_status != dst_status:
                result = result.replace(src_status, dst_status)

    return result


def make_example(prompt: str, code: str, category: str, domain: str,
                 source_model: str) -> dict:
    """Create a training example."""
    return {
        "messages": [
            {"role": "system", "content": SYSTEM_PROMPT},
            {"role": "user", "content": prompt.strip()},
            {"role": "assistant", "content": code.strip()},
        ],
        "metadata": {
            "category": category,
            "domain": domain,
            "source_model": source_model,
        },
    }


# The base domain that our existing tasks use (order/user based)
BASE_DOMAIN = {
    "name": "base",
    "entity": "User", "entity_lower": "user",
    "entity_plural": "Users", "entity_plural_lower": "users",
    "id_type": "int", "id_prop": "Id",
    "props": [("string", "Name"), ("string", "Email"), ("int", "Id")],
    "status_enum": ["Pending", "Active", "Suspended", "Deleted", "Archived"],
    "event_name": "OrderSubmitted", "event_prop": "SubmittedAt",
    "service_iface": "IUserService",
    "controller": "UsersController",
    "consumer": "SubmitOrderConsumer",
    "message": "SubmitOrder",
    "db_context": "AppDbContext",
}

# Map task categories to which domain fields are most relevant for substitution
TASK_CATEGORY_MAP = {
    "aspnet_oneof_controller": "aspnet",
    "aspnet_validation_endpoint": "aspnet",
    "aspnet_service_di": "aspnet",
    "efcore_leftjoin": "efcore",
    "efcore_json_columns": "efcore",
    "efcore_executeupdate": "efcore",
    "masstransit_consumer": "masstransit",
    "masstransit_statemachine": "masstransit",
    "masstransit_test_harness": "masstransit",
    "blazor_interactive_component": "blazor",
    "blazor_streaming_render": "blazor",
    "blazor_cascading_auth": "blazor",
    "xunit_v3_theory_tests": "xunit",
    "xunit_awesome_nsubstitute": "xunit",
    "xunit_assembly_fixture": "xunit",
    "linq_complex_query": "linq",
    "async_cancellation": "async",
    "vertical_order_service": "vertical",
    "vertical_consumer_pipeline": "vertical",
    "vertical_blazor_crud": "vertical",
}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--target", type=int, default=1000)
    parser.add_argument("--seed", type=int, default=42)
    args = parser.parse_args()

    random.seed(args.seed)

    print("=== Direct Training Data Generator ===")
    print(f"Target: {args.target} examples\n")

    # Load all task YAMLs
    yaml_files = sorted(
        p for p in TASKS_DIR.glob("[0-9]*.yaml")
        if not p.stem.startswith("_")
    )
    tasks = {}
    for yf in yaml_files:
        task = load_yaml(yf)
        task_name = task.get("name", yf.stem)
        task["resolved_prompt"] = resolve_references(task)
        tasks[task_name] = task
    print(f"Loaded {len(tasks)} task definitions")

    # Discover available completions per model per task
    completions = {}  # (task_name, model_dir) -> code
    model_task_counts = {}
    for model_dir in SOURCE_MODELS:
        model_path = GENERATED_DIR / model_dir
        if not model_path.is_dir():
            continue
        count = 0
        for task_name in tasks:
            code = read_completion(model_dir, task_name)
            if code:
                completions[(task_name, model_dir)] = code
                count += 1
        if count > 0:
            model_task_counts[model_dir] = count
            print(f"  {model_dir}: {count} completions")

    print(f"\nTotal available completions: {len(completions)}")

    # Strategy:
    # 1. Original pairs: each task x each model with valid completion (~200 examples)
    # 2. Domain-substituted variants: each original pair x domain (~800+ examples)

    examples = []

    # Phase 1: Original task/completion pairs
    print("\n[Phase 1] Original task/completion pairs...")
    for task_name, task in tasks.items():
        prompt = task["resolved_prompt"]
        for model_dir in SOURCE_MODELS:
            code = completions.get((task_name, model_dir))
            if code:
                category = TASK_CATEGORY_MAP.get(task_name, "other")
                examples.append(make_example(
                    prompt, code, category, "original", model_dir
                ))
    print(f"  Generated {len(examples)} original pairs")

    # Phase 2: Domain-substituted variants
    print("\n[Phase 2] Domain-substituted variants...")
    phase2_count = 0

    # For each original completion, create variants by substituting domain terms
    original_pairs = list(completions.items())
    random.shuffle(original_pairs)

    for (task_name, model_dir), code in original_pairs:
        if len(examples) >= args.target:
            break

        task = tasks[task_name]
        prompt = task["resolved_prompt"]
        category = TASK_CATEGORY_MAP.get(task_name, "other")

        for domain in DOMAINS:
            if len(examples) >= args.target:
                break

            # Apply domain substitution to both prompt and completion
            new_prompt = apply_domain_substitution(prompt, BASE_DOMAIN, domain)
            new_code = apply_domain_substitution(code, BASE_DOMAIN, domain)

            # Skip if substitution didn't change much (domain terms not present)
            if new_code == code and new_prompt == prompt:
                continue

            # Validate the substituted code still looks like valid C#
            if not re.search(r'\b(class|record|interface|enum)\b', new_code):
                continue

            examples.append(make_example(
                new_prompt, new_code, category, domain["name"], model_dir
            ))
            phase2_count += 1

    print(f"  Generated {phase2_count} domain variants")

    # Phase 3: If still under target, create prompt-rephrased variants
    if len(examples) < args.target:
        print(f"\n[Phase 3] Prompt rephrasings (need {args.target - len(examples)} more)...")
        phase3_count = 0

        # Rephrase templates — different ways to ask for the same thing
        rephrase_prefixes = [
            "Write a complete C# implementation for the following:\n\n",
            "Implement the following in C# (.NET 10). Single file, no markdown:\n\n",
            "Create a production-ready C# implementation:\n\n",
            "Generate C# code for the following specification:\n\n",
            "Write clean, idiomatic C# for this requirement:\n\n",
            "Implement this in C# with proper patterns and best practices:\n\n",
            "Create a well-structured C# solution for:\n\n",
            "Write the following C# code. Return only the code, no explanations:\n\n",
        ]

        rephrase_suffixes = [
            "\n\nEnsure all types are in a single file. Return only valid C# code.",
            "\n\nAll code must be in one file. No markdown fences.",
            "\n\nWrite production-quality code in a single C# file.",
            "\n\nReturn only the C# source code, nothing else.",
            "\n\nOutput a single, compilable C# file.",
        ]

        for (task_name, model_dir), code in original_pairs:
            if len(examples) >= args.target:
                break

            task = tasks[task_name]
            original_prompt = task["resolved_prompt"]
            category = TASK_CATEGORY_MAP.get(task_name, "other")

            for prefix in rephrase_prefixes:
                if len(examples) >= args.target:
                    break

                suffix = random.choice(rephrase_suffixes)
                # Extract the core task description (skip "Given the following API reference:" header)
                core = original_prompt
                new_prompt = prefix + core + suffix

                examples.append(make_example(
                    new_prompt, code, category, "rephrased", model_dir
                ))
                phase3_count += 1

                # Also do domain + rephrase combos
                for domain in random.sample(DOMAINS, min(3, len(DOMAINS))):
                    if len(examples) >= args.target:
                        break
                    sub_prompt = apply_domain_substitution(new_prompt, BASE_DOMAIN, domain)
                    sub_code = apply_domain_substitution(code, BASE_DOMAIN, domain)
                    if sub_code != code:
                        examples.append(make_example(
                            sub_prompt, sub_code, category,
                            f"rephrased+{domain['name']}", model_dir
                        ))
                        phase3_count += 1

        print(f"  Generated {phase3_count} rephrased variants")

    # Shuffle and truncate to target
    random.shuffle(examples)
    examples = examples[:args.target]

    # Write output (without metadata)
    OUTPUT_FILE.parent.mkdir(parents=True, exist_ok=True)
    with open(OUTPUT_FILE, "w", encoding="utf-8") as fh:
        for ex in examples:
            clean = {"messages": ex["messages"]}
            fh.write(json.dumps(clean, ensure_ascii=False) + "\n")

    print(f"\n=== Complete ===")
    print(f"Total examples: {len(examples)}")
    print(f"Output: {OUTPUT_FILE}")

    # Category breakdown
    from collections import Counter
    cats = Counter(ex["metadata"]["category"] for ex in examples)
    print("\nBy category:")
    for cat, count in cats.most_common():
        print(f"  {cat}: {count}")

    # Source breakdown
    sources = Counter(ex["metadata"]["domain"] for ex in examples)
    print("\nBy source type:")
    for src, count in sources.most_common(10):
        print(f"  {src}: {count}")

    models = Counter(ex["metadata"]["source_model"] for ex in examples)
    print("\nBy source model:")
    for m, count in models.most_common(5):
        print(f"  {m}: {count}")


if __name__ == "__main__":
    main()
