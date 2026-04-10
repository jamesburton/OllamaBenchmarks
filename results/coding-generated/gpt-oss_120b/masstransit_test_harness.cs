using System;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Consumer_Processes_Payment_And_Publishes_Event()
    {
        // Arrange: build the test harness with the consumer registration
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var payment = new ProcessPayment(orderId, 99.99m);

        // Act: publish the command message
        await harness.Bus.Publish(payment);

        // Assert: evaluate the async checks first
        bool wasConsumed = await harness.Consumed.Any<ProcessPayment>();
        bool wasPublished = await harness.Published.Any<PaymentProcessed>(x => x.Context.Message.OrderId == orderId);

        Assert.Multiple(
            () => Assert.True(wasConsumed, "ProcessPayment message was not consumed by the consumer."),
            () => Assert.True(wasPublished, "PaymentProcessed event was not published with the correct OrderId.")
        );
    }
}