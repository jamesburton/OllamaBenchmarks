using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPayment_Should_Process_Payment_And_Publish_PaymentProcessed_Event()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<ProcessPaymentConsumer>(); })
            .BuildServiceProvider(true);

        var harness = serviceProvider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(new ProcessPayment(Guid.NewGuid(), 100));

        // Assert
        var paymentProcessedConsumed = await harness.Consumed.Any<PaymentProcessed>();
        var paymentProcessedPublished = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(paymentProcessedConsumed),
            () => Assert.True(paymentProcessedPublished)
        );
    }
}