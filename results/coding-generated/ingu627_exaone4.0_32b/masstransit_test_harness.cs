using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Should_process_message_and_publish_event()
    {
        var services = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true);

        var harness = services.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100.50m;
        var message = new ProcessPayment(orderId, amount);

        await harness.Bus.Publish(message);

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );
    }
}