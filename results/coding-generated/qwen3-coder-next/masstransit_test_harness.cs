using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    private readonly ITestHarness harness;

    public ProcessPaymentConsumerTests()
    {
        harness = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<ProcessPaymentConsumer>(); })
            .BuildServiceProvider(true)
            .GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task ProcessPaymentConsumer_Should_Process_Message_And_Publish_Event()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var amount = 99.99m;
        var message = new ProcessPayment(orderId, amount);

        await harness.Start();

        // Act
        await harness.Bus.Publish(message);

        // Assert
        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(() =>
        {
            Assert.True(consumed);
            Assert.True(published);
        });

        // Verify the published message has the correct OrderId
        var publishedMessages = await harness.Published.GetConsumedMessages<PaymentProcessed>();
        Assert.Single(publishedMessages);
        Assert.Equal(orderId, publishedMessages[0].Message.OrderId);
    }
}