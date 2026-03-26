using MassTransit;
using MassTransit.Testing;
using Xunit;
using Xunit.v3;

public record ProcessPayment(Guid OrderId, decimal Amount);
public record PaymentProcessed(Guid OrderId, string TransactionId);

public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        await context.Publish(new PaymentProcessed(context.Message.OrderId, Guid.NewGuid().ToString()));
    }
}

public class ProcessPaymentConsumerTests : IAsyncLifetime
{
    private ITestHarness _harness;
    private IBus _bus;

    public async ValueTask InitializeAsync()
    {
        var collection = new ServiceCollection();
        collection.AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>());
        var provider = collection.BuildServiceProvider();

        _harness = provider.GetRequiredService<ITestHarness>();
        _bus = _harness.Bus;

        await _harness.Start();
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.Stop();
    }

    [Fact]
    public async Task ProcessPayment_WhenCalled_ShouldPublishPaymentProcessed()
    {
        var orderId = Guid.NewGuid();
        var amount = 100.50m;

        await _bus.Publish(new ProcessPayment(orderId, amount));

        Assert.Multiple(() =>
        {
            Assert.True(await _harness.Consumed.Any<ProcessPayment>(), "ProcessPayment message should be consumed");
            Assert.True(await _harness.Consumed.Any<PaymentProcessed>(), "PaymentProcessed event should be published");
        });
    }
}