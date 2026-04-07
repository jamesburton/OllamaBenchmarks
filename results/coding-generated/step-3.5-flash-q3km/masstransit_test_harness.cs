using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests : IAsyncLifetime
{
    private ServiceProvider _provider;
    private ITestHarness _harness;

    public async ValueTask InitializeAsync()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>());
        _provider = services.BuildServiceProvider(true);
        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task Should_consume_ProcessPayment_and_publish_PaymentProcessed_with_correct_OrderId()
    {
        var orderId = Guid.NewGuid();
        var amount = 100m;

        await _harness.Bus.Publish(new ProcessPayment(orderId, amount));

        bool consumed = await _harness.Consumed.Any<ProcessPayment>(x => x.Message.OrderId == orderId);
        bool published = await _harness.Published.Any<PaymentProcessed>(x => x.Message.OrderId == orderId);

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment message was not consumed by the consumer"),
            () => Assert.True(published, "PaymentProcessed event was not published with matching OrderId")
        );
    }
}