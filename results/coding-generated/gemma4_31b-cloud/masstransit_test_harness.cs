using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Should_ProcessPayment_And_Publish_PaymentProcessed()
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
        bool consumed = await harness.Consumed.Any<ProcessPayment>();
        bool published = await harness.Published.Any<PaymentProcessed>();

        var publishedMessage = harness.Published.SelectFirst(typeof(PaymentProcessed)).Message as PaymentProcessed;

        Assert.Multiple(
            () => Assert.True(consumed, "The ProcessPayment message should be consumed."),
            () => Assert.True(published, "A PaymentProcessed event should be published."),
            () => Assert.Equal(orderId, publishedMessage?.OrderId)
        );
    }
}