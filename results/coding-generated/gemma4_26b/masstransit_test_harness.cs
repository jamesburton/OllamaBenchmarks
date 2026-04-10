using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Consume_Should_ProcessPayment_And_Publish_PaymentProcessed()
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
        var amount = 99.99m;
        var message = new ProcessPayment(orderId, amount);

        // Act
        await harness.Bus.Publish(message);

        // Assert - Await results individually before Assert.Multiple as it does not support async lambdas
        bool wasConsumed = await harness.Consumed.Any<ProcessPayment>();
        bool wasPublished = await harness.Published.Any<PaymentProcessed>();

        var publishedContext = harness.Published.Select<PaymentProcessed>().FirstOrDefault();
        bool orderIdMatches = publishedContext != null && publishedContext.Context.Message.OrderId == orderId;

        Assert.Multiple(
            () => Assert.True(wasConsumed, "The consumer did not receive the ProcessPayment message."),
            () => Assert.True(wasPublished, "The PaymentProcessed event was not published."),
            () => Assert.True(orderIdMatches, "The published PaymentProcessed event does not contain the correct OrderId.")
        );
    }
}