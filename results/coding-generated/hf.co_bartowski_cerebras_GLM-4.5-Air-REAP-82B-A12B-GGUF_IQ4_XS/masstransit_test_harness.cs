using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ShouldConsumeProcessPaymentAndPublishPaymentProcessed()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg => 
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });
        using var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new ProcessPayment(orderId, 100.0m));

        // Assert
        bool received = await harness.Consumed.Any<ProcessPayment>();
        bool published = await harness.Published.Any<PaymentProcessed>();

        var processedMessage = harness.Published
            .Select(m => m.Message)
            .OfType<PaymentProcessed>()
            .FirstOrDefault();

        Assert.Multiple(
            () => 
            {
                Assert.True(received, "The ProcessPayment message was not consumed");
            },
            () => 
            {
                Assert.True(published, "No PaymentProcessed event was published");
                Assert.NotNull(processedMessage);
                Assert.Equal(orderId, processedMessage.OrderId);
            }
        );
    }
}