using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public async Task ProcessPayment_ShouldConsumeMessageAndPublishEvent()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });

        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var message = new ProcessPayment(orderId, 100.00m);

        // Act
        await harness.Bus.Publish(message);

        // Assert
        bool wasConsumed = await harness.Consumed.Any<ProcessPayment>();

        // We need to find the specific published message to check its content
        var publishedMessages = harness.Published.Select<PaymentProcessed>().ToList();
        bool wasPublishedWithCorrectId = publishedMessages.Any(m => m.OrderId == orderId);

        Assert.Multiple(
            () => Assert.True(wasConsumed, "The ProcessPayment message should have been consumed."),
            () => Assert.True(wasPublishedWithCorrectId, "A PaymentProcessed event with the correct OrderId should have been published.")
        );
    }
}