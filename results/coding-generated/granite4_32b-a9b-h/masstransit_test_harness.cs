using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task VerifyProcessPaymentConsumer()
    {
        var serviceProvider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true);

        using (var harness = serviceProvider.GetRequiredService<ITestHarness>())
        {
            await harness.Start();

            var orderId = Guid.NewGuid();
            var amount = 123.45m;
            var message = new ProcessPayment(orderId, amount);
            await harness.Bus.Publish(message);

            bool consumed = await harness.Consumed.Any<ProcessPayment>();
            bool published = await harness.Published.Any<PaymentProcessed>();

            Assert.Multiple(() =>
            {
                Assert.True(consumed, "Consumer did not receive the message");
                Assert.True(published, "Event was not published");

                var paymentProcessedMessage = await harness.Published.Get<PaymentProcessed>();
                Assert.NotNull(paymentProcessedMessage);
                Assert.Equal(orderId, paymentProcessedMessage.OrderId);
            });
        }
    }
}