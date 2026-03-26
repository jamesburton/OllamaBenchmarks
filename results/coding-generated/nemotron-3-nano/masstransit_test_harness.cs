using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public async Task ProcessPayment_consumer_processes_message_and_publishes_PaymentProcessed()
    {
        // Build test harness
        var services = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = services.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Arrange
        Guid orderId = Guid.NewGuid();

        // Act
        await harness.Bus.Publish(new ProcessPayment(orderId, 100m));

        // Assert: consumer received the message
        bool received = await harness.Consumed.Any<ProcessPayment>();

        // Assert: a PaymentProcessed event was published
        bool published = await harness.Published.Any<PaymentProcessed>();
        PaymentProcessed publishedMsg = await harness.Published.Get<PaymentProcessed>(0);

        // Combine assertions
        Assert.Multiple(
            () => Assert.True(received),
            () => Assert.True(published),
            () => Assert.Equal(orderId, publishedMsg.OrderId)
        );
    }
}