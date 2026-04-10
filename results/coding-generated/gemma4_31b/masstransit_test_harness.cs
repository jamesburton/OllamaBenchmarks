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
        var message = new ProcessPayment(orderId, 99.99m);

        // Act
        await harness.Bus.Publish(message);

        // Assert
        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        // Retrieve the published message to verify the OrderId
        var publishedMessage = harness.Published.Select<PaymentProcessed>().FirstOrDefault();

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment message should be consumed"),
            () => Assert.True(published, "PaymentProcessed event should be published"),
            () => Assert.Equal(orderId, publishedMessage?.OrderId)
        );
    }
}