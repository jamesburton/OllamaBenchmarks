# Coding Benchmark Suite — Design Spec

**Date:** 2026-03-23
**Goal:** Differentiate the 21 models that score 5/5 on the current quick quality suite with a rigorous, multi-layer coding benchmark weighted toward C# and .NET 10 practical tasks.

## Composite Scoring

| Layer | Weight | Problems | Language |
|-------|-------:|--------:|----------|
| 1. EvalPlus HumanEval+ | 15% | 164 | Python |
| 2. MultiPL-E C# | 25% | 164 | C# |
| 3. Custom .NET Practical Suite | 60% | 20 | C# (.NET 10) |

Within Layer 3, full vertical-slice tasks carry **2x weight** vs single-category tasks.

Final composite per model:

```
score = 0.15 * layer1_pass_rate
      + 0.25 * layer2_pass_rate
      + 0.60 * layer3_weighted_score
```

Where `layer3_weighted_score` is:

```
layer3 = (sum_of_category_scores + 2 * sum_of_vertical_slice_scores)
       / (num_category_tasks + 2 * num_vertical_slice_tasks)
```

With 17 category tasks (weight 1) and 3 vertical slices (weight 2), the denominator is always **23 weighted units**.

All pass rates are 0.0–1.0. Each task is binary pass/fail (code compiles AND all tests pass = 1, else 0). Timeouts and harness errors count as **0** and are always included in the denominator (no score inflation from skipped tasks). A `harness_error` field is recorded alongside `passed` for diagnostics.

---

## Layer 1: EvalPlus HumanEval+ (Python)

**Purpose:** Establish baseline pass@1 rate on a well-understood benchmark.

**Runner:** `evalplus` package with native Ollama backend.

```bash
pip install "evalplus @ git+https://github.com/evalplus/evalplus"
evalplus.evaluate \
  --model <model> \
  --dataset humaneval \
  --backend ollama \
  --base-url http://localhost:11434/v1 \
  --greedy
```

**Output:** pass@1 rate (0.0–1.0) per model.

**Integration:** A wrapper script (`scripts/benchmark_coding_layer1.py`) invokes evalplus, parses the result JSON, and writes a per-model checkpoint as a nested key within `results/coding-<model_slug>.json`.

---

## Layer 2: MultiPL-E C# (HumanEval translated)

**Purpose:** Test C# code generation on the same canonical problems.

**Runner:** Custom harness (consistent with Layer 3 architecture) that:

1. Loads the MultiPL-E HumanEval-C# dataset (from HuggingFace `nuprl/MultiPL-E`)
2. For each problem, sends the C# prompt to Ollama `/v1/chat/completions`
3. Writes the generated code into a temp .NET console project using top-level statements
4. Appends the MultiPL-E-provided test assertions as inline code after the generated function
5. Runs `dotnet run` — exit code 0 = pass, non-zero = fail
6. Records pass/fail per problem

The temp project uses a minimal `.csproj` (no test framework, just `net10.0` console app) since MultiPL-E tests are simple assertion-based programs, not xUnit tests.

**Ollama call parameters:** `temperature=0`, `seed=42`, `max_tokens=2048`, `num_ctx=4096`. Model-family sampling overrides (e.g., Nemotron at `temperature=1.0`) apply per existing `sampling_options()` convention.

**Per-task timeout:** 60s for generation, 30s for build+run.

**Prerequisites:** .NET 10 SDK installed.

**Output:** pass@1 rate (0.0–1.0) per model, stored as a nested key in `results/coding-<model_slug>.json`.

---

## Layer 3: Custom .NET Practical Suite

**Purpose:** Test real-world .NET coding ability with the specific libraries and patterns used in production.

### Architecture

```
scripts/
  benchmark_coding_layer3.py        # Orchestrator
  coding_tasks/
    __init__.py
    task_runner.py                   # Sends prompt, compiles, tests
    references/                     # API reference snippets per library
      xunit_v3.md
      masstransit_v8.md
      awesome_assertions.md
      nsubstitute.md
      oneof.md
      efcore_10.md
      blazor_net10.md
      aspnet_net10.md
    tasks/
      01_aspnet_oneof_controller.yaml
      02_aspnet_validation_endpoint.yaml
      03_aspnet_service_di.yaml
      04_efcore_leftjoin.yaml
      05_efcore_json_columns.yaml
      06_efcore_executeupdate.yaml
      07_masstransit_consumer.yaml
      08_masstransit_statemachine.yaml
      09_masstransit_test_harness.yaml
      10_blazor_interactive_component.yaml
      11_blazor_streaming_render.yaml
      12_blazor_state_persistence.yaml
      13_xunit_v3_test_generation.yaml
      14_xunit_awesome_nsubstitute.yaml
      15_xunit_assembly_fixture.yaml
      16_linq_complex_query.yaml
      17_async_cancellation.yaml
      18_vertical_order_api.yaml
      19_vertical_saga_workflow.yaml
      20_vertical_blazor_crud.yaml
    templates/
      test_project/                 # xUnit test project for most tasks
        TestProject.csproj
        GlobalUsings.cs
      blazor_project/               # Razor-capable project for Blazor tasks
        BlazorTestProject.csproj
        GlobalUsings.cs
```

### Test Project Templates

**Standard test project** (tasks 1–9, 13–19):

```xml
<!-- templates/test_project/TestProject.csproj -->
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

**Blazor-capable test project** (tasks 10–12, 20):

```xml
<!-- templates/blazor_project/BlazorTestProject.csproj -->
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

Blazor tasks test the code-behind C# logic and use bUnit for component rendering tests, avoiding raw `.razor` compilation issues. Task prompts ask for `@code` blocks as standalone C# classes with `[Parameter]` attributes, plus bUnit test code.

### NuGet Cache Strategy

At startup, the task runner:

1. Copies the template project to a staging directory
2. Runs `dotnet restore` once to populate the global NuGet cache
3. For each task, copies the pre-restored template (including `obj/` artifacts) into a temp directory
4. Writes generated + test code into the temp directory
5. Runs `dotnet build --no-restore` and `dotnet test --no-restore --no-build` (after a successful build)

This ensures NuGet packages are downloaded once, not 420 times.

### Task Definition Format (YAML)

```yaml
name: aspnet_oneof_controller
category: aspnet             # aspnet | efcore | masstransit | blazor | xunit | linq | vertical
weight: 1                    # 1 for category tasks, 2 for vertical slices
template: test_project       # test_project | blazor_project
max_tokens: 4096             # default; vertical slices use 8192
num_ctx: 12288               # Ollama context window; ensures prompt + completion fits
description: >
  Generate an ASP.NET controller that returns OneOf<User, NotFound, ValidationError>
  and maps each case to the correct HTTP response.
references:
  - oneof.md
  - aspnet_net10.md
prompt: |
  Given the following API reference:
  {references}

  Create an ASP.NET Core 10 controller `UsersController` with:
  - GET /api/users/{id} that calls IUserService.GetByIdAsync(int id)
  - The service returns OneOf<User, NotFound, ValidationError>
  - Map User to 200 OK, NotFound to 404, ValidationError to 400 with message
  - Use record types for NotFound and ValidationError
  - Include the User record with Id, Name, Email properties
  - Include the IUserService interface

  Return only valid C# code in a single file.
test_code: |
  using AwesomeAssertions;
  using Microsoft.AspNetCore.Mvc;
  using NSubstitute;
  using OneOf;

  public class UsersControllerTests
  {
      [Fact]
      public async Task Get_ExistingUser_Returns200()
      {
          var service = Substitute.For<IUserService>();
          service.GetByIdAsync(1).Returns(
              OneOf<User, NotFound, ValidationError>.FromT0(
                  new User { Id = 1, Name = "Alice", Email = "a@b.com" }));
          var controller = new UsersController(service);
          var result = await controller.Get(1);
          result.Should().BeOfType<OkObjectResult>();
      }

      [Fact]
      public async Task Get_MissingUser_Returns404()
      {
          var service = Substitute.For<IUserService>();
          service.GetByIdAsync(99).Returns(
              OneOf<User, NotFound, ValidationError>.FromT1(new NotFound()));
          var controller = new UsersController(service);
          var result = await controller.Get(99);
          result.Should().BeOfType<NotFoundResult>();
      }

      [Fact]
      public async Task Get_InvalidId_Returns400()
      {
          var service = Substitute.For<IUserService>();
          service.GetByIdAsync(-1).Returns(
              OneOf<User, NotFound, ValidationError>.FromT2(
                  new ValidationError("ID must be positive")));
          var controller = new UsersController(service);
          var result = await controller.Get(-1);
          result.Should().BeOfType<BadRequestObjectResult>();
      }
  }
```

### Task Runner Flow

For each task:

1. **Load** task YAML + referenced API docs
2. **Build prompt** by injecting references (max 2 files for category tasks, max 4 for vertical slices)
3. **Send** to Ollama `/v1/chat/completions` with task-specific `max_tokens`, `num_ctx`, `temperature=0`, `seed=42`. Apply model-family sampling overrides per existing `sampling_options()` convention.
4. **Extract** C# code from response (strip markdown fences if present)
5. **Write** generated code + test code into pre-restored temp .NET project (selected by `template` field)
6. **Compile** with `dotnet build --no-restore` (timeout: 60s) — record build success/failure
7. **Test** with `dotnet test --no-restore --no-build` (timeout: 60s) — record test pass/fail count
8. **Score** — binary: 1 if all tests pass, 0 otherwise. Timeouts and errors score 0.
9. **Save** per-task result with build output, test output, and path to generated code sidecar file

**Generation timeout:** 120s for category tasks, 300s for vertical slices.

### All 20 Tasks

**ASP.NET + OneOf (3 tasks, weight 1 each):**

1. **aspnet_oneof_controller** — GET endpoint returning `OneOf<User, NotFound, ValidationError>`, map to correct HTTP status codes
2. **aspnet_validation_endpoint** — POST endpoint with .NET 10 built-in validation, `[Required]`/`[Range]` on a `CreateOrderRequest`, return `ProblemDetails` on failure
3. **aspnet_service_di** — Register services with correct lifetimes (scoped DbContext, singleton config, transient handler), wire up options pattern from `IConfiguration`

**EF Core 10 (3 tasks, weight 1 each):**

4. **efcore_leftjoin** — Use EF Core 10 `LeftJoin` to query orders with optional customer data, project to DTO
5. **efcore_json_columns** — Map a `ComplexType` `Address` to a JSON column, write a query filtering on a nested JSON property
6. **efcore_executeupdate** — Bulk update with `ExecuteUpdateAsync` including a JSON column property update

**MassTransit v8 (3 tasks, weight 1 each):**

7. **masstransit_consumer** — Consumer for `SubmitOrder` that publishes `OrderSubmitted`, with retry middleware and consumer definition
8. **masstransit_statemachine** — `OrderStateMachine` with Initially/During/Schedule, at least 3 states and 2 events
9. **masstransit_test_harness** — xUnit v3 test using `AddMassTransitTestHarness` to verify the consumer from task 7

**Blazor .NET 10 (3 tasks, weight 1 each, uses `blazor_project` template):**

10. **blazor_interactive_component** — Counter component as C# code-behind class with `[Parameter]`, event callback to parent; bUnit test validates rendering and click behavior
11. **blazor_streaming_render** — Data-loading component using `[StreamRendering]` pattern; bUnit test validates placeholder then data state
12. **blazor_state_persistence** — Component using `[SupplyParameterFromPersistentComponentState]`; bUnit test validates state survives re-render

**xUnit v3 + Testing Libraries (3 tasks, weight 1 each):**

13. **xunit_v3_test_generation** — Given an `OrderService` class, generate xUnit v3 tests using `[Fact]`, `[Theory]`, `Assert.Multiple()`, and `MatrixTheoryData`
14. **xunit_awesome_nsubstitute** — Test a `NotificationService` using NSubstitute mocks and AwesomeAssertions, verify `.Received()` calls and `.Should().BeEquivalentTo()`
15. **xunit_assembly_fixture** — Test class using `[AssemblyFixture]` for shared state, `IAsyncLifetime` with `ValueTask`, and `TestContext.Current`

**LINQ + Async (2 tasks, weight 1 each):**

16. **linq_complex_query** — Chain `GroupBy`, `SelectMany`, `Aggregate` to transform sales data; include C# 14 extension property
17. **async_cancellation** — Convert sync method to async with proper `CancellationToken` propagation, `ConfigureAwait(false)`, and `IAsyncEnumerable` yield

**Full Vertical Slices (3 tasks, weight 2 each):**

18. **vertical_order_api** — Complete order API: ASP.NET controller + EF Core 10 DbContext + `OneOf` return types + service layer + xUnit v3 tests with NSubstitute + AwesomeAssertions. Must compile and pass tests as a unit. (`max_tokens: 8192`)
19. **vertical_saga_workflow** — MassTransit saga: define messages, state machine with 4+ states, EF persistence config, and test harness test verifying the happy path. All in one generation. (`max_tokens: 8192`)
20. **vertical_blazor_crud** — Blazor code-behind class that displays a list from an injected service, supports add/delete with event callbacks, uses streaming rendering for initial load, plus an xUnit v3 bUnit test for the component and a service-layer test. Uses `blazor_project` template. (`max_tokens: 8192`)

---

## Reference Context Strategy

Each task prompt includes a **reference block** assembled from the `references/` directory. These are curated API snippets (not full docs) giving models the signatures and patterns they need.

**Constraints:**
- Category tasks: max **2 reference files** (~1000 tokens total)
- Vertical slice tasks: max **4 reference files** (~2000 tokens total)
- All tasks use `num_ctx: 12288` minimum to ensure prompt + completion fits

| Reference File | Content | Source |
|---------------|---------|--------|
| `xunit_v3.md` | Key attributes, `Assert.Multiple`, `MatrixTheoryData`, `[AssemblyFixture]`, `IAsyncLifetime` (ValueTask), `TestContext.Current` | xunit.net/docs |
| `masstransit_v8.md` | Consumer/StateMachine patterns, `.AddMassTransit()` config, test harness, EF saga persistence | masstransit.io |
| `awesome_assertions.md` | `Should()`, `BeEquivalentTo()`, `ThrowAsync()`, collection assertions | awesomeassertions.org |
| `nsubstitute.md` | `Substitute.For<T>()`, `.Returns()`, `.Received()`, `Arg.Any/Is`, async mocking | nsubstitute.github.io |
| `oneof.md` | `OneOf<T0,T1,T2>`, `.Match()`, `.Switch()`, ASP.NET controller pattern | github.com/mcintyre321/OneOf |
| `efcore_10.md` | `LeftJoin`, JSON columns, `ExecuteUpdateAsync`, named query filters | learn.microsoft.com/ef |
| `blazor_net10.md` | Render modes, `[StreamRendering]`, `[SupplyParameterFromPersistentComponentState]`, bUnit patterns | learn.microsoft.com/aspnet/blazor |
| `aspnet_net10.md` | Built-in validation, SSE, `ProblemDetails`, minimal API patterns | learn.microsoft.com/aspnet |

Each reference file targets **<500 tokens** — essential types, signatures, and one idiomatic example per pattern.

---

## Output Format

Per-model result file (`results/coding-<model_slug>.json`):

```json
{
  "model": "glm-4.7-flash",
  "benchmark": "coding",
  "run_started_at": "2026-03-24T08:00:00+00:00",
  "run_finished_at": "2026-03-24T10:00:00+00:00",
  "layer1_pass_rate": 0.72,
  "layer2_pass_rate": 0.58,
  "layer3_results": [
    {
      "task": "aspnet_oneof_controller",
      "category": "aspnet",
      "weight": 1,
      "passed": true,
      "harness_error": null,
      "build_success": true,
      "tests_passed": 3,
      "tests_total": 3,
      "generation_time_s": 12.4,
      "generated_code_path": "results/coding-generated/glm-4.7-flash/01_aspnet_oneof_controller.cs"
    }
  ],
  "layer3_weighted_score": 0.65,
  "composite_score": 0.632
}
```

Generated code is stored in sidecar files under `results/coding-generated/<slug>/` to keep result JSON small and git-diffable. The `--save-code` flag controls whether sidecar files are written (default: on).

Aggregate file (`results/coding-current.json`) ranks all models by composite score, following existing `rebuild_benchmark_aggregate.py` conventions. The coding benchmark uses its own rebuild logic since it combines three layers.

Timestamps use `datetime.datetime.now(datetime.timezone.utc).isoformat()` (producing `+00:00` suffix) consistent with all existing scripts.

---

## Implementation Sequence

1. **Phase 1:** Reference docs — curate the 8 reference files from research findings
2. **Phase 2:** Test project templates — two `.csproj` files (standard + Blazor), `GlobalUsings.cs`, pre-restore logic
3. **Phase 3:** Task runner — Python harness: prompt assembly, Ollama call, temp project creation, `dotnet build --no-restore`/`dotnet test --no-restore`, scoring, timeout handling
4. **Phase 4:** Layer 3 tasks — write all 20 YAML task definitions with test code
5. **Phase 5:** Layer 1 wrapper — EvalPlus integration script
6. **Phase 6:** Layer 2 wrapper — custom MultiPL-E C# harness (consistent with Layer 3 architecture)
7. **Phase 7:** Composite scorer — combine all three layers, generate rankings, produce `coding-current.json`
8. **Phase 8:** Run against top 21 models, publish results

---

## Prerequisites

- Python 3.12+ with `evalplus`, `requests`, `pyyaml`
- .NET 10 SDK
- Ollama running with target models pulled
- ~2-4 hours per model for full suite (164 + 164 + 20 tasks)
