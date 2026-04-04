using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests : IAsyncLifetime
{
    private ITestHarness _harness = null!;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });

        var provider = services.BuildServiceProvider(true);
        _harness = provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
    }

    [Fact]
    public async Task ConsumerReceivesAndPublishesCorrectEvent()
    {
        var orderId = Guid.NewGuid();
        var message = new ProcessPayment(orderId, 99.99m);

        await _harness.Bus.Publish(message);

        var consumedAny = await _harness.Consumed.Any<ProcessPayment>();
        var publishedAny = await _harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumedAny),
            () => Assert.True(publishedAny)
        );
    }

    [Fact]
    public async Task PublishedPaymentProcessedHasCorrectOrderId()
    {
        var orderId = Guid.NewGuid();
        var message = new ProcessPayment(orderId, 149.99m);

        await _harness.Bus.Publish(message);

        var publishedAny = await _harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(publishedAny)
        );

        var publishedMessage = await _harness.Published.SelectAsync<PaymentProcessed>().FirstOrDefault();
        Assert.NotNull(publishedMessage);
        Assert.Equal(orderId, publishedMessage.Message.OrderId);
    }
}