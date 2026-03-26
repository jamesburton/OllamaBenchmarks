using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public async Task ProcessPayment_consumer_publishes_correct_event()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });
        var provider = serviceCollection.BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        // Act
        var orderId = Guid.NewGuid();
        var message = new ProcessPayment(orderId, 100.0m);
        await harness.Bus.Publish(message);

        // Assert
        bool consumed = await harness.Consumed.Any<ProcessPayment>();
        bool published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(() =>
        {
            Assert.True(consumed, "ProcessPayment message was not consumed.");
            Assert.True(published, "PaymentProcessed event was not published.");
        });
    }
}