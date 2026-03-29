using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ProcessPaymentConsumerTests;

public class ProcessPaymentConsumerTests
{
    public async Task RunTests()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().AddMassTransitTestHarness(cfg => { cfg.AddConsumer<ProcessPaymentConsumer>(); }).BuildServiceProvider(true);
        var harness = serviceProvider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new ProcessPayment(orderId, 100.00m));

        // Assert
        Assert.Multiple(() =>
        {
            // Assert that the consumer was called
            Assert.True(await harness.Consumed.Any<ProcessPayment>());

            // Assert that the PaymentProcessed was published with the correct OrderId
            Assert.True(await harness.Published.Any<PaymentProcessed>(p => p.OrderId == orderId));
        });
    }
}