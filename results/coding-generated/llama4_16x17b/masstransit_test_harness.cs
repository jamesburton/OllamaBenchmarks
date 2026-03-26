using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Should_Consume_ProcessPayment_And_Publish_PaymentProcessed()
    {
        // Arrange
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        await using var harness = provider.GetService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(new ProcessPayment(Guid.NewGuid(), 10.99m));

        // Assert
        var paymentProcessed = await harness.Consumed.Any<ProcessPayment>();
        var publishedPaymentProcessed = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(paymentProcessed),
            () => Assert.True(publishedPaymentProcessed)
        );
    }
}