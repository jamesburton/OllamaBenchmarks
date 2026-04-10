using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    private readonly ITestHarness _harness;
    private readonly IServiceProvider _provider;

    public ProcessPaymentConsumerTests()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<ProcessPaymentConsumer>(); })
            .BuildServiceProvider(true);
        _harness = _provider.GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task Consume_ProcessPayment_PublishesPaymentProcessed()
    {
        var orderId = Guid.NewGuid();
        var amount = 100.00m;

        await _harness.Start();

        var message = new ProcessPayment(orderId, amount);
        await _harness.Bus.Publish(message);

        var consumed = await _harness.Consumed.Any<ProcessPayment>();
        var published = await _harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(() =>
        {
            Assert.True(consumed, "Consumer should have received the ProcessPayment message");
            Assert.True(published, "PaymentProcessed event should have been published");
        });

        if (published)
        {
            var paymentProcessed = _harness.Published.GetLast<PaymentProcessed>();
            Assert.Equal(orderId, paymentProcessed.OrderId, "OrderId should match in published event");
        }
    }
}