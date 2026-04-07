using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

[Fact]
public async Task ProcessPaymentConsumer_receives_and_publishes()
{
    var orderId = Guid.NewGuid();

    var services = new ServiceCollection()
        .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
        .BuildServiceProvider(true);

    var harness = services.GetRequiredService<ITestHarness>();
    await harness.Start();

    try
    {
        await harness.Bus.Publish(new ProcessPayment(orderId, 123.45m));

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>(msg => msg.OrderId == orderId);

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published));
    }
    finally
    {
        await harness.Stop();
    }
}