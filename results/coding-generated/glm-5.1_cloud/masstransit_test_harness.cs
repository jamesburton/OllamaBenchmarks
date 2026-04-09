using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Should_Consume_ProcessPayment_And_Publish_PaymentProcessed()
    {
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100.50m;

        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment message was not consumed"),
            () => Assert.True(published, "PaymentProcessed event was not published")
        );

        // Also verify the OrderId matches
        var paymentProcessed = await harness.Published.SelectAsync<PaymentProcessed>().First();
        Assert.Equal(orderId, paymentProcessed.Context.Message.OrderId);
    }
}

var publishedMessage = await harness.Published.SelectAsync<PaymentProcessed>().First();

var consumed = await harness.Consumed.Any<ProcessPayment>();
var paymentProcessed = await harness.Published.SelectAsync<PaymentProcessed>().First();
var actualOrderId = paymentProcessed.Context.Message.OrderId;

Assert.Multiple(
    () => Assert.True(consumed, "ProcessPayment was not consumed"),
    () => Assert.Equal(orderId, actualOrderId)
);

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Should_Consume_ProcessPayment_And_Publish_PaymentProcessed_With_Correct_OrderId()
    {
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100.50m;

        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();
        var paymentProcessedMessage = await harness.Published.SelectAsync<PaymentProcessed>().First();
        var actualOrderId = paymentProcessedMessage.Context.Message.OrderId;

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment was not consumed"),
            () => Assert.Equal(orderId, actualOrderId)
        );
    }
}

Assert.Multiple(
    () => Assert.True(consumed, "ProcessPayment was not consumed"),
    () => Assert.Equal(orderId, actualOrderId)
);

Assert.Multiple(
    () => Assert.True(consumed, "ProcessPayment was not consumed"),
    () => Assert.True(published, "PaymentProcessed was not published"),
    () => Assert.Equal(orderId, actualOrderId)
);

await foreach (var msg in harness.Published.SelectAsync<PaymentProcessed>())
{
    // get first
    break;
}

var publishedMessages = harness.Published.SelectAsync<PaymentProcessed>();
PaymentProcessed? publishedMessage = null;
await foreach (var msg in publishedMessages)
{
    publishedMessage = msg.Context.Message;
    break;
}

var message = harness.Published.SelectAsync<PaymentProcessed>().FirstOrDefault();

await harness.Published.Any<PaymentProcessed>(); // wait for it
var publishedMessage = harness.Published.SelectAsync<PaymentProcessed>().FirstAsync();

PaymentProcessed publishedEvent = null!;
await foreach (var msg in harness.Published.SelectAsync<PaymentProcessed>())
{
    publishedEvent = msg.Context.Message;
    break;
}

var consumed = await harness.Consumed.Any<ProcessPayment>();
var published = await harness.Published.Any<PaymentProcessed>();

// Get the published message to check OrderId
PaymentProcessed? publishedEvent = null;
await foreach (var msg in harness.Published.SelectAsync<PaymentProcessed>())
{
    publishedEvent = msg.Context.Message;
    break;
}

Assert.Multiple(
    () => Assert.True(consumed, "ProcessPayment was not consumed"),
    () => Assert.Equal(orderId, publishedEvent!.OrderId)
);

await using var provider = new ServiceCollection()
    .AddMassTransitTestHarness(cfg =>
    {
        cfg.AddConsumer<ProcessPaymentConsumer>();
    })
    .BuildServiceProvider(true);

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Should_Consume_ProcessPayment_And_Publish_PaymentProcessed()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100.50m;

        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        PaymentProcessed? publishedEvent = null;
        await foreach (var msg in harness.Published.SelectAsync<PaymentProcessed>())
        {
            publishedEvent = msg.Context.Message;
            break;
        }

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment was not consumed"),
            () => Assert.Equal(orderId, publishedEvent!.OrderId)
        );
    }
}

Assert.Multiple(
    () => Assert.True(consumed, "ProcessPayment was not consumed"),
    () => Assert.True(published, "PaymentProcessed was not published"),
    () => Assert.Equal(orderId, publishedEvent!.OrderId)
);

Assert.Multiple(
    () => Assert.True(consumed, "ProcessPayment was not consumed"),
    () => Assert.Equal(orderId, publishedEvent!.OrderId)
);

Assert.Multiple(
    () => Assert.True(consumed, "ProcessPayment was not consumed"),
    () => Assert.NotNull(publishedEvent),
    () => Assert.Equal(orderId, publishedEvent!.OrderId)
);

Assert.Multiple(
    () => Assert.True(consumed),
    () => Assert.Equal(orderId, publishedEvent!.OrderId)
);

var consumed = await harness.Consumed.Any<ProcessPayment>();
await harness.Published.Any<PaymentProcessed>(); // ensure it was published

PaymentProcessed publishedEvent = default!;
await foreach (var msg in harness.Published.SelectAsync<PaymentProcessed>())
{
    publishedEvent = msg.Context.Message;
    break;
}

Assert.Multiple(
    () => Assert.True(consumed),
    () => Assert.Equal(orderId, publishedEvent.OrderId)
);