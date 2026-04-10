using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ShouldConsumeAndPublishPaymentProcessed()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = serviceProvider.GetRequiredService<ITestHarness>();
        var testOrderId = Guid.NewGuid();
        var inputMessage = new ProcessPayment(testOrderId, 100.00m);

        // Act
        await harness.Start();
        await harness.Bus.Publish(inputMessage);

        // 1. Check if the input message was consumed
        var consumed = await harness.Consumed.Any<ProcessPayment>();

        // 2. Check if the output message was published
        var published = await harness.Published.Any<PaymentProcessed>();

        // Assert
        // Since Assert.Multiple does not support async lambdas, we check the results synchronously.
        Assert.Multiple(
            () => Assert.True(consumed, "The consumer must have received the ProcessPayment message."),
            () => Assert.True(published, "The consumer must have published a PaymentProcessed event.")
        );
    }
}