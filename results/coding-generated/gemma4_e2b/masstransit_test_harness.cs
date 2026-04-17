using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ShouldPublishPaymentProcessedEvent()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });

        var provider = serviceCollection.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();

        var orderId = Guid.NewGuid();
        var amount = 100.50m;
        var paymentMessage = new ProcessPayment(orderId, amount);

        // Act
        await harness.Start();
        await harness.Bus.Publish(paymentMessage);

        // Assert
        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        // 1. Verify the message was consumed
        Assert.True(consumed, $"Expected to consume ProcessPayment message, but found none.");

        // 2. Verify the PaymentProcessed event was published
        Assert.True(published, $"Expected to publish PaymentProcessed event, but found none.");

        // 3. Verify the published event contains the correct OrderId
        var paymentProcessedEvents = await harness.Consumed.OfType<PaymentProcessed>().ToList();

        Assert.Single(paymentProcessedEvents, $"Expected exactly one PaymentProcessed event, but found {paymentProcessedEvents.Count}.");

        var processedEvent = paymentProcessedEvents.First();
        Assert.Equal(orderId, processedEvent.OrderId);
    }
}