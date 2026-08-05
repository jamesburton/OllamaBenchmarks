using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ShouldPublishPaymentProcessedEvent()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Build the test harness configuration
        serviceCollection.AddMassTransit(cfg =>
        {
            // Register the consumer we are testing
            cfg.AddConsumer<ProcessPaymentConsumer>();

            // Use the in-memory bus for testing
            cfg.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
        });

        // Build the service provider and the test harness
        var provider = serviceCollection.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();

        // Act
        await harness.Start();

        var testOrderId = Guid.NewGuid();
        var paymentMessage = new ProcessPayment(testOrderId, 100.50m);

        // Publish the input message
        await harness.Bus.Publish(paymentMessage);

        // Assert
        // 1. Verify that the consumer successfully consumed the input message
        var consumedResult = await harness.Consumed.Any<ProcessPayment>();

        // 2. Verify that the consumer published the expected output event
        var publishedResult = await harness.Published.Any<PaymentProcessed>();

        // Use Assert.Multiple to check both results
        Assert.Multiple(
            () =>
            {
                // Check if the input message was consumed
                Assert.True(consumedResult, "The ProcessPayment message should have been consumed.");

                // Check if the output event was published
                Assert.True(publishedResult, "The PaymentProcessed event should have been published.");
            });

        // Optional: Further verification of content (requires inspecting the published messages)
        // Since we only checked existence above, we can optionally check the content if needed, 
        // but the prompt only required checking the flow and existence.
    }
}