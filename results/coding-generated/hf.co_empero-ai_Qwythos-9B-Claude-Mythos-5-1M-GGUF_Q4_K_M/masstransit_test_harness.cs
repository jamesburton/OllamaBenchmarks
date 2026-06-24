using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_PublishesProcessedEvent()
    {
        var harness = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true);

        await harness.Start();

        Guid sentOrderId = Guid.NewGuid();
        decimal amount = 10.5m;

        await harness.Bus.Publish(new ProcessPayment(sentOrderId, amount));

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var publishedMsg = await harness.Published.Get<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(publishedMsg.OrderId == sentOrderId && !string.IsNullOrEmpty(publishedMsg.TransactionId))
        );
    }
}