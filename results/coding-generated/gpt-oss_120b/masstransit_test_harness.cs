using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Consumer_Processes_ProcessPayment_And_Publishes_PaymentProcessed()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            });

        await using var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 99.99m;

        // Act
        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        // Allow the message pipeline to process
        // (optional small delay can be added if needed)

        // Assert
        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>(x => x.Context.Message.OrderId == orderId);

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment message was not consumed by the consumer."),
            () => Assert.True(published, "PaymentProcessed event was not published with the correct OrderId.")
        );
    }
}