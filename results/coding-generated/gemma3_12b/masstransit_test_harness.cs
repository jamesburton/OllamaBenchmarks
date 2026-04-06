using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPayment_PublishesPaymentProcessed()
    {
        var harness = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new ProcessPayment(orderId, 100.00m));

        bool consumed = await harness.Consumed.Any<ProcessPayment>();
        bool published = await harness.Published.Any<PaymentProcessed>();

        Assert.True(consumed);
        Assert.True(published);

        var publishedEvents = await harness.Published.GetPayloads<PaymentProcessed>();
        Assert.Single(publishedEvents);

        Assert.Multiple(
            () => Assert.Equal(orderId, publishedEvents[0].OrderId),
            () => Assert.True(Guid.TryParse(publishedEvents[0].TransactionId, out _))
        );

        await harness.Stop();
    }
}