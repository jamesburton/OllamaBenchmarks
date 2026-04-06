using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ShouldPublishPaymentProcessed()
    {
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<ProcessPaymentConsumer>(); })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100.00m;

        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment should be consumed"),
            () => Assert.True(published, "PaymentProcessed should be published")
        );

        var publishedMessage = await harness.Published.First<PaymentProcessed>();
        Assert.Equal(orderId, publishedMessage.Context.Message.OrderId);
    }
}