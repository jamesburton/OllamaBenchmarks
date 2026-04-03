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
        services.AddMassTransit(x =>
        {
            // Register the consumer we are testing
            x.AddConsumer<ProcessPaymentConsumer>();
        });

        // Build the service provider and the test harness
        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();

        // Act
        await harness.Start();

        var testOrderId = Guid.NewGuid();
        var testAmount = 100.50m;
        var message = new ProcessPayment(testOrderId, testAmount);

        await harness.Bus.Publish(message);

        // Assert
        // 1. Verify that the consumer received the input message (implicitly checked by the harness setup)
        // 2. Verify that the consumer published the output event

        var consumedResult = await harness.Consumed.Any<PaymentProcessed>();
        var publishedResult = await harness.Published.Any<PaymentProcessed>();

        // Use Assert.Multiple to check both results
        Assert.Multiple(
            () =>
            {
                // Check 1: Ensure the PaymentProcessed event was published
                Assert.True(publishedResult, "PaymentProcessed event should have been published.");

                // Check 2: Ensure the published event contains the correct OrderId
                var publishedEvent = await harness.Published.FirstAsync<PaymentProcessed>();
                Assert.Equal(testOrderId, publishedEvent.OrderId);
            });
    }
}