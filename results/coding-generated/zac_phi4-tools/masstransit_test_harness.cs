using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Consumer_Processes_Payment_And_Publishes_Event()
    {
        var orderId = Guid.NewGuid();
        var amount = 100.0m;

        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true);

        using var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        await harness.InputQueueSendEndpoint.Send<ProcessPayment>(new ProcessPayment(orderId, amount));

        bool paymentProcessedReceived = false;
        bool paymentPublishedCorrectly = false;

        try
        {
            paymentProcessedReceived = await harness.Consumed.Any<ProcessPayment>();
            var publishedEvent = await harness.Published.Any<PaymentProcessed>();

            if (publishedEvent)
            {
                var paymentProcessedMessage = await harness.Published.Get<PaymentProcessed>(0);
                paymentPublishedCorrectly = paymentProcessedMessage.OrderId == orderId;
            }
        }
        finally
        {
            Assert.Multiple(() =>
            {
                Assert.True(paymentProcessedReceived, "The ProcessPayment message was not consumed.");
                Assert.True(paymentPublishedCorrectly, "The PaymentProcessed event was not published with the correct OrderId.");
            });
        }
    }
}