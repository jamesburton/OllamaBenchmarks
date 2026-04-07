using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public sealed class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPayment_IsConsumed_And_PaymentProcessed_IsPublished()
    {
        // Arrange: build the test harness with the consumer
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 99.99m;

        // Act: publish the ProcessPayment command
        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        // Assert: verify consumption and publication
        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();
        var publishedMessage = harness.Published
            .Select<PaymentProcessed>()
            .FirstOrDefault()
            ?.Context
            .Message;

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment message was not consumed."),
            () => Assert.True(published, "PaymentProcessed event was not published."),
            () => Assert.Equal(orderId, publishedMessage?.OrderId)
        );
    }
}