using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Consume_ProcessPayment_Publishes_PaymentProcessed()
    {
        var provider = new ServiceCollection().AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        }).BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new ProcessPayment(orderId, 100m));

        bool consumed = await harness.Consumed.Any<ProcessPayment>();
        bool published = await harness.Published.Any<PaymentProcessed>(x => x.Message.OrderId == orderId);

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );
    }
}