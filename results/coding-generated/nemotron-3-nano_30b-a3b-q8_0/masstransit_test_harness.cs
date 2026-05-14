using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace MyTest;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Consumes_ProcessPayment_And_Publishes_PaymentProcessed()
    {
        using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        await harness.Bus.Publish(new ProcessPayment(Guid.NewGuid(), 100m));

        var paymentProcessedConsumed = await harness.Consumed.Any<PaymentProcessed>();
        var paymentProcessedPublished = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(paymentProcessedConsumed),
            () => Assert.True(paymentProcessedPublished)
        );
    }
}