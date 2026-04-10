using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ShouldPublishPaymentProcessedEvent()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });

        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();

        var orderId = Guid.NewGuid();
        var amount = 100.50m;
        var paymentMessage = new ProcessPayment(orderId, amount);

        // Act
        await harness.Start();
        await harness.Bus.Publish(paymentMessage);

        // Assert
        var consumed = await harness.Consumed.Any<PaymentProcessed>();
        var published = await harness.Published.Any<PaymentProcessed>();

        // Check if the event was consumed
        var consumedResult = await consumed;
        Assert.True(consumedResult, "PaymentProcessed event should have been consumed.");

        // Check if the event was published
        var publishedResult = await published;
        Assert.True(publishedResult, "PaymentProcessed event should have been published.");

        // Verify the content of the published event
        var publishedEvent = await published.First();
        Assert.Equal(orderId, publishedEvent.OrderId);

        // Since the consumer generates a new Guid for TransactionId, we only check the OrderId match.
        // We don't assert the exact TransactionId as it's generated inside the consumer.

        // Use Assert.Multiple to group the checks
        Assert.Multiple(
            () =>
            {
                // 1. Check consumption
                Assert.True(consumedResult, "The PaymentProcessed event must be consumed.");

                // 2. Check publication
                Assert.True(publishedResult, "The PaymentProcessed event must be published.");
            });
    }
}