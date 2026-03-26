using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ShouldPublishPaymentProcessed_WithCorrectOrderId()
    {
        var services = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<ProcessPaymentConsumer>(); })
            .BuildServiceProvider(true);

        var harness = services.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var message = new ProcessPayment(orderId, 100.00m);

        await harness.Bus.Publish(message);

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        var consumedResult = await harness.Consumed.Any<ProcessPayment>();
        var publishedResult = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumedResult),
            () => Assert.True(publishedResult)
        );

        var publishedMessage = harness.Published.Select<PaymentProcessed>().FirstOrDefault();
        Assert.NotNull(publishedMessage);
        Assert.Equal(orderId, publishedMessage.Message.OrderId);
    }
}