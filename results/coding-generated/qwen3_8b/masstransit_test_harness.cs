using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    private readonly ITestHarness _harness;

    public ProcessPaymentConsumerTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg => 
            cfg.AddConsumer<ProcessPaymentConsumer>());

        var provider = services.BuildServiceProvider(true);
        _harness = provider.GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task Consumer_Receives_ProcessPayment_And_Publishes_PaymentProcessed()
    {
        await _harness.Start();

        var orderId = Guid.NewGuid();
        var published = await _harness.Published.Any<PaymentProcessed>(x => 
            x.OrderId == orderId);

        var consumed = await _harness.Consumed.Any<ProcessPayment>(x => 
            x.OrderId == orderId);

        await _harness.Stop();

        Assert.Multiple(() =>
        {
            Assert.True(published, "PaymentProcessed event was not published");
            Assert.True(consumed, "ProcessPayment message was not consumed");
        });
    }
}