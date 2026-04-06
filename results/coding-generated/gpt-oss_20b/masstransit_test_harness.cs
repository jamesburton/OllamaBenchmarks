using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ShouldPublishPaymentProcessed()
    {
        // Arrange
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 123.45m;

        // Act
        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        // Assert
        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();
        var publishedMessage = await harness.Published.Select<PaymentProcessed>().FirstOrDefaultAsync();

        var correctOrderId = publishedMessage != null && publishedMessage.Message.OrderId == orderId;

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment was not consumed"),
            () => Assert.True(published, "PaymentProcessed was not published"),
            () => Assert.True(correctOrderId, "Published PaymentProcessed has incorrect OrderId")
        );

        await harness.Stop();
    }
}