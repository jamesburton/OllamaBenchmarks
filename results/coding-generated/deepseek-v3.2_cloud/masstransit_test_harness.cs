using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests : IAsyncLifetime
{
    private ServiceProvider _provider;
    private ITestHarness _harness;

    public async ValueTask InitializeAsync()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    [Fact]
    public async Task ProcessPaymentConsumer_ProcessesMessage_AndPublishesEvent()
    {
        var orderId = Guid.NewGuid();
        var amount = 100.50m;
        var message = new ProcessPayment(orderId, amount);

        await _harness.Bus.Publish(message);

        var consumed = await _harness.Consumed.Any<ProcessPayment>();
        var published = await _harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment message should be consumed"),
            () => Assert.True(published, "PaymentProcessed event should be published")
        );
    }

    [Fact]
    public async Task ProcessPaymentConsumer_PublishesEvent_WithCorrectOrderId()
    {
        var orderId = Guid.NewGuid();
        var amount = 75.25m;
        var message = new ProcessPayment(orderId, amount);

        await _harness.Bus.Publish(message);

        var consumed = await _harness.Consumed.Any<ProcessPayment>(x => x.Context.Message.OrderId == orderId);
        var publishedEvent = (await _harness.Published.SelectAsync<PaymentProcessed>()).FirstOrDefault();

        Assert.Multiple(
            () => Assert.True(consumed, $"ProcessPayment with OrderId {orderId} should be consumed"),
            () => Assert.NotNull(publishedEvent),
            () => Assert.Equal(orderId, publishedEvent?.Context.Message.OrderId)
        );
    }

    [Fact]
    public async Task ProcessPaymentConsumer_CompletesProcessing_ForMultipleMessages()
    {
        var messages = new[]
        {
            new ProcessPayment(Guid.NewGuid(), 50.0m),
            new ProcessPayment(Guid.NewGuid(), 150.0m),
            new ProcessPayment(Guid.NewGuid(), 200.0m)
        };

        foreach (var message in messages)
        {
            await _harness.Bus.Publish(message);
        }

        await _harness.InactivityTask;

        var consumedCount = await _harness.Consumed.Count<ProcessPayment>();
        var publishedCount = await _harness.Published.Count<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.Equal(3, consumedCount),
            () => Assert.Equal(3, publishedCount)
        );
    }

    public async ValueTask DisposeAsync()
    {
        if (_harness != null)
            await _harness.Stop();

        await _provider.DisposeAsync();
    }
}