using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Consume_ProcessPayment_MessageIsHandledAndEventPublished()
    {
        var harness = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100.00m;

        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        var consumedChecked = await harness.Consumed.Any<ProcessPayment>();
        var publishedChecked = await harness.Published.Any<PaymentProcessed>();

        Assert.True(consumedChecked, "Consumer did not receive the ProcessPayment message.");
        Assert.True(publishedChecked, "No PaymentProcessed event was published.");

        var consumedResult = await harness.Consumed.Single<ProcessPayment>(x => x.Message.OrderId == orderId);
        var publishedResult = await harness.Published.Single<PaymentProcessed>(x => x.Message.OrderId == orderId);

        Assert.Equal(orderId, consumedResult.Message.OrderId, "OrderId in consumed message does not match.");
        Assert.NotNull(publishedResult.Message.TransactionId, "TransactionId should not be null.");

        Assert.Multiple(
            () => Assert.Equal(orderId, consumedResult.Message.OrderId),
            () => Assert.NotNull(publishedResult.Message.TransactionId)
        );
    }
}