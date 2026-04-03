using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Consume_ProcessPayment_PublishesPaymentProcessedEvent()
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

        // Wait for the consumer to process the message
        await Task.Delay(100); 

        // Assertions setup
        // 1. Check if the input message was consumed
        var consumed = await harness.Consumed.Any<ProcessPayment>();

        // 2. Check if the output message was published
        var published = await harness.Published.Any<PaymentProcessed>();

        // 3. Check the content of the published message
        // We must retrieve the actual published messages to check the content.
        var publishedMessages = await harness.Published.OfType<PaymentProcessed>().ToListAsync();
        var publishedOrderIdMatches = publishedMessages.Any(p => p.OrderId == testOrderId);

        // Use Assert.Multiple to check all conditions
        Assert.Multiple(
            () => Assert.True(consumed, "The consumer should have received the ProcessPayment message."),
            () => Assert.True(published, "A PaymentProcessed event should have been published."),
            () => Assert.True(publishedOrderIdMatches, "The published PaymentProcessed event must contain the original OrderId.")
        );
    }
}