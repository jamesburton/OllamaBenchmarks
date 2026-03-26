using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Consume_PublishesPaymentProcessedEvent()
    {
        // Arrange
        var harness = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true)
            .GetRequiredService<ITestHarness>();

        await harness.Start();

        // Act
        await harness.Bus.Publish(new ProcessPayment(Guid.NewGuid(), 100m));

        // Assert
        var processedEvent = await harness.Published.Any<PaymentProcessed>();

        var consumedMessage = await harness.Consumed.Any<ProcessPayment>();

        Assert.Multiple(
            () =>
            {
                Assert.True(processedEvent, "Expected PaymentProcessed event to be published");
                Assert.Equal(consumedMessage.Message.OrderId, processedEvent.Message.OrderId);
            });
    }
}