using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Consumer_ShouldProcessMessage_AndPublishEvent()
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
        var amount = 100.50m;

        // Act
        await harness.Bus.Publish(new ProcessPayment(orderId, amount));

        // Assert
        // Await harness checks individually since Assert.Multiple does not support async lambdas
        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(() =>
        {
            Assert.True(consumed, "ProcessPayment message was not consumed");
            Assert.True(published, "PaymentProcessed event was not published");
        });

        // Verify content of the published event
        var publishedMessage = harness.Published.Select<PaymentProcessed>().First();

        Assert.Equal(orderId, publishedMessage.Context.Message.OrderId);
    }
}