using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ShouldReceiveProcessPaymentAndPublishPaymentProcessed()
    {
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<ProcessPaymentConsumer>(); })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100.00m;
        var message = new ProcessPayment(orderId, amount);

        await harness.Bus.Publish(message);

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        var publishedMessage = await harness.Published.First<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published),
            () => Assert.Equal(orderId, publishedMessage.Message.OrderId)
        );
    }
}