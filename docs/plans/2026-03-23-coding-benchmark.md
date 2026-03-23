# Coding Benchmark Suite Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a three-layer coding benchmark that differentiates the 21 models scoring 5/5 on the quick quality suite, weighted heavily toward C# / .NET 10 practical tasks.

**Architecture:** A Python orchestrator per layer talks to Ollama's OpenAI-compatible `/v1/chat/completions` endpoint. Layer 3 generates C# code, writes it into pre-restored .NET test projects, and runs `dotnet build`/`dotnet test` to score pass/fail. Results merge into a single `results/coding-<slug>.json` per model with a composite score.

**Tech Stack:** Python 3.12+, .NET 10 SDK, xUnit v3, AwesomeAssertions, NSubstitute, OneOf, MassTransit v8, EF Core 10, bUnit, evalplus

**Spec:** `docs/specs/2026-03-23-coding-benchmark-design.md`

---

## File Structure

```
scripts/
  benchmark_coding.py                    # Top-level orchestrator (all 3 layers)
  benchmark_coding_layer1.py             # EvalPlus HumanEval+ wrapper
  benchmark_coding_layer2.py             # MultiPL-E C# custom harness
  benchmark_coding_layer3.py             # .NET practical suite orchestrator
  benchmark_coding_composite.py          # Combine all 3 layers into final rankings
  coding_tasks/
    __init__.py
    task_runner.py                       # Core: prompt → Ollama → dotnet build/test → score
    code_extractor.py                    # Extract C# from LLM response (strip fences etc)
    references/
      xunit_v3.md
      masstransit_v8.md
      awesome_assertions.md
      nsubstitute.md
      oneof.md
      efcore_10.md
      blazor_net10.md
      aspnet_net10.md
    tasks/
      01_aspnet_oneof_controller.yaml  ... 20_vertical_blazor_crud.yaml
    templates/
      test_project/
        TestProject.csproj
        GlobalUsings.cs
      blazor_project/
        BlazorTestProject.csproj
        GlobalUsings.cs
      layer2_project/
        Layer2Project.csproj
results/
  coding-<model_slug>.json              # Per-model result (all 3 layers)
  coding-current.json                   # Aggregate rankings
  coding-generated/<slug>/<task>.cs     # Generated code sidecar files
```

---

### Task 1: Reference Documentation Files

**Files:**
- Create: `scripts/coding_tasks/references/xunit_v3.md`
- Create: `scripts/coding_tasks/references/masstransit_v8.md`
- Create: `scripts/coding_tasks/references/awesome_assertions.md`
- Create: `scripts/coding_tasks/references/nsubstitute.md`
- Create: `scripts/coding_tasks/references/oneof.md`
- Create: `scripts/coding_tasks/references/efcore_10.md`
- Create: `scripts/coding_tasks/references/blazor_net10.md`
- Create: `scripts/coding_tasks/references/aspnet_net10.md`

Each file is a curated API cheat-sheet (<500 tokens) with essential types, signatures, and one idiomatic example per pattern. These are injected into prompts so models aren't disadvantaged by training cutoff.

- [ ] **Step 1: Create `xunit_v3.md`**

Content: `[Fact]`, `[Theory]`, `[InlineData]`, `Assert.Multiple()`, `MatrixTheoryData<>`, `[AssemblyFixture]`, `IAsyncLifetime` (returns `ValueTask` in v3), `TestContext.Current`, `[Explicit]`. One example of `Assert.Multiple` and one of `[AssemblyFixture]`. Note: package is `xunit.v3`, project must NOT have `OutputType=Exe`.

Source: https://xunit.net/docs/getting-started/v3/whats-new

- [ ] **Step 2: Create `masstransit_v8.md`**

Content: `IConsumer<T>`, `MassTransitStateMachine<TState>`, `ConsumerDefinition<T>`, `.AddMassTransit()` with `ConfigureEndpoints`, `AddMassTransitTestHarness`, `ITestHarness`, `GetConsumerHarness<T>`, saga state `InstanceState()`, `Initially/During/When/TransitionTo`. Note: default serializer is System.Text.Json, use `IBusRegistrationConfigurator` not `IServiceCollectionBusConfigurator`.

Source: https://masstransit.io/documentation/configuration

- [ ] **Step 3: Create `awesome_assertions.md`**

Content: `.Should().Be()`, `.BeEquivalentTo()`, `.HaveCount()`, `.Contain()`, `.BeOfType<T>()`, `.Throw<T>()/.ThrowAsync<T>()`, `.WithMessage()`. Note: namespace is `AwesomeAssertions` (not `FluentAssertions`), package `AwesomeAssertions` v9.4.0.

Source: https://awesomeassertions.org/introduction

- [ ] **Step 4: Create `nsubstitute.md`**

Content: `Substitute.For<T>()`, `.Returns()`, `.Received()`, `.DidNotReceive()`, `Arg.Any<T>()`, `Arg.Is<T>()`, async returns pattern, `Received.InOrder()`. Package: `NSubstitute` v5.3.0.

Source: https://nsubstitute.github.io/help/getting-started/

- [ ] **Step 5: Create `oneof.md`**

Content: `OneOf<T0,T1,T2>`, `.Match()`, `.Switch()`, `.FromT0/T1/T2()`, ASP.NET controller pattern mapping to `IActionResult`. Package: `OneOf` v3.0.271.

Source: https://github.com/mcintyre321/OneOf

- [ ] **Step 6: Create `efcore_10.md`**

Content: `.LeftJoin()`, `[ComplexType]` with JSON columns, `ExecuteUpdateAsync` with `.SetProperty()`, `HasJsonConversion()`, named query filters. Package: `Microsoft.EntityFrameworkCore.InMemory` v10.0.0.

Source: https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew

- [ ] **Step 7: Create `blazor_net10.md`**

Content: `@rendermode InteractiveServer`, `[StreamRendering]`, `[SupplyParameterFromPersistentComponentState]`, `[Parameter]`, `EventCallback<T>`, `[CascadingParameter]`, bUnit `RenderComponent<T>()`, `.Find()`, `.Click()`. Package: `bunit` v2.*.

Source: https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes

- [ ] **Step 8: Create `aspnet_net10.md`**

Content: `[Required]`/`[Range]` on minimal API params (auto-validated in .NET 10), `Results.Ok/NotFound/BadRequest`, `ProblemDetails`, `FrameworkReference Microsoft.AspNetCore.App`, DI lifetimes (`AddScoped/Singleton/Transient`), options pattern `Configure<T>`.

Source: https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0

- [ ] **Step 9: Commit**

```bash
git add scripts/coding_tasks/references/
git commit -m "feat(coding-bench): add 8 API reference docs for prompt context"
```

---

### Task 2: .NET Project Templates

**Files:**
- Create: `scripts/coding_tasks/templates/test_project/TestProject.csproj`
- Create: `scripts/coding_tasks/templates/test_project/GlobalUsings.cs`
- Create: `scripts/coding_tasks/templates/blazor_project/BlazorTestProject.csproj`
- Create: `scripts/coding_tasks/templates/blazor_project/GlobalUsings.cs`
- Create: `scripts/coding_tasks/templates/layer2_project/Layer2Project.csproj`

- [ ] **Step 1: Create standard test project csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
    <PackageReference Include="xunit.v3" Version="3.2.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.0.1" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="AwesomeAssertions" Version="9.4.0" />
    <PackageReference Include="NSubstitute" Version="5.3.0" />
    <PackageReference Include="OneOf" Version="3.0.271" />
    <PackageReference Include="MassTransit" Version="8.3.6" />
    <PackageReference Include="MassTransit.EntityFrameworkCore" Version="8.3.6" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.0" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Create GlobalUsings.cs for test project**

```csharp
global using Xunit;
global using AwesomeAssertions;
global using NSubstitute;
```

- [ ] **Step 3: Create Blazor test project csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="xunit.v3" Version="3.2.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.0.1" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="AwesomeAssertions" Version="9.4.0" />
    <PackageReference Include="NSubstitute" Version="5.3.0" />
    <PackageReference Include="bunit" Version="2.*" />
  </ItemGroup>
</Project>
```

- [ ] **Step 4: Create GlobalUsings.cs for Blazor project**

Same xUnit + AwesomeAssertions + NSubstitute + `Bunit`.

- [ ] **Step 5: Create Layer 2 console project csproj**

Minimal `Microsoft.NET.Sdk` console project, `net10.0`, `OutputType=Exe`, no test packages. This is for MultiPL-E C# problems that use assertion-based programs (not xUnit).

- [ ] **Step 6: Verify templates restore**

```bash
cd scripts/coding_tasks/templates/test_project && dotnet restore
cd ../blazor_project && dotnet restore
cd ../layer2_project && dotnet restore
```

All three must succeed. If any NuGet version is wrong, fix it now.

- [ ] **Step 7: Commit**

```bash
git add scripts/coding_tasks/templates/
git commit -m "feat(coding-bench): add .NET 10 project templates for test harness"
```

---

### Task 3: Code Extractor Module

**Files:**
- Create: `scripts/coding_tasks/__init__.py`
- Create: `scripts/coding_tasks/code_extractor.py`

- [ ] **Step 1: Create `scripts/coding_tasks/__init__.py`**

Empty file. Required for Python package imports.

- [ ] **Step 2: Create `code_extractor.py`**

Functions needed:
- `extract_csharp(text: str) -> str` — Strips markdown fences (` ```csharp `, ` ```cs `, ` ``` `), handles multiple code blocks (concatenate), strips leading prose. Similar to existing `extract_code()` in `benchmark_quality.py` but targets C# instead of Python.
- `extract_python(text: str) -> str` — Reuse existing pattern from `benchmark_quality.py`.

- [ ] **Step 3: Add basic smoke tests**

Run a quick manual test: pass a markdown-fenced C# response through `extract_csharp()`, verify it returns clean code.

- [ ] **Step 4: Commit**

```bash
git add scripts/coding_tasks/
git commit -m "feat(coding-bench): add code extractor for C# LLM responses"
```

---

### Task 4: Task Runner Core

**Files:**
- Create: `scripts/coding_tasks/task_runner.py`

This is the central engine. It handles: load YAML task → assemble prompt with references → call Ollama → extract code → write to temp project → `dotnet build` → `dotnet test` → return result dict.

- [ ] **Step 1: Write `TaskResult` dataclass**

```python
@dataclass
class TaskResult:
    task: str
    category: str
    weight: int
    passed: bool
    harness_error: str | None
    build_success: bool
    tests_passed: int
    tests_total: int
    generation_time_s: float
    generated_code_path: str | None
```

- [ ] **Step 2: Write `load_task(yaml_path) -> dict`**

Parse YAML, resolve reference file paths, load and concatenate reference content.

- [ ] **Step 3: Write `call_ollama(model, prompt, max_tokens, num_ctx, seed, sampling_opts, timeout) -> str`**

POST to `http://127.0.0.1:11434/v1/chat/completions` (OpenAI-compatible endpoint, NOT `/api/chat`). Payload is `{"model": model, "messages": [{"role": "user", "content": prompt}], "max_tokens": max_tokens, "temperature": ..., "seed": seed}`. Parameters are top-level keys — do NOT wrap in an `options` dict.

Use existing `sampling_options()` convention from `benchmark_quality.py` for model-family overrides (e.g., Nemotron at temperature=1.0). Default timeout 120s for category tasks; vertical tasks (weight=2) use 300s. Return response `choices[0].message.content` string.

- [ ] **Step 4: Write `setup_template_cache(template_dir, cache_dir) -> Path`**

Copies the template project to a staging directory, runs `dotnet restore`, keeps the `obj/` artifacts. Returns the cached path. Called once per template type at orchestrator startup. Subsequent task runs copy from this cached dir and use `--no-restore`.

- [ ] **Step 5: Write `run_dotnet_task(generated_code, test_code, cached_template_dir, work_dir) -> TaskResult`**

1. Copy pre-restored cached template to temp dir (including `obj/`)
2. Write `Generated.cs` (the model's output) and `Tests.cs` (the task's test code)
3. Run `dotnet build --no-restore` with 60s timeout
4. If build succeeds, run `dotnet test --no-restore --no-build` with 60s timeout
5. Parse test results from exit code + stdout
6. Return `TaskResult`

- [ ] **Step 6: Write `run_task(task_def, model, cached_template_dir, output_dir) -> TaskResult`**

Orchestrates: load task → build prompt → call Ollama (timeout from task weight) → extract code → run dotnet → save generated code sidecar (create parent dir with `os.makedirs(..., exist_ok=True)`) → return result.

- [ ] **Step 7: Test with a dummy task**

Create a minimal YAML task that asks for `public static int Add(int a, int b) => a + b;` and test code that asserts `Add(2, 3) == 5`. Run against a known model. Verify the full pipeline: prompt → generation → build → test → pass.

- [ ] **Step 8: Commit**

```bash
git add scripts/coding_tasks/task_runner.py
git commit -m "feat(coding-bench): implement task runner core (prompt → Ollama → dotnet → score)"
```

---

### Task 5: Layer 3 Orchestrator

**Files:**
- Create: `scripts/benchmark_coding_layer3.py`

- [ ] **Step 1: Write orchestrator**

Argparse interface matching project conventions:
```
python scripts/benchmark_coding_layer3.py \
  --models "glm-4.7-flash" "qwen3-coder-next" \
  --output results/coding-layer3-results.json \
  --checkpoint-dir results \
  --task-dir scripts/coding_tasks/tasks
```

Flow:
1. Pre-restore templates by calling `setup_template_cache()` for both `test_project` and `blazor_project`
2. For each model, for each task YAML in `--task-dir`:
   - Call `task_runner.run_task()`
   - Append result to model's results list
   - Write per-model checkpoint: `results/coding-<model_slug>.json`
3. Compute `layer3_weighted_score` per model using:
   ```
   layer3 = sum(task.weight * task.passed for task in results) / sum(task.weight for task in results)
   ```
   The denominator is always 23 (17 category tasks x1 + 3 vertical slices x2). Timeouts/errors count as passed=0, always included in denominator.
4. Write aggregate output

Do NOT include `host_details` in the coding benchmark output format (not part of this benchmark's spec — existing quality/throughput benchmarks embed it but coding does not).

Use `model_slug()`, `write_json()`, `sampling_options()` imported from existing utilities or reimplemented consistently.

- [ ] **Step 2: Test with 2 models on the dummy task from Task 4**

Verify checkpoint files are written, scores are correct, generated code sidecar files exist.

- [ ] **Step 3: Commit**

```bash
git add scripts/benchmark_coding_layer3.py
git commit -m "feat(coding-bench): add Layer 3 orchestrator with checkpoint and scoring"
```

---

### Task 6: Layer 3 Task Definitions (ASP.NET + OneOf)

**Files:**
- Create: `scripts/coding_tasks/tasks/01_aspnet_oneof_controller.yaml`
- Create: `scripts/coding_tasks/tasks/02_aspnet_validation_endpoint.yaml`
- Create: `scripts/coding_tasks/tasks/03_aspnet_service_di.yaml`

Each YAML file has: `name`, `category: aspnet`, `weight: 1`, `template: test_project`, `max_tokens: 4096`, `num_ctx: 12288`, `references` list, `prompt`, and `test_code`.

- [ ] **Step 1: Write task 01 — OneOf controller**

See spec for full example. Test code uses NSubstitute + AwesomeAssertions to verify OkObjectResult/NotFoundResult/BadRequestObjectResult.

- [ ] **Step 2: Write task 02 — Validation endpoint**

Prompt asks for a POST minimal API with `[Required]`/`[Range]` attributes. Test code uses `WebApplicationFactory` or manual validation to verify `ProblemDetails` response.

- [ ] **Step 3: Write task 03 — Service DI registration**

Prompt asks for a `ServiceCollectionExtensions.AddOrderServices()` method. Test code builds a `ServiceProvider` and resolves services, verifying correct lifetimes.

- [ ] **Step 4: Verify all 3 tasks pass with a known good model**

Run Layer 3 orchestrator with just these 3 tasks against a strong model (e.g., `qwen3-coder-next`). Fix any test code issues.

- [ ] **Step 5: Commit**

```bash
git add scripts/coding_tasks/tasks/01_* scripts/coding_tasks/tasks/02_* scripts/coding_tasks/tasks/03_*
git commit -m "feat(coding-bench): add ASP.NET + OneOf tasks (01-03)"
```

---

### Task 7: Layer 3 Task Definitions (EF Core 10)

**Files:**
- Create: `scripts/coding_tasks/tasks/04_efcore_leftjoin.yaml`
- Create: `scripts/coding_tasks/tasks/05_efcore_json_columns.yaml`
- Create: `scripts/coding_tasks/tasks/06_efcore_executeupdate.yaml`

- [ ] **Step 1: Write tasks 04–06**

Task 04: LeftJoin with projection. Test uses InMemory provider to seed data and verify query result.
Task 05: ComplexType Address as JSON column. Test verifies EF model builds and query compiles.
Task 06: ExecuteUpdateAsync. Test seeds rows, runs bulk update, verifies changes.

- [ ] **Step 2: Verify with known good model**
- [ ] **Step 3: Commit**

```bash
git add scripts/coding_tasks/tasks/04_* scripts/coding_tasks/tasks/05_* scripts/coding_tasks/tasks/06_*
git commit -m "feat(coding-bench): add EF Core 10 tasks (04-06)"
```

---

### Task 8: Layer 3 Task Definitions (MassTransit v8)

**Files:**
- Create: `scripts/coding_tasks/tasks/07_masstransit_consumer.yaml`
- Create: `scripts/coding_tasks/tasks/08_masstransit_statemachine.yaml`
- Create: `scripts/coding_tasks/tasks/09_masstransit_test_harness.yaml`

- [ ] **Step 1: Write tasks 07–09**

Task 07: Consumer with retry. Test uses `AddMassTransitTestHarness`.
Task 08: State machine with 3+ states. Test verifies state transitions.
Task 09: Write tests for a given consumer (model generates the test code this time).

- [ ] **Step 2: Verify with known good model**
- [ ] **Step 3: Commit**

```bash
git add scripts/coding_tasks/tasks/07_* scripts/coding_tasks/tasks/08_* scripts/coding_tasks/tasks/09_*
git commit -m "feat(coding-bench): add MassTransit v8 tasks (07-09)"
```

---

### Task 9: Layer 3 Task Definitions (Blazor)

**Files:**
- Create: `scripts/coding_tasks/tasks/10_blazor_interactive_component.yaml`
- Create: `scripts/coding_tasks/tasks/11_blazor_streaming_render.yaml`
- Create: `scripts/coding_tasks/tasks/12_blazor_state_persistence.yaml`

All use `template: blazor_project`. Prompts ask for C# code-behind classes (not .razor files). Tests use bUnit.

- [ ] **Step 1: Write tasks 10–12**
- [ ] **Step 2: Verify with known good model**
- [ ] **Step 3: Commit**

```bash
git add scripts/coding_tasks/tasks/10_* scripts/coding_tasks/tasks/11_* scripts/coding_tasks/tasks/12_*
git commit -m "feat(coding-bench): add Blazor .NET 10 tasks (10-12)"
```

---

### Task 10: Layer 3 Task Definitions (xUnit v3 + Testing Libraries)

**Files:**
- Create: `scripts/coding_tasks/tasks/13_xunit_v3_test_generation.yaml`
- Create: `scripts/coding_tasks/tasks/14_xunit_awesome_nsubstitute.yaml`
- Create: `scripts/coding_tasks/tasks/15_xunit_assembly_fixture.yaml`

- [ ] **Step 1: Write tasks 13–15**

Task 13: Given an `OrderService` class (provided in prompt), generate tests using `[Fact]`, `[Theory]`, `Assert.Multiple()`, `MatrixTheoryData`.
Task 14: Test a `NotificationService` with NSubstitute + AwesomeAssertions.
Task 15: Use `[AssemblyFixture]`, `IAsyncLifetime` with `ValueTask`, `TestContext.Current`.

For these tasks, the test code in the YAML is a **meta-test** that validates the model's generated test code compiles and runs.

- [ ] **Step 2: Verify with known good model**
- [ ] **Step 3: Commit**

```bash
git add scripts/coding_tasks/tasks/13_* scripts/coding_tasks/tasks/14_* scripts/coding_tasks/tasks/15_*
git commit -m "feat(coding-bench): add xUnit v3 + testing library tasks (13-15)"
```

---

### Task 11: Layer 3 Task Definitions (LINQ + Async)

**Files:**
- Create: `scripts/coding_tasks/tasks/16_linq_complex_query.yaml`
- Create: `scripts/coding_tasks/tasks/17_async_cancellation.yaml`

- [ ] **Step 1: Write tasks 16–17**

Task 16: LINQ `GroupBy`/`SelectMany`/`Aggregate` with C# 14 extension property. Test provides input data and expected output.
Task 17: Convert sync to async with `CancellationToken`, `ConfigureAwait(false)`, `IAsyncEnumerable`. Test verifies cancellation works.

- [ ] **Step 2: Verify with known good model**
- [ ] **Step 3: Commit**

```bash
git add scripts/coding_tasks/tasks/16_* scripts/coding_tasks/tasks/17_*
git commit -m "feat(coding-bench): add LINQ + async tasks (16-17)"
```

---

### Task 12: Layer 3 Task Definitions (Vertical Slices)

**Files:**
- Create: `scripts/coding_tasks/tasks/18_vertical_order_api.yaml`
- Create: `scripts/coding_tasks/tasks/19_vertical_saga_workflow.yaml`
- Create: `scripts/coding_tasks/tasks/20_vertical_blazor_crud.yaml`

All have `weight: 2`, `max_tokens: 8192`, up to 4 reference files.

- [ ] **Step 1: Write task 18 — vertical order API**

Prompt: complete order API with controller + EF DbContext + OneOf service + xUnit tests. Test code validates the full stack compiles and tests pass.

- [ ] **Step 2: Write task 19 — vertical saga workflow**

Prompt: MassTransit messages + state machine + EF persistence config + test harness test.

- [ ] **Step 3: Write task 20 — vertical Blazor CRUD**

Uses `blazor_project` template. Prompt: code-behind for list/add/delete + service + bUnit test + service-layer xUnit test.

- [ ] **Step 4: Verify all 3 with known good model**

These are the hardest tasks. Expect some models to fail. Verify at least one strong model passes each.

- [ ] **Step 5: Commit**

```bash
git add scripts/coding_tasks/tasks/18_* scripts/coding_tasks/tasks/19_* scripts/coding_tasks/tasks/20_*
git commit -m "feat(coding-bench): add vertical slice tasks (18-20, weight 2x)"
```

---

### Task 13: Layer 1 Wrapper (EvalPlus)

**Files:**
- Create: `scripts/benchmark_coding_layer1.py`

- [ ] **Step 1: Install and verify evalplus**

```bash
pip install "evalplus @ git+https://github.com/evalplus/evalplus"
python -m evalplus.evaluate --help | grep -q "ollama" && echo "OK: ollama backend available"
```

If `--backend ollama` is not listed, the installed version is too old — reinstall from git HEAD.

- [ ] **Step 2: Write wrapper script**

Argparse: `--models`, `--checkpoint-dir`, `--evalplus-dataset humaneval`.

For each model:
1. Shell out to `evalplus.evaluate --model <model> --dataset humaneval --backend ollama --base-url http://localhost:11434/v1 --greedy`
2. Parse result JSON for pass@1 rate
3. Write as `layer1_pass_rate` key in `results/coding-<slug>.json` checkpoint

- [ ] **Step 3: Test with one model**
- [ ] **Step 4: Commit**

```bash
git add scripts/benchmark_coding_layer1.py
git commit -m "feat(coding-bench): add Layer 1 EvalPlus HumanEval+ wrapper"
```

---

### Task 14: Layer 2 Wrapper (MultiPL-E C#)

**Files:**
- Create: `scripts/benchmark_coding_layer2.py`

- [ ] **Step 1: Write custom harness**

Argparse: `--models`, `--checkpoint-dir`, `--dataset-path` (path to MultiPL-E C# JSONL).

For each model, for each problem:
1. Send C# prompt to Ollama `/v1/chat/completions`
2. Extract code with `extract_csharp()`
3. Copy `layer2_project` template to temp dir
4. Write `Program.cs` with top-level statements: generated function + assertion code from dataset
5. Run `dotnet run --no-restore` (timeout 30s)
6. Record pass/fail

Compute pass@1 rate, write as `layer2_pass_rate` in checkpoint.

- [ ] **Step 2: Download MultiPL-E C# dataset**

```bash
pip install huggingface_hub
python -c "
from huggingface_hub import hf_hub_download
hf_hub_download('nuprl/MultiPL-E', 'data/humaneval-cs-reworded.json',
                repo_type='dataset', local_dir='scripts/coding_tasks/datasets/')
"
```

Verify the file exists at `scripts/coding_tasks/datasets/data/humaneval-cs-reworded.json`. This is the JSONL file containing the C# translations of HumanEval with test assertions. Use this path as `--dataset-path` in the harness.

- [ ] **Step 3: Test with one model on first 10 problems**

```bash
python scripts/benchmark_coding_layer2.py \
  --models "glm-4.7-flash" \
  --dataset-path scripts/coding_tasks/datasets/data/humaneval-cs-reworded.json \
  --checkpoint-dir results --limit 10
```
- [ ] **Step 4: Commit**

```bash
git add scripts/benchmark_coding_layer2.py
git commit -m "feat(coding-bench): add Layer 2 MultiPL-E C# harness"
```

---

### Task 15: Composite Scorer

**Files:**
- Create: `scripts/benchmark_coding_composite.py`

- [ ] **Step 1: Write composite scorer**

Reads all `results/coding-<slug>.json` checkpoints. For each model:
1. Read `layer1_pass_rate`, `layer2_pass_rate`, `layer3_weighted_score`
2. Compute `composite_score = 0.15 * L1 + 0.25 * L2 + 0.60 * L3`
3. Rank models by composite score

Write `results/coding-current.json` with all models ranked.

- [ ] **Step 2: Test with mock data**

Create 2 fake checkpoint files with known scores, run composite scorer, verify ranking.

- [ ] **Step 3: Commit**

```bash
git add scripts/benchmark_coding_composite.py
git commit -m "feat(coding-bench): add composite scorer combining all 3 layers"
```

---

### Task 16: Top-Level Orchestrator

**Prerequisite:** Task 15 (composite scorer) must be complete.

**Files:**
- Create: `scripts/benchmark_coding.py`

- [ ] **Step 1: Write orchestrator**

Argparse: `--models`, `--layers 1,2,3` (default all), `--output`, `--checkpoint-dir`.

Runs each selected layer script in sequence using `subprocess.run([sys.executable, "scripts/benchmark_coding_layerN.py", "--models", ...], check=True)`, then runs composite scorer the same way. Supports running individual layers for partial re-runs via `--layers 3` etc.

- [ ] **Step 2: Test end-to-end with one model**

Run full pipeline: all 3 layers → composite score → verify `results/coding-current.json`.

- [ ] **Step 3: Commit**

```bash
git add scripts/benchmark_coding.py
git commit -m "feat(coding-bench): add top-level orchestrator for all 3 layers"
```

---

### Task 17: Full Suite Run

- [ ] **Step 1: Run Layer 3 against all 21 five-star models**

```bash
python scripts/benchmark_coding_layer3.py \
  --models "glm-4.7-flash" "qwen3.5:35b-a3b" "nemotron-3-nano:30b-a3b-q8_0" \
           "qwen3-coder-next" "nemotron-cascade-2" "granite4:7b-a1b-h" \
           "granite4:32b-a9b-h" "qwen3-coder:30b" "gpt-oss:120b" \
           "qwen3.5:4b" "qwen3:8b" "qwen3.5:9b" "qwen3.5" \
           "ministral-3:14b" "qwen3:14b" "nemotron-3-super" \
           "qwen3-coder-next:q8_0" "qwen3.5:122b" "qwen3.5:122b-a10b" \
           "devstral-small-2:24b-instruct-2512-q8_0" "qwen3.5:35b-a3b-q2_k_l" \
  --checkpoint-dir results
```

- [ ] **Step 2: Run Layer 1 (EvalPlus) against all 21 models**
- [ ] **Step 3: Run Layer 2 (MultiPL-E C#) against all 21 models**
- [ ] **Step 4: Run composite scorer**
- [ ] **Step 5: Review results, commit and push**

```bash
git add results/coding-*.json results/coding-generated/
git commit -m "feat(coding-bench): full coding benchmark results for 21 five-star models"
git push origin main
```
