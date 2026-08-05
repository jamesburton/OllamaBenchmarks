using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Consumer_Processes_Message_And_Publishes_Event()
    {
        var services = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true);

        var harness = services.GetRequiredService<ITestHarness>();
        var bus = services.GetRequiredService<IBus>();

        await harness.Start();

        var orderId = Guid.NewGuid();
        var message = new ProcessPayment(orderId, 100m);

        await bus.Publish(message);

        var paymentProcessedConsumed = await harness.Consumed.Any<PaymentProcessed>();
        var paymentProcessedPublished = await harness.Published.Any<PaymentProcessed>();

        await harness.Stop();

        Assert.Multiple(
            () => Assert.True(paymentProcessedConsumed, "Consumer did not receive ProcessPayment message."),
            () => Assert.True(paymentProcessedPublished, "Consumer did not publish PaymentProcessed event.")
        );
    }
}