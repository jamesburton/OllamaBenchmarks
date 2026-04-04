using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ProcessesMessage_AndPublishesEvent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var processPayment = new ProcessPayment(orderId, 100.0m);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        // Act
        await harness.Start();
        await harness.Bus.Publish(processPayment);

        // Assert - await first, then check in Assert.Multiple
        var processPaymentConsumed = await harness.Consumed.Any<ProcessPayment>();
        var paymentProcessedPublished = await harness.Published.Any<PaymentProcessed>();

        var publishedContext = harness.Published
            .Select<PaymentProcessed>()
            .FirstOrDefault();

        Assert.Multiple(
            () => Assert.True(processPaymentConsumed, "ProcessPayment should be consumed"),
            () => Assert.True(paymentProcessedPublished, "PaymentProcessed should be published"),
            () => Assert.NotNull(publishedContext),
            () => Assert.Equal(orderId, publishedContext!.Message.OrderId)
        );
    }
}