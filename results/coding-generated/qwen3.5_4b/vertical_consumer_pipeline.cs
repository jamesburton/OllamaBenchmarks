global using Contracts; // Wait, this is usually inside a file. If I put it at the very top, it implies the file IS in the Contracts namespace or I'm defining types there.
        // Actually, standard C# doesn't allow `global using` before a namespace declaration unless it's part of the same compilation unit context.
        // The prompt says: "Place all message types and consumer classes inside `namespace Contracts;`".
        // It also says: "Add `global using Contracts;` at the very top of the file so the types are accessible without explicit using statements from other files."
        // This implies the file content starts with `global using Contracts;` then `namespace Contracts { ... }`.

global using Contracts;
        using MassTransit; // Wait, if I put this before namespace, it's fine.
        namespace Contracts { ... }

global using Contracts;
        using MassTransit;
        using System; // etc
        namespace Contracts { ... }
        public class MyTests { ... }

global using Contracts;
        using MassTransit;
        using MassTransit.Testing;
        using Microsoft.Extensions.DependencyInjection;
        using System;
        using System.Threading.Tasks; // For ValueTask handling

var services = new ServiceCollection();
            services.AddMassTransit(x => { x.AddConsumer<PlaceOrderConsumer>(); x.AddConsumer<NotifyCustomerConsumer>(); });
            // Wait, the prompt says "Use AddMassTransitTestHarness with cfg.AddConsumer...".
            // So: services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<PlaceOrderConsumer>());

global using Contracts;
        using MassTransit;
        namespace Contracts { ... }

var services = new ServiceCollection();
            services.AddMassTransit(x => { x.AddConsumer<PlaceOrderConsumer>(); x.AddConsumer<NotifyCustomerConsumer>(); }); // Wait, prompt says AddMassTransitTestHarness cfg...
            // Actually, the prompt example shows: services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<PlaceOrderConsumer>());
            // This implies I should use AddMassTransitTestHarness.

services.AddMassTransit(x => { x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx)); });
            services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<PlaceOrderConsumer>());

services.AddMassTransit(x => { x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx)); });
            services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<PlaceOrderConsumer>());

services.AddMassTransit(x => { x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx)); });
            services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<PlaceOrderConsumer>());

. Just raw text.
    *   Wait, the system instruction says "Return ONLY valid C# code in a single file. No markdown fences."
    *   So I must not output 
.

    *   **Wait, Constraint Check:** "global using Contracts; at the very top of the file".
    *   This is tricky because `Contracts` namespace is defined later. But `global using` works if the namespace exists in the compilation unit. Since I define it immediately after, it's fine.

    *   **Wait, Constraint Check:** "Include `using MassTransit;` BEFORE the `namespace Contracts;` declaration."
    *   So:

*   Wait, if I put `global using Contracts;` first, it imports `Contracts`. Then I define `namespace Contracts`. This is valid.

    *   **Wait, Constraint Check:** "Do NOT use `Xunit.Abstractions`".
    *   Okay.

    *   **Wait, Constraint Check:** "Use positional record syntax".
    *   `public record PlaceOrder(Guid OrderId, string CustomerName);`

    *   **Wait, Constraint Check:** "xUnit v3 tests... [Fact]... Assert.Multiple".
    *   I need to use `[Fact]`.

    *   **Wait, Constraint Check:** "IAsyncLifetime — RETURNS ValueTask".
    *   `public ValueTask InitializeAsync() { ... return default; }`

    *   **Wait, Constraint Check:** "Do NOT wrap the test class in a namespace".
    *   So after `namespace Contracts { ... }`, I write `public class MyTests { ... }`.

    *   **Wait, Constraint Check:** "Use `harness.Consumed.Any<T>()` and `harness.Published.Any<T>()` for assertions."
    *   Okay.

    *   **Wait, Constraint Check:** "Return ONLY valid C# code in a single file."
    *   This means I cannot output markdown fences.

    *   **Wait, Constraint Check:** "Ensure all public types and methods exactly match the names specified in the prompt."
    *   Types: `PlaceOrder`, `OrderPlaced`, `CustomerNotified`.
    *   Consumers: `PlaceOrderConsumer`, `NotifyCustomerConsumer`.
    *   Test class name? Not specified, but I'll use `MyTests` or similar. Let's use `PipelineTests`.

    *   **Wait, Constraint Check:** "Use `AddMassTransitTestHarness` with `cfg.AddConsumer<PlaceOrderConsumer>()` and `cfg.AddConsumer<NotifyCustomerConsumer>()`."
    *   So I need to register both consumers in the harness config.

    *   **Wait, Constraint Check:** "For tests, add `using MassTransit;`, `using MassTransit.Testing;`, and `using Microsoft.Extensions.DependencyInjection;` outside the namespace."
    *   Okay.

    *   **Wait, Constraint Check:** "Do NOT use `Xunit.Abstractions`".
    *   Okay.

    *   **Wait, Constraint Check:** "Return ONLY valid C# code in a single file."
    *   This means I should not output any explanation text.

    *   **Wait, Constraint Check:** "No markdown fences."
    *   So no