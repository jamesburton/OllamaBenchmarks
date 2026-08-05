using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests : IAsyncLifetime
{
    private readonly ITestHarness _harness;

    public ProcessPaymentConsumerTests()
    {
        _harness = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true)
            .GetRequiredService<ITestHarness>();
    }

    public async ValueTask InitializeAsync() => await _harness.Start();

    public async ValueTask DisposeAsync() => await _harness.Stop();

    [Fact]
    public async Task ProcessPayment_ConsumesMessageAndPublishesEvent()
    {
        var orderId = Guid.NewGuid();
        var message = new ProcessPayment(orderId, 100.0m);

        await _harness.Bus.Publish(message);

        bool consumed = await _harness.Consumed.Any<ProcessPayment>();
        bool published = await _harness.Published.Any<PaymentProcessed>(x => x.Message.OrderId == orderId);

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );
    }
}