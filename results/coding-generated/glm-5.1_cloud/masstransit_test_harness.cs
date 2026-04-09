using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPayment_ShouldBeConsumedAndPublishPaymentProcessed()
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
        var amount = 99.50m;

        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        var wasConsumed = await harness.Consumed.Any<ProcessPayment>();
        var wasPublished = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(wasConsumed, "ProcessPayment message was not consumed"),
            () => Assert.True(wasPublished, "PaymentProcessed event was not published")
        );

        var consumedMessage = harness.Consumed.Select<ProcessPayment>().First();
        var publishedMessage = harness.Published.Select<PaymentProcessed>().First();

        Assert.Multiple(
            () => Assert.Equal(orderId, consumedMessage.Context.Message.OrderId),
            () => Assert.Equal(amount, consumedMessage.Context.Message.Amount),
            () => Assert.Equal(orderId, publishedMessage.Context.Message.OrderId),
            () => Assert.NotNull(publishedMessage.Context.Message.TransactionId)
        );

        await harness.Stop();
    }
}