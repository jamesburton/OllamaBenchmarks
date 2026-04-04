using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class ConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_HandlesMessageAndPublishesProcessedEvent()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = serviceProvider.GetRequiredService<ITestHarness>();
        var testOrderId = Guid.NewGuid();
        var inputMessage = new ProcessPayment(testOrderId, 100.00m);

        // Act
        await harness.Start();
        await harness.Bus.Publish(inputMessage);

        // Assert
        // 1. Check if the message was consumed
        var consumed = await harness.Consumed.Any<ProcessPayment>();

        // 2. Check if the expected event was published
        var published = await harness.Published.Any<PaymentProcessed>();

        // Use Assert.Multiple to check both conditions
        Assert.Multiple(
            () => Assert.True(consumed, "The consumer should have received and processed the ProcessPayment message."),
            () => Assert.True(published, "A PaymentProcessed event should have been published.")
        );

        // Optional: Verify the content of the published message
        var publishedMessages = await harness.Published.OfType<PaymentProcessed>().ToListAsync();
        Assert.Single(publishedMessages);
        Assert.Equal(testOrderId, publishedMessages.First().OrderId);
    }
}