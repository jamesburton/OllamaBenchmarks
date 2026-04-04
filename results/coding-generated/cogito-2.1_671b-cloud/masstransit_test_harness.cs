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
    public async Task ProcessPayment_Consumed_And_PaymentProcessed_Published()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var message = new ProcessPayment(orderId, 100.0m);

        // Act
        await _harness.Bus.Publish(message);

        // Assert - First wait for both events to complete
        var consumed = await _harness.Consumed.Any<ProcessPayment>();
        var published = await _harness.Published.Any<PaymentProcessed>();
        var publishedMessage = published ? _harness.Published.Select<PaymentProcessed>().FirstOrDefault()?.Context.Message : null;

        // Use Assert.Multiple to report all failures at once
        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment message should be consumed"),
            () => 
            {
                Assert.True(published, "PaymentProcessed event should be published");
                if (published)
                {
                    Assert.NotNull(publishedMessage);
                    Assert.Equal(orderId, publishedMessage!.OrderId);
                }
            }
        );
    }

    public async ValueTask DisposeAsync()
    {
        if (_harness != null)
            await _harness.Stop();

        if (_provider is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        else
            _provider?.Dispose();
    }
}