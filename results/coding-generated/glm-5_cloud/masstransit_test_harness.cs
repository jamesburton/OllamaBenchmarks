using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Should_Consume_ProcessPayment_And_Publish_PaymentProcessed()
    {
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100.50m;

        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        bool consumed = await harness.Consumed.Any<ProcessPayment>();
        bool published = await harness.Published.Any<PaymentProcessed>(x => x.Context.Message.OrderId == orderId);

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment message was not consumed"),
            () => Assert.True(published, "PaymentProcessed message was not published with the correct OrderId")
        );
    }
}