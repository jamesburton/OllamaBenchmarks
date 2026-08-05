using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public async Task Consumer_ShouldReceiveMessageAndPublishProcessedEvent()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>());
        var provider = services.BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100.50m;

        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed, "Consumer should have received the ProcessPayment message."),
            () => Assert.True(published, "Consumer should have published the PaymentProcessed event.")
        );
    }
}