using MassTransit;
using MassTransit.TestFramework;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public record ProcessPayment(Guid OrderId, decimal Amount);
public record PaymentProcessed(Guid OrderId, string TransactionId);

public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        await context.Publish(new PaymentProcessed(context.Message.OrderId, Guid.NewGuid().ToString()));
    }
}

public class ProcessPaymentConsumerTests
{
    private readonly ITestHarness _harness;

    public ProcessPaymentConsumerTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg => 
            cfg.AddConsumer<ProcessPaymentConsumer>());
        _harness = services.BuildServiceProvider().GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task ProcessPayment_Consumer_When_ProcessPayment_Received_Publishes_PaymentProcessed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var amount = 100.00m;
        var payment = new ProcessPayment(orderId, amount);

        // Act
        await _harness.Start();
        await _harness.Bus.Publish(payment);
        await _harness.Bus.Stop();

        // Assert
        await Assert.Multiple(async () =>
        {
            // Verify ProcessPayment was consumed
            Assert.True(await _harness.Consumed.Any<ProcessPayment>());
            var consumedPayment = await _harness.Consumed.Select<ProcessPayment>().GetMessage();
            Assert.Equal(orderId, consumedPayment.OrderId);

            // Verify PaymentProcessed was published
            Assert.True(await _harness.Published.Any<PaymentProcessed>());
            var publishedPayment = await _harness.Published.Select<PaymentProcessed>().GetMessage();
            Assert.Equal(orderId, publishedPayment.OrderId);
        });
    }
}