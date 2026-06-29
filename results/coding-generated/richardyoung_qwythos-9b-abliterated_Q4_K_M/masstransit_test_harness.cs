using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public async Task ProcessPaymentConsumer()
    {
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        Assert.NotNull(harness.Bus.GetConsumerDefinition<ProcessPaymentConsumer>());

        var orderId = Guid.NewGuid();
        var payment = new ProcessPayment(orderId, 10m);
        await harness.Bus.Publish(payment);

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published),
            () => Assert.Equal(orderId, harness.Published<PaymentProcessed>().First().OrderId)
        );
    }
}