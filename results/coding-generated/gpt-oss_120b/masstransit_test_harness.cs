using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPayment_IsConsumed_And_PaymentProcessedPublished()
    {
        var services = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            });

        await using var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new ProcessPayment(orderId, 100m));

        bool consumed = await harness.Consumed.Any<ProcessPayment>();
        var publishedMessage = harness.Published.Select<PaymentProcessed>().FirstOrDefault();
        bool published = publishedMessage != null && publishedMessage.Context.Message.OrderId == orderId;

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment message was not consumed"),
            () => Assert.True(published, $"PaymentProcessed event not published with expected OrderId {orderId}")
        );
    }
}