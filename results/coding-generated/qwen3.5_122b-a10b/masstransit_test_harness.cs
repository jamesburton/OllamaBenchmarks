using MassTransit;
        using MassTransit.Testing;
        using Microsoft.Extensions.DependencyInjection;
        // No using Xunit;

public class ProcessPaymentConsumerTests
        {
            [Fact]
            public async Task Should_Process_Payment_And_Publish_Event()
            {
                // Setup
                var services = new ServiceCollection();
                services.AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<ProcessPaymentConsumer>();
                });
                var provider = services.BuildServiceProvider(true);
                var harness = provider.GetRequiredService<ITestHarness>();

                await harness.Start();

                // Arrange
                var orderId = Guid.NewGuid();
                var amount = 100.0m;
                var command = new ProcessPayment(orderId, amount);

                // Act
                await harness.Bus.Publish(command);

                // Wait for processing (TestHarness usually handles this, but sometimes need to wait)
                // The reference implies checking Consumed/Published directly.
                // MassTransit TestHarness waits for in-memory bus processing usually, but explicit wait might be safer.
                // However, standard pattern is just check Consumed/Published.
                // To ensure assertions pass, we might need to wait for the bus to settle.
                // The reference snippet: Assert.True(await harness.Consumed.Any<OrderPlaced>());
                // I will follow that pattern but capture the bools first.

                // Verification
                var consumed = await harness.Consumed.Any<ProcessPayment>();
                var published = await harness.Published.Any<PaymentProcessed>();

                // Need to check specific OrderId in published message.
                // harness.Published.Any<T>() returns bool. To check content, I need to get the context.
                // Reference: `harness.Published.Any<T>()` returns Task<bool>.
                // To check content, I usually need `harness.Published.Select<T>(...)` or similar.
                // Looking at MassTransit TestHarness API:
                // `harness.Published.Any<T>()` checks if *any* message of type T was published.
                // To check content, I need to retrieve the message.
                // Common pattern: `var context = await harness.Published.GetContext<PaymentProcessed>();` or iterate.
                // Actually, `harness.Published.Any<T>(predicate)` exists in some versions, but the reference says `await harness.Published.Any<T>()`.
                // Let's look at the constraint: "Use `await harness.Consumed.Any<T>()` and `await harness.Published.Any<T>()` (these return `Task<bool>`)."
                // It implies I should use these for existence.
                // But requirement 2 says: "A PaymentProcessed event is published with the correct OrderId".
                // If I only use `Any<T>()`, I can't verify the OrderId.
                // I need to access the message content.
                // MassTransit TestHarness usually allows: `var published = await harness.Published.GetContext<PaymentProcessed>();`
                // Or `harness.Published.Select<PaymentProcessed>(...)`.
                // Let's check standard MassTransit TestHarness usage.
                // `harness.Published.Any<PaymentProcessed>()` checks existence.
                // To check content: `var context = await harness.Published.GetContext<PaymentProcessed>();` (returns EnvelopeContext)
                // Or `harness.Published.Select<PaymentProcessed>(m => m.Message).FirstOrDefault()`.
                // Given the constraint "Use `await harness.Consumed.Any<T>()` and `await harness.Published.Any<T>()`", I must use these for the boolean checks.
                // But I also need to verify the OrderId.
                // I will use `harness.Published.GetContext<PaymentProcessed>()` or similar to get the message for the OrderId check.
                // Wait, the constraint says: "Use `await harness.Consumed.Any<T>()` and `await harness.Published.Any<T>()` (these return `Task<bool>`)."
                // It doesn't say "ONLY use". It says "Use ... to check".
                // However, to verify the OrderId, I need the message.
                // Let's try to find the message via `harness.Published.Select<PaymentProcessed>(...)`.
                // Actually, `harness.Published.Any<PaymentProcessed>(m => m.Message.OrderId == orderId)` might not exist in the simplified API reference provided.
                // The reference shows: `Assert.True(await harness.Consumed.Any<OrderPlaced>());`
                // I will assume standard MassTransit TestHarness capabilities allow retrieving the message.
                // `var publishedContext = await harness.Published.GetContext<PaymentProcessed>();`
                // `var message = publishedContext.Message;`
                // But `GetContext` might throw if not found.
                // Better: `var published = await harness.Published.Select<PaymentProcessed>(m => m.Message).FirstOrDefaultAsync();`
                // Or simply rely on `Any` for existence and then fetch the specific one.
                // Let's look at the constraint again: "Use `await harness.Consumed.Any<T>()` and `await harness.Published.Any<T>()` ... to check both assertions in a single test".
                // This implies the boolean existence checks are the primary assertions for `Assert.Multiple`.
                // But Requirement 2 says "verify ... with the correct OrderId".
                // So I need to assert the OrderId too.
                // I will capture the published message to check the OrderId.
                // `var publishedMessages = await harness.Published.GetContext<PaymentProcessed>();`
                // Actually, `harness.Published.Any<T>()` is sufficient for existence.
                // To check OrderId, I need the message.
                // `var context = await harness.Published.GetContext<PaymentProcessed>();`
                // If I use `GetContext`, I can check the message.
                // Let's write the code to retrieve the message.

                // Wait, `Assert.Multiple` does NOT support async lambdas.
                // So:
                // bool consumed = await harness.Consumed.Any<ProcessPayment>();
                // bool published = await harness.Published.Any<PaymentProcessed>();
                // PaymentProcessed msg = await harness.Published.GetContext<PaymentProcessed>().Message; // Hypothetical
                // Assert.Multiple(() => { Assert.True(consumed); Assert.True(published); Assert.Equal(orderId, msg.OrderId); });

                // How to get the message content in TestHarness?
                // `var published = await harness.Published.Select<PaymentProcessed>(m => m.Message).FirstOrDefaultAsync();`
                // Or `var context = await harness.Published.GetContext<PaymentProcessed>();`
                // In MassTransit v8, `harness.Published` is `IPublishTestHarness`.
                // `IPublishTestHarness` has `GetContext<T>()`.
                // Let's assume `var context = await harness.Published.GetContext<PaymentProcessed>();` works.
                // Then `context.Message.OrderId`.

                // Wait, `GetContext` returns `IPublishContext<T>`.
                // Let's refine the retrieval.
                // `var published = await harness.Published.GetContext<PaymentProcessed>();`
                // `var message = published.Message;`

                // Wait, `GetContext` might return null if not found? Or throw?
                // Usually `Any` is safer for existence.
                // I'll combine:
                // 1. Check Consumed.Any
                // 2. Check Published.Any
                // 3. Retrieve Published Message to check OrderId.

                // Wait, constraint: "Use `await harness.Consumed.Any<T>()` and `await harness.Published.Any<T>()` (these return `Task<bool>`)."
                // This might imply I should use these specific methods for the boolean checks inside `Assert.Multiple`.
                // I will do that.
                // For the OrderId check, I need to fetch the message.

                // One more constraint: "Do NOT use a namespace".
                // "Do NOT use `using Xunit;`".

                // Code Structure:
                // using MassTransit;
                // using MassTransit.Testing;
                // using Microsoft.Extensions.DependencyInjection;
                // 
                // public class ProcessPaymentConsumerTests
                // {
                //     [Fact]
                //     public async Task Test() { ... }
                // }

    *   **Refining the OrderId Check:**
        To verify the OrderId, I need to access the published message.
        `var publishedContext = await harness.Published.GetContext<PaymentProcessed>();`
        If `Any` returns true, `GetContext` should succeed (or I can use `Select`).
        `var publishedMessage = await harness.Published.Select<PaymentProcessed>(m => m.Message).FirstOrDefaultAsync();`
        This is safer.

        Wait, `Select` returns `IEnumerable<T>`. `FirstOrDefaultAsync` is LINQ.
        MassTransit TestHarness `Published` property usually exposes `GetContext<T>()`.
        Let's check the reference provided in the prompt.
        Reference: `Assert.True(await harness.Consumed.Any<OrderPlaced>());`
        It doesn't show how to get the message content.
        However, standard MassTransit TestHarness usage for content verification involves `GetContext` or `Select`.
        I will use `GetContext` as it's the standard way to get the envelope/message in v8.
        Actually, `harness.Published.GetContext<PaymentProcessed>()` returns `IPublishContext<PaymentProcessed>`.
        So `var context = await harness.Published.GetContext<PaymentProcessed>();`
        `var msg = context.Message;`

        Wait, `GetContext` might throw if not found. Since I check `Any` first, it should be safe.
        Or I can just do:
        `var published = await harness.Published.GetContext<PaymentProcessed>();`
        `Assert.NotNull(published);`
        `Assert.Equal(orderId, published.Message.OrderId);`
        But the constraint says "Use `await harness.Consumed.Any<T>()` and `await harness.Published.Any<T>()`".
        So I must use `Any` for the boolean existence check.
        Then I need another check for OrderId.

        Plan:
        1. `bool consumed = await harness.Consumed.Any<ProcessPayment>();`
        2. `bool published = await harness.Published.Any<PaymentProcessed>();`
        3. `var publishedContext = await harness.Published.GetContext<PaymentProcessed>();` (Assuming this works to get the message)
        4. `Assert.Multiple(() => { Assert.True(consumed); Assert.True(published); Assert.Equal(orderId, publishedContext.Message.OrderId); });`

        Wait, `GetContext` might not be available on `IPublishTestHarness` directly in the way I recall.
        In MassTransit, `harness.Published` is `IPublishTestHarness`.
        Methods: `Any<T>()`, `GetContext<T>()`.
        Yes, `GetContext<T>()` is standard.

        Wait, one detail: `Assert.Multiple` does NOT support async lambdas.
        So I must await everything before calling `Assert.Multiple`.

        Also, `using` statements:
        `using MassTransit;`
        `using MassTransit.Testing;`
        `using Microsoft.Extensions.DependencyInjection;`
        No `using Xunit;`.

        Namespace: Global (no `namespace ... { }`).

        Consumer types: `ProcessPayment`, `PaymentProcessed`, `ProcessPaymentConsumer` are assumed to exist in the global namespace (or same assembly) based on "Do NOT redeclare the consumer types — they exist in a separate file."
        Since I am not using a namespace, they must be in the global namespace too for the test to compile without `using` directives for them.
        The prompt says "Do NOT include the consumer code... it is already provided in a separate file."
        It implies I can reference them directly.

        Wait, `ProcessPayment` and `PaymentProcessed` are `record`s.
        `ProcessPaymentConsumer` is `class`.

        One detail on `BuildServiceProvider(true)`:
        `services.BuildServiceProvider(true);`
        The `true` enables validation.

        One detail on `Assert.Multiple`:
        `Assert.Multiple` takes `Action`.
        So: