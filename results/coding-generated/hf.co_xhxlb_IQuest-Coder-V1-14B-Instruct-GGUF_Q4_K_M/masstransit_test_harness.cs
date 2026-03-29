using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_should_consume_and_publish_PaymentProcessed()
    {
        var serviceProvider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = serviceProvider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var processPayment = new ProcessPayment(orderId, 100.00m);

        await harness.Bus.Publish(processPayment);

        Assert.Multiple(() =>
        {
            Assert.True(await harness.Consumed.Any<ProcessPayment>());
            Assert.True(await harness.Published.Any<PaymentProcessed>());
        });
    }
}