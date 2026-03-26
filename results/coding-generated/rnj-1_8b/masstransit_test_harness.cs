using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Should_process_payment_and_publish_event()
    {
        var harness = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true)
            .GetService<ITestHarness>();

        await harness.Start();

        var orderId = System.Guid.NewGuid();
        var amount = 100.50m;

        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        var paymentProcessedPublished = await harness.Published.Any<PaymentProcessed>();
        var paymentProcessed = await harness.Published.OfType<PaymentProcessed>().FirstOrDefaultAsync();

        Assert.Multiple(() =>
        {
            Assert.True(paymentProcessedPublished, "PaymentProcessed event was not published");
            Assert.NotNull(paymentProcessed, "PaymentProcessed event was null");
            Assert.Equal(orderId, paymentProcessed.OrderId, "Order ID mismatch");
            Assert.NotNull(paymentProcessed.TransactionId, "Transaction ID is null");
        });
    }
}