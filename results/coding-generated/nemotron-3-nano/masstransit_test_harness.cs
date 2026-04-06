using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public void ProcessPaymentConsumer_HandlesMessage_AndPublishesPaymentProcessed()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });
        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();

        // Act
        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100.00m;
        var message = new ProcessPayment(orderId, amount);
        await harness.Bus.Publish(message);

        bool received = await harness.Consumed.Any<ProcessPayment>();
        bool published = await harness.Published.Any<PaymentProcessed>();

        // Assert
        Assert.Multiple(
            () => Assert.True(received),
            () => Assert.True(published)
        );
    }
}