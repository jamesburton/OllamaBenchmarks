using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    private readonly ITestHarness harness;

    public ProcessPaymentConsumerTests()
    {
        harness = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<ProcessPaymentConsumer>(); })
            .BuildServiceProvider(true)
            .GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task ProcessPaymentConsumer_Should_Process_Message_And_Publish_Event()
    {
        var orderId = Guid.NewGuid();
        var amount = 99.99m;

        await harness.Start();
        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        // Await checks before Assert.Multiple since it doesn't support async lambdas
        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(() =>
        {
            Assert.True(consumed);
            Assert.True(published);
        });
    }
}