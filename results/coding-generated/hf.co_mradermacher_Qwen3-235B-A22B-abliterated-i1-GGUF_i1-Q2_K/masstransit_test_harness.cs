using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Should_Process_Payment_And_Publish_Processed_Event()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });
        var serviceProvider = serviceCollection.BuildServiceProvider(true);
        var harness = serviceProvider.GetRequiredService<ITestHarness>();

        // Act
        await harness.Start();
        await harness.Bus.Publish(new ProcessPayment(orderId, 100.0m));

        // Assert
        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var paymentProcessedPublished = await harness.Published.Any<PaymentProcessed>(x => x.Message.OrderId == orderId);

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(paymentProcessedPublished)
        );
    }
}