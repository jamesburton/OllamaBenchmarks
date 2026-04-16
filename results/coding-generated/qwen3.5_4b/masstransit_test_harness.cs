using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class PaymentProcessingTests
{
    private readonly ITestHarness _harness;

    public PaymentProcessingTests(ServiceProvider provider)
    {
        _harness = provider.GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task ProcessPaymentConsumerPublishesPaymentProcessedEvent()
    {
        var orderId = Guid.NewGuid();
        var amount = 99.99m;

        await _harness.Start();

        await _harness.Bus.Publish(new ProcessPayment(orderId, amount));

        var consumed = await _harness.Consumed.Any<ProcessPayment>();
        var published = await _harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment message should be consumed"),
            () => Assert.True(published, "PaymentProcessed event should be published")
        );
    }

    [Fact]
    public async Task ProcessPaymentConsumerPreservesOrderId()
    {
        var orderId = Guid.NewGuid();
        var amount = 150.00m;

        await _harness.Start();

        await _harness.Bus.Publish(new ProcessPayment(orderId, amount));

        var consumed = await _harness.Consumed.Any<ProcessPayment>();
        var published = await _harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment message should be consumed"),
            () => Assert.True(published, "PaymentProcessed event should be published"),
            () => Assert.Equal(orderId, _harness.Published.First<PaymentProcessed>().Message.OrderId)
        );
    }
}