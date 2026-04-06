using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Consume_ProcessPayment_PublishesPaymentProcessed()
    {
        // Arrange
        var harness = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<ProcessPaymentConsumer>(); })
            .BuildServiceProvider(true);

        var orderId = Guid.NewGuid();
        var amount = 100.50m;

        // Act
        await harness.Start();
        await harness.Bus.Consume<ProcessPayment>(context =>
            context.Message = new ProcessPayment(orderId, amount));

        // Assert
        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed, "Consumer should have received ProcessPayment message"),
            () => Assert.True(published, "PaymentProcessed event should have been published")
        );

        if (published)
        {
            var paymentProcessed = await harness.Published.GetLast<PaymentProcessed>();
            Assert.Equal(orderId, paymentProcessed.OrderId);
            Assert.NotNull(paymentProcessed.TransactionId);
        }
    }
}