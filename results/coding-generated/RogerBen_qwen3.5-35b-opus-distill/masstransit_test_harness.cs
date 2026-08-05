using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPayment_WhenCalled_ShouldPublishPaymentProcessed()
    {
        var serviceProvider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<ProcessPaymentConsumer>(); })
            .BuildServiceProvider(true);

        var harness = serviceProvider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new ProcessPayment(orderId, 100.00m));

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(() =>
        {
            Assert.True(consumed, "ProcessPayment message should be consumed");
            Assert.True(published, "PaymentProcessed event should be published");
        });
    }
}