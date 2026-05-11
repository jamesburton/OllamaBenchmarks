using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ProcessesMessageAndPublishesEvent()
    {
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<ProcessPaymentConsumer>(); })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new ProcessPayment(orderId, 100m));

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment should have been consumed"),
            () => Assert.True(published, "PaymentProcessed should have been published")
        );
    }

    [Fact]
    public async Task ProcessPaymentConsumer_PublishesEventWithCorrectOrderId()
    {
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<ProcessPaymentConsumer>(); })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new ProcessPayment(orderId, 100m));

        var published = await harness.Published.Any<PaymentProcessed>();
        var publishedMessage = await harness.Published.GetLast<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(published, "PaymentProcessed should have been published"),
            () => Assert.Equal(orderId, publishedMessage.Message.OrderId, "OrderId should match")
        );
    }
}