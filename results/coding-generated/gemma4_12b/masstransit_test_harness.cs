using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public async Task ProcessPayment_ShouldBeConsumedAndPublishPaymentProcessed()
    {
        // Arrange
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        ITestHarness harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var message = new ProcessPayment(orderId, 100.0m);

        // Act
        await harness.Bus.Publish(message);

        // Assert
        // Since Assert.Multiple does not support async lambdas, we await the results first.
        bool wasConsumed = await harness.Consumed.Any<ProcessPayment>();
        bool wasPublished = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(wasConsumed, "The ProcessPayment message should have been consumed by the consumer."),
            () => Assert.True(wasPublished, "The PaymentProcessed event should have been published.")
        );
    }
}