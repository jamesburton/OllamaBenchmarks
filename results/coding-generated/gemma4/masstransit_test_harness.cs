using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System;
using System.Threading.Tasks;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ReceivesMessageAndPublishesPaymentProcessed()
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
        var paymentMessage = new ProcessPayment(testOrderId, 100.50m);

        // Act
        await harness.Start();
        await harness.Bus.Publish(paymentMessage);

        // Assert
        // 1. Check if the consumer received the message
        var receivedMessage = await harness.Consumed.Any<ProcessPayment>();

        // 2. Check if the expected event was published
        var publishedEvent = await harness.Published.Any<PaymentProcessed>();

        // Use Assert.Multiple to check both assertions in a single test
        Assert.Multiple(
            () => Assert.True(receivedMessage, "The consumer should have received the ProcessPayment message."),
            () => Assert.True(publishedEvent, "A PaymentProcessed event should have been published.")
        );

        // Optional: Verify the content of the published message if needed, 
        // but the prompt only requires checking existence.
        // If we needed to check content, we would use:
        // var published = await harness.Published.Get<PaymentProcessed>();
        // Assert.NotNull(published);
        // Assert.Equal(testOrderId, published.OrderId);
    }
}