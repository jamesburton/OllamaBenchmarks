using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    // Test method
}

var provider = new ServiceCollection()
    .AddMassTransitTestHarness(cfg =>
    {
        cfg.AddConsumer<ProcessPaymentConsumer>();
    })
    .BuildServiceProvider(true);

var harness = provider.GetRequiredService<ITestHarness>();

await harness.Start();

var orderId = Guid.NewGuid();
var amount = 100.50m; // Need an amount for the record constructor

await harness.Bus.Publish(new ProcessPayment(orderId, amount));

bool consumed = await harness.Consumed.Any<ProcessPayment>();
bool published = await harness.Published.Any<PaymentProcessed>();

Assert.Multiple(() =>
{
    Assert.True(consumed);
    // How to check the OrderId?
    // The prompt says: "A PaymentProcessed event is published with the correct OrderId".
    // `harness.Published.Any<T>()` just checks existence.
    // To check properties, usually one iterates `harness.Published.Select(...)` or uses `.Any<T>(predicate)`.
    // The prompt specifically says: "Use `await harness.Consumed.Any()...`".
    // Let's look at the prompt constraints again.
    // "Use `await harness.Consumed.Any()...` (these return `Task<bool>`)."
    // It doesn't explicitly forbid checking the context inside the message, but `Any` returns bool.
    // If I need to check the OrderId, I probably need to look at the consumed/published messages.
    // However, the prompt instructions regarding `Assert.Multiple` are specific: "Await each harness check individually first... then pass sync assertions".
    // If I use `Any`, I just get a boolean.
    // If I need to verify the OrderId, I might need to use `harness.Published.Select(...)` or similar.
    // BUT, the prompt says: "Use `await harness.Consumed.Any()...`".
    // Let's re-read carefully: "Use `await harness.Consumed.Any()...` and `await harness.Published.Any()...` (these return `Task<bool>`)."
    // It implies I should use these methods.
    // Does `Any` support a filter? In MassTransit TestHarness, `Any` usually has an overload `Any(Func<...> filter)`.
    // If I use `Any`, I can check existence.
    // To check the OrderId, I might need to access the message context.
    // Let's look at the standard MassTransit testing pattern.
    // `harness.Published.Select(x => x.Context.Message).OfType<PaymentProcessed>()`...
    // Or `harness.Published.Any<PaymentProcessed>(x => x.Context.Message.OrderId == orderId)`.

    // The prompt says: "Use `await harness.Consumed.Any()...`". It doesn't explicitly say "don't use overloads".
    // However, the constraint "Await each harness check individually first" suggests I should get the result of the check into a variable.
    // If I use `Any` with a predicate, it returns `Task<bool>`. That fits the pattern.
    // Let's try to verify the OrderId using the predicate inside `Any`.
});

// Await checks
var consumed = await harness.Consumed.Any<ProcessPayment>();
// To check OrderId, I need the specific message.
// The prompt says "Use `await harness.Consumed.Any()...`". It doesn't explicitly forbid checking properties.
// But `Any` returns bool.
// If I want to check properties inside Assert.Multiple, I need the message object available in the sync scope.
// However, `harness.Published` is `IPublishedMessageList`. It doesn't expose the message directly without async iteration usually?
// Actually, `harness.Published.Select(...)` is synchronous on the list of messages already captured.
// Let's check MassTransit TestHarness API.
// `harness.Published` is `IMessageList`. It has `Select`.
// So I can do:
// var publishedMessage = harness.Published.Select(x => x.Context.Message).OfType<PaymentProcessed>().FirstOrDefault();
// This is synchronous.

// Let's stick to the prompt's explicit instruction: "Use `await harness.Consumed.Any()...`".
// If I strictly follow "Use `await harness.Consumed.Any()...`", I get a boolean.
// If I need to check the OrderId, I have two options:
// 1. Use `Any` with a predicate: `await harness.Published.Any<PaymentProcessed>(x => x.Context.Message.OrderId == orderId)`.
// 2. Use `Any` for existence, then inspect the collection synchronously.

// The prompt says: "Await each harness check individually first... then pass sync assertions".
// If I use `Any` with a predicate, that counts as a "harness check".
// So:
// bool consumedExists = await harness.Consumed.Any<ProcessPayment>();
// bool publishedCorrectId = await harness.Published.Any<PaymentProcessed>(x => x.Message.OrderId == orderId); // Wait, context access?
// In MassTransit v8, `ConsumeContext` is usually accessed via `x.Context`.
// So: `x.Context.Message.OrderId`.

// Let's try to combine them.
// Constraint: "Use `await harness.Consumed.Any()...` and `await harness.Published.Any()...`".
// This suggests using these specific methods.

// Let's write the test.

[Fact]
public async Task ProcessPayment_ShouldConsumeAndPublishEvent()
{
    // Arrange
    var provider = new ServiceCollection()
        .AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        })
        .BuildServiceProvider(true);

    var harness = provider.GetRequiredService<ITestHarness>();

    await harness.Start();

    var orderId = Guid.NewGuid();
    var amount = 100m;

    // Act
    await harness.Bus.Publish(new ProcessPayment(orderId, amount));

    // Assert
    // Constraint: Await checks individually first.
    // Constraint: Assert.Multiple does not support async lambdas.

    // Check 1: Consumer received message
    var wasConsumed = await harness.Consumed.Any<ProcessPayment>();

    // Check 2: Published event with correct OrderId
    // Using the predicate overload of Any to verify the OrderId
    var wasPublishedCorrectly = await harness.Published.Any<PaymentProcessed>(x => x.Context.Message.OrderId == orderId);

    Assert.Multiple(() =>
    {
        Assert.True(wasConsumed);
        Assert.True(wasPublishedCorrectly);
    });
}

var consumed = await harness.Consumed.Any<ProcessPayment>();
// I need to verify the OrderId.
// If I just check existence of PaymentProcessed, I haven't checked the OrderId.
// If I check `harness.Published.Any<PaymentProcessed>(x => x.Context.Message.OrderId == orderId)`, I check both existence and ID.

var consumed = await harness.Consumed.Any<ProcessPayment>();
var published = await harness.Published.Any<PaymentProcessed>();

Assert.Multiple(() =>
{
    Assert.True(consumed);
    Assert.True(published);

    // Check OrderId
    // Need to find the message
    var message = harness.Published.Select<PaymentProcessed>().FirstOrDefault(); // or Single
    Assert.NotNull(message);
    Assert.Equal(orderId, message.Context.Message.OrderId);
});

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPayment_ConsumesMessageAndPublishesEvent()
    {
        // Arrange
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100m;

        // Act
        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        // Assert
        // Await harness checks individually
        var wasConsumed = await harness.Consumed.Any<ProcessPayment>();
        var wasPublished = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(() =>
        {
            Assert.True(wasConsumed);
            Assert.True(wasPublished);

            // Verify OrderId in the published message
            // harness.Published is synchronous access to the captured messages
            var publishedMessage = harness.Published.Select(x => x.Context.Message).OfType<PaymentProcessed>().FirstOrDefault();
            // Or simpler:
            // var publishedMessage = harness.Published.Select<PaymentProcessed>().FirstOrDefault()?.Context.Message;

            // Let's use the cleaner syntax provided by MassTransit.Testing
            // harness.Published.Select<PaymentProcessed>() returns IEnumerable<IPublishedMessage<PaymentProcessed>>
            var publishedContext = harness.Published.Select<PaymentProcessed>().FirstOrDefault();

            Assert.NotNull(publishedContext);
            Assert.Equal(orderId, publishedContext.Context.Message.OrderId);
        });
    }
}

Assert.Multiple(() =>
{
    Assert.True(wasConsumed);
    Assert.True(wasPublished);
},
() => 
{
    // Separate lambda or just code block?
    // Assert.Multiple params are params Action[]
    // So I can pass multiple lambdas.

    // But I need to access the message.
    // If I separate them:
});

Assert.Multiple(
    () => Assert.True(wasConsumed),
    () => Assert.True(wasPublished),
    () => 
    {
        var msg = harness.Published.Select<PaymentProcessed>().FirstOrDefault();
        Assert.NotNull(msg);
        Assert.Equal(orderId, msg.Context.Message.OrderId);
    }
);