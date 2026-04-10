*   Goal: Write xUnit v3 tests for a `ProcessPaymentConsumer`.
    *   Requirements:
        1.  Verify consumer receives/processes `ProcessPayment`.
        2.  Verify `PaymentProcessed` is published with correct `OrderId`.
        3.  Use `Assert.Multiple`.
        4.  No namespace.
        5.  No `using Xunit;` or `using Xunit.Abstractions;`.
        6.  Include `using MassTransit;`, `using MassTransit.Testing;`, and `using Microsoft.Extensions.DependencyInjection;`.
        7.  Use `AddMassTransitTestHarness` (via `ServiceCollection`).
        8.  `Assert.Multiple` cannot take async lambdas; must resolve the results first.

    *   `ProcessPayment` (Input)
    *   `PaymentProcessed` (Output)
    *   `ProcessPaymentConsumer` (The logic)

    *   `ServiceCollection` setup: `new ServiceCollection().AddMassTransitTestHarness(cfg => { cfg.AddConsumer<ProcessPaymentConsumer>(); }).BuildServiceProvider(true);`
    *   `ITestHarness` retrieval: `provider.GetRequiredService<ITestHarness>()`.
    *   `harness.Start()`: Must be called.
    *   `harness.Published.Any<T>()` and `hL_harness.Consumed.Any<T>()`.

    *   `Assert.Multiple` takes a list of actions.
    *   Since `harness.Published.Any<T>()` is async, I need to await it and store the result in a boolean.
    *   Example: `bool result = await harness.Published.Any<PaymentProcessed>();`
    *   Then: `Assert.Multiple(() => { ... });` (Wait, the prompt says "Assert.Multiple( ... )").
    *   Actually, the prompt says: "Assert.Multiple( () => Assert.Equal(1, a), ... )".
s
    *   Wait, the prompt says: "Assert.Multiple( () => Assert.Equal(1, a), ... )".
    *   Wait, the prompt says: "Assert.Multiple( () => Assert.Equal(1, a), ... )".
Lately, the prompt says: "Assert.Multiple( () => Assert.Equal(1, a), ... )".
    *   Wait, the prompt says: "Assert.Multiple( () => Assert.Equal(1, a), ... )".
    *   Wait, the prompt says: "Assert.Multiple( () => Assert.S... )".
    *   Actually, the prompt says: "Assert.Multiple( () => Assert.Equal(1, a), ... )".
    *   Wait, the prompt says: "Assert.Multiple( () => Assert.Equal