using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ShouldProcessMessageAndPublishEvent()
    {
        var harness = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true);

        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100.0m;

        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var publishedMessages = await harness.Published.GetPublished<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed),
            () =>
            {
                Assert.NotNull(publishedMessages);
                Assert.Single(publishedMessages);
                Assert.Equal(orderId, publishedMessages.First().Message.OrderId);
            }
        );
    }
}