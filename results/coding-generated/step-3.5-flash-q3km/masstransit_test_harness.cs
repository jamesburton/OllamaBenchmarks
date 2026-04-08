using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Should_Process_Payment_And_Publish_PaymentProcessed()
    {
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            Guid orderId = Guid.NewGuid();
            decimal amount = 100m;

            await harness.Bus.Publish(new ProcessPayment(orderId, amount));

            bool consumed = await harness.Consumed.Any<ProcessPayment>();
            bool publishedWithCorrectOrderId = await harness.Published.Any<PaymentProcessed>(x => x.Context.Message.OrderId == orderId);

            Assert.Multiple(() =>
            {
                Assert.True(consumed, "ProcessPayment was not consumed");
                Assert.True(publishedWithCorrectOrderId, "PaymentProcessed with the correct OrderId was not published");
            });
        }
        finally
        {
            await harness.Stop();
            await provider.DisposeAsync();
        }
    }
}