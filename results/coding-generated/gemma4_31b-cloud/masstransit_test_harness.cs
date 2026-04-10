using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Consume_ProcessPayment_PublishesPaymentProcessed()
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

        var publishedMessage = harness.Published.Select<PaymentProcessed>().FirstOrDefault();

        Assert.Multiple(
            () => Assert.True(consumed, "The ProcessPayment message should have been consumed."),
            () => Assert.True(published, "A PaymentProcessed event should have been published."),
            () => Assert.Equal(orderId, publishedMessage?.OrderId)
        );
    }
}