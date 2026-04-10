using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ShouldProcessPaymentAndPublishEvent()
    {
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<ProcessPaymentConsumer>(); })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 99.99m;

        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        bool isConsumed = await harness.Consumed.Any<ProcessPayment>();
        bool isPublished = await harness.Published.Any<PaymentProcessed>();
        var publishedMessage = await harness.Published.Select<PaymentProcessed>().First();

        Assert.Multiple(
            () => Assert.True(isConsumed),
            () => Assert.True(isPublished),
            () => Assert.Equal(orderId, publishedMessage.OrderId)
        );
    }
}