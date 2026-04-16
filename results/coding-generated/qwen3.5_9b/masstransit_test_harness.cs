using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    private readonly ITestHarness _harness;

    public ProcessPaymentTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });
        _harness = services.BuildServiceProvider(true).GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task ProcessPaymentConsumerReceivesAndProcessesMessage()
    {
        await _harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100.0m;
        var message = new ProcessPayment(orderId, amount);

        await _harness.Bus.Publish(message);

        var consumed = await _harness.Consumed.Any<ProcessPayment>();
        var published = await _harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed, "Consumer did not consume the ProcessPayment message"),
            () => Assert.True(published, "Consumer did not publish the PaymentProcessed event")
        );
    }

    [Fact]
    public async Task PublishedEventContainsCorrectOrderId()
    {
        await _harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 50.0m;
        var message = new ProcessPayment(orderId, amount);

        await _harness.Bus.Publish(message);

        var consumed = await _harness.Consumed.Any<ProcessPayment>();
        var published = await _harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed, "Consumer did not consume the message"),
            () => Assert.True(published, "Consumer did not publish the event"),
            () =>
            {
                var events = _harness.Published.Get<PaymentProcessed>();
                Assert.Single(events);
                Assert.Equal(orderId, events[0].OrderId);
            }
        );
    }
}