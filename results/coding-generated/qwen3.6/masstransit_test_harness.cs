using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Consume_ProcessPayment_ShouldPublishPaymentProcessed()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });
        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100.0m;
        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        var paymentProcessedPublished = await harness.Published.Any<PaymentProcessed>();
        var processPaymentConsumed = await harness.Consumed.Any<ProcessPayment>();

        Assert.Multiple(
            () => Assert.True(paymentProcessedPublished),
            () => Assert.True(processPaymentConsumed)
        );

        var publishedEvent = await harness.Published.Select<PaymentProcessed>().FirstAsync();
        Assert.Equal(orderId, publishedEvent.Message.OrderId);
    }
}