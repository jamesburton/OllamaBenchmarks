using MassTransit;
using Microsoft.Extensions.DependencyInjection;

public class PaymentProcessorTests
{
    [Fact]
    public async Task Test()
    {
        var serviceCollection = new ServiceCollection();
        await using (var harness = new ServiceCollection().AddMassTransitTestHarness(serviceCollection))
        {
            harness.AddConsumer<ProcessPaymentConsumer>();
            harness.Start();
            await harness.Publish(new ProcessPayment(Guid.NewGuid(), 10m));
            Assert.True(harness.Consumed.Any<ProcessPayment>());
            var paymentProcessed = harness.Published.Any<PaymentProcessed>();
            Assert.Equal(1, paymentProcessed.OrderId);
        }
    }
}