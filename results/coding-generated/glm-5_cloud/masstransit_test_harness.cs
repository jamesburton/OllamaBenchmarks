[Fact]
public async Task ProcessPayment_ShouldPublishPaymentProcessed()
{
    // Arrange
    var provider = new ServiceCollection()
        .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
        .BuildServiceProvider(true);

    var harness = provider.GetRequiredService<ITestHarness>();
    await harness.Start();

    var orderId = Guid.NewGuid();
    var amount = 100m;

    // Act
    await harness.Bus.Publish(new ProcessPayment(orderId, amount));

    // Assert
    // harness.Consumed.Any returns Task<bool>
    var consumed = await harness.Consumed.Any<ProcessPayment>();
    // harness.Published.Any returns Task<bool>
    var published = await harness.Published.Any<PaymentProcessed>();

    Assert.Multiple(() =>
    {
        Assert.True(consumed);
        Assert.True(published);
    });

    // Wait, I need to verify the content of the published message too?
    // Prompt: "A PaymentProcessed event is published with the correct OrderId"
    // Just checking Any() isn't enough for "correct OrderId".
    // I should probably check the message context.

    // Let's look at MassTransit Test Harness API.
    // harness.Published.Select() or similar?
    // The prompt specifically says: "Use `await harness.Consumed.Any()` and `await harness.Published.Any()` (these return `Task<bool>`)."
    // It implies I should use these methods. 
    // However, verifying "correct OrderId" usually requires inspecting the message.
    // If I strictly follow the prompt's instruction on methods to use, I might be limited to existence checks.
    // BUT, requirement #2 says: "A PaymentProcessed event is published with the correct OrderId".
    // If I only use `.Any()`, I can't verify the OrderId.
    // Let's check if I can use `.Select()` or similar to get the context.
    // The MassTransit reference doesn't show how to inspect the message content in the `Assert` block, only `Any`.
    // However, standard MassTransit testing usually involves `harness.Published.Select(...).FirstOrDefault()` or checking the context.
    // Let's look at the constraints again.
    // "Use `await harness.Consumed.Any()` and `await harness.Published.Any()` (these return `Task<bool>`)."
    // This is a constraint on *how* to check. It might be implying that checking existence is sufficient for this specific exercise, OR I need to find a way to check content.
    // If I look at the prompt's MassTransit reference:
    // `Assert.True(await harness.Consumed.Any<OrderPlaced>());`
    // It doesn't show content verification.
    // However, requirement #2 is explicit: "correct OrderId".
    // If I cannot use other methods, I cannot verify the OrderId.
    // Maybe the prompt implies I should use `Any` for the consumer check, and maybe something else for the published check?
    // Or maybe the prompt assumes `Any` is the primary check and I should add the content check if possible?
    // Let's look at the constraint: "Use `await harness.Consumed.Any()` and `await harness.Published.Any()`...". It doesn't say "ONLY use".
    // But it does say "these return Task<bool>".
    // Let's try to verify content using standard MassTransit testing patterns if allowed, but the prompt is very specific about the `Any` usage.
    // Actually, `harness.Published.Any<T>(predicate)` exists? No, usually it's `Select`.
    // Let's assume the prompt wants me to use `Any` to confirm the flow happened.
    // But "correct OrderId" is a strong requirement.
    // Let's look at the provided MassTransit reference again. It's very minimal.
    // If I write:
    /*
    var consumed = await harness.Consumed.Any<ProcessPayment>();
    var published = await harness.Published.Any<PaymentProcessed>();
    */
    // I satisfy the method usage constraint.
    // How do I verify OrderId?
    // Maybe `harness.Published.Select<PaymentProcessed>().First().Context.Message.OrderId`?
    // The prompt doesn't explicitly forbid other methods, but highlights `Any`.
    // Let's try to include the content check.
    // Wait, `harness.Published` is `IReceivedMessageList`. It has a `Select` method.
    // `harness.Published.Select<PaymentProcessed>(x => x.Context.Message.OrderId == orderId).Any()`?
    // Or just get the message.

    // Let's re-read the prompt constraints carefully.
    // "Use `await harness.Consumed.Any()` and `await harness.Published.Any()` (these return `Task<bool>`)."
    // This looks like a hint on how to handle the async nature for `Assert.Multiple`.
    // If I use `Select`, that returns an `IEnumerable`. `Select` itself isn't async.
    // `harness.Published.Select<PaymentProcessed>()` is synchronous access to the in-memory list.
    // So I can do:
    // `var messageExists = await harness.Published.Any<PaymentProcessed>();`
    // `var publishedMessage = harness.Published.Select<PaymentProcessed>().FirstOrDefault();`
    // Then inside `Assert.Multiple`:
    // `Assert.True(messageExists);`
    // `Assert.Equal(orderId, publishedMessage.Context.Message.OrderId);`

    // This seems like the most robust interpretation. It uses `Any` as requested for the async check, and then inspects the result.
    // However, `Assert.Multiple` logic:
    // The prompt says: "Await each harness check individually first... then pass sync assertions".
    // If I use `Select`, I don't need to await it.

    // Let's try to stick to the simplest interpretation that satisfies all constraints.
    // Constraint: Use `await harness...Any()`.
    // Requirement: Verify correct OrderId.

    // Implementation:
    /*
    var consumedAny = await harness.Consumed.Any<ProcessPayment>();
    // For published, we need to check content.
    // harness.Published.Any<PaymentProcessed>() checks existence.
    // To check content, we usually need the context.

    // Let's look at the specific constraint text again:
    // "Use `await harness.Consumed.Any()` and `await harness.Published.Any()` (these return `Task<bool>`)."
    // It might be forcing me to use these specific methods for the existence check.
    // If I strictly follow "Use... Any...", I might be penalized for using Select?
    // But if I don't use Select, I can't verify OrderId.
    // Let's assume the prompt wants me to use `Any` to get the boolean for the `Assert.Multiple` pattern demonstration, and I can add extra checks.

    // Actually, `Any` has an overload `Any<T>(Func<ConsumeContext<T>, bool> filter)`?
    // MassTransit's `IReceivedMessageList` (which `Consumed` and `Published` are) has `Any<T>()` and `Any<T>(predicate)`.
    // If I use `await harness.Published.Any<PaymentProcessed>(x => x.Message.OrderId == orderId)`, that returns `Task<bool>`.
    // This satisfies the method usage constraint AND the content verification constraint!
    // And it fits perfectly into the `Assert.Multiple` pattern described.
    */

    // Let's verify if `Any(predicate)` exists in MassTransit.Testing.
    // Yes, `IReceivedMessageList` has `Select` and `Any`.
    // `Any<T>(Expression<Func<ConsumeContext<T>, bool>> filter)`? Or similar.
    // Actually, usually it's `Select` then `Any` (LINQ).
    // Let's check MassTransit source/docs mentally.
    // `harness.Published` is `InMemoryTestHarnessMessageList`.
    // It has `Select<T>()`.
    // Does it have `Any<T>(predicate)`?
    // The MassTransit documentation usually shows: `harness.Published.Select<MyMessage>().Any()`.
    // Or `harness.Published.Any<MyMessage>()`.
    // If I use `harness.Published.Select<PaymentProcessed>().Any(x => x.Context.Message.OrderId == orderId)`, that is LINQ `Any`, not the harness `Any`.
    // The harness `Any` is `Task<bool>`. The LINQ `Any` is `bool`.
    // The prompt says: "Use `await harness.Consumed.Any()` and `await harness.Published.Any()` (these return `Task<bool>`)."
    // This implies using the async method on the harness list.
    // If I use the LINQ `Any` after `Select`, I am not using the harness `Any`.
    // If the harness `Any` doesn't support predicates (or if I'm unsure), I should stick to the harness `Any` for existence and maybe LINQ for content?

    // Let's look at the prompt again. "Verify: 1. Consumer receives... 2. PaymentProcessed published with correct OrderId".
    // If I only use `await harness.Published.Any<PaymentProcessed>()`, I verify existence, not content.
    // If I use `await harness.Published.Any<PaymentProcessed>(x => x.Message.OrderId == orderId)`, I verify both.
    // Does MassTransit's `Any` support predicates?
    // Checking MassTransit source (mental check): `InMemoryTestHarnessMessageList` implements `IReceivedMessageList`.
    // `IReceivedMessageList` has `Task<bool> Any<T>()`.
    // It does NOT seem to have `Any<T>(predicate)`.
    // So I cannot use `Any` with a predicate.

    // So I must use `Select`.
    // `var publishedMessages = harness.Published.Select<PaymentProcessed>();`
    // `var publishedExists = await harness.Published.Any<PaymentProcessed>();`
    // `var correctOrderId = publishedMessages.Any(x => x.Context.Message.OrderId == orderId);`

    // Wait, `Assert.Multiple` needs sync assertions.
    // `Assert.True(publishedExists);`
    // `Assert.True(correctOrderId);`

    // This seems the safest path.
    // 1. `var consumed = await harness.Consumed.Any<ProcessPayment>();`
    // 2. `var published = await harness.Published.Any<PaymentProcessed>();`
    // 3. `var publishedMessage = harness.Published.Select<PaymentProcessed>().FirstOrDefault();`
    // 4. `Assert.Multiple(() => { Assert.True(consumed); Assert.True(published); Assert.Equal(orderId, publishedMessage?.Context.Message.OrderId); });`

    // Wait, if `published` is false, `publishedMessage` is null. `Assert.Equal` would fail comparing Guid to null.
    // That's fine.

    // Let's refine the code.
    // The prompt asks to verify "A PaymentProcessed event is published with the correct OrderId".
    // I will use `Select` to get the message and verify the ID.
    // I will use `Any` as requested by the constraints.

    // Constraint check: "Use `await harness.Consumed.Any()` and `await harness.Published.Any()` (these return `Task<bool>`)."
    // I will use them.

    // Constraint check: "Assert.Multiple does NOT support async lambdas."
    // I will await the `Any` calls outside.

    // Constraint check: "Do NOT use a namespace".
    // Constraint check: "Do NOT use `using Xunit;`".
    // Constraint check: "Do NOT redeclare consumer types".

    // Code construction:

    /*
    using MassTransit;
    using MassTransit.Testing;
    using Microsoft.Extensions.DependencyInjection;

    public class ProcessPaymentConsumerTests
    {
        [Fact]
        public async Task Should_Process_Payment_And_Publish_Event()
        {
            var provider = new ServiceCollection()
                .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var orderId = Guid.NewGuid();
            var amount = 100.50m;

            await harness.Bus.Publish(new ProcessPayment(orderId, amount));

            // Await harness checks
            var consumed = await harness.Consumed.Any<ProcessPayment>();
            var published = await harness.Published.Any<PaymentProcessed>();

            // Get the message for content verification
            // Note: harness.Published.Select is synchronous
            var publishedContext = harness.Published.Select<PaymentProcessed>().FirstOrDefault();

            Assert.Multiple(() =>
            {
                Assert.True(consumed);
                Assert.True(published);
                // Verify content
                Assert.NotNull(publishedContext);
                Assert.Equal(orderId, publishedContext.Context.Message.OrderId);
            });
        }
    }
    */

    // Wait, `Assert.Multiple` executes all actions.
    // If `published` is false, `publishedContext` is null. `Assert.NotNull` fails. `Assert.Equal` fails.
    // This is acceptable behavior for `Assert.Multiple`.

    // One detail: `ProcessPaymentConsumer` generates a random TransactionId.
    // I don't need to check that, just the OrderId.

    // Is there any other way to verify OrderId using `Any`?
    // No, `Any` on the harness list doesn't take a predicate.

    // Let's double check the `using` statements.
    // `using MassTransit;` -> for `IConsumer`, `ConsumeContext`, `Bus`.
    // `using MassTransit.Testing;` -> for `ITestHarness`, `AddMassTransitTestHarness`.
    // `using Microsoft.Extensions.DependencyInjection;` -> for `ServiceCollection`, `GetRequiredService`.
    // Implicit usings for xUnit v3 are assumed.

    // The prompt says: "Return only valid C# code in a single file."
    // I will output exactly that.

    // One final check on the `Assert.Multiple` logic.
    // The prompt says: "Await each harness check individually first, store results in local variables, then pass sync assertions to Assert.Multiple."
    // My code does exactly that.
    // `var consumed = await ...`
    // `var published = await ...`
    // `Assert.Multiple(() => { ... })`

    // The prompt says: "Use `await harness.Consumed.Any()` and `await harness.Published.Any()`".
    // My code does exactly that.

    // The prompt says: "Verify... correct OrderId".
    // My code uses `Select` to get the message and `Assert.Equal` to check the ID.
    // This seems the only logical way to satisfy all requirements.

    // Wait, `Assert.Multiple` takes `Action[]`.
    // `Assert.Multiple(() => ..., () => ...)`
    // My syntax `Assert.Multiple(() => { ... })` puts multiple asserts in one action.
    // xUnit v3 `Assert.Multiple` executes the actions.
    // If I put multiple asserts inside one lambda, and the first one fails, the second one in the same lambda won't run?
    // No, `Assert.Multiple` is designed to run all *actions* provided to it.
    // If I provide one action containing multiple asserts, it treats it as one unit.
    // If I want ALL failures to be collected, I should pass separate lambdas to `Assert.Multiple`.
    // Example:
    /*
    Assert.Multiple(
        () => Assert.True(consumed),
        () => Assert.True(published),
        () => Assert.Equal(orderId, publishedContext?.Context.Message.OrderId)
    );
    */
    // This is the correct usage of `Assert.Multiple` to collect all failures.
    // If I wrap them in one lambda `() => { Assert.True(...); Assert.True(...); }`, if the first fails, the second is skipped inside that block.
    // The reference snippet in the prompt shows:
    /*
    Assert.Multiple(
        () => Assert.Equal(1, a),
        () => Assert.Equal(2, b)
    );
    */
    // So I should follow this pattern.

    // Revised Assert block:
    /*
    var publishedContext = harness.Published.Select<PaymentProcessed>().