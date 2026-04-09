using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests : IAsyncLifetime
{
    private ServiceProvider _provider;
    private ITestHarness _harness;

    public ProcessPaymentConsumerTests()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
    }

    public async ValueTask InitializeAsync()
    {
        await _harness.Start();
    }

    public async ValueTask DisposeAsync()
    {
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task Should_Consume_ProcessPayment_And_Publish_PaymentProcessed_WithCorrectOrderId()
    {
        var orderId = Guid.NewGuid();
        var amount = 250.00m;

        await _harness.Bus.Publish(new ProcessPayment(orderId, amount));

        var consumed = await _harness.Consumed.Any<ProcessPayment>();
        var published = await _harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment message should have been consumed"),
            () =>
            {
                Assert.True(published, "PaymentProcessed event should have been published");
                var publishedMessage = _harness.Published.Select<PaymentProcessed>().First();
                Assert.Equal(orderId, publishedMessage.Message.OrderId);
            }
        );
    }
}