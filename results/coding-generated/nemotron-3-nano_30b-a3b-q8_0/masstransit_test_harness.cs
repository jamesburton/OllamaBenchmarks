using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ReceivesAndPublishesPaymentProcessed()
    {
        var expectedOrderId = Guid.NewGuid();

        var serviceProvider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true);

        var harness = serviceProvider.GetRequiredService<ITestHarness>();

        await harness.Start();

        await harness.Bus.Publish(new ProcessPayment(expectedOrderId, 100m));

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();
        var publishedMessage = await harness.Published.Get<PaymentProcessed>();

        Assert.Multiple(() =>
        {
            Assert.True(consumed);
            Assert.True(published);
            Assert.Equal(expectedOrderId, publishedMessage.OrderId);
        });
    }
}