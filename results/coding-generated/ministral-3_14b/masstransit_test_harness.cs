using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
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

public class ProcessPaymentConsumerTests : IAsyncLifetime
{
    private readonly IServiceProvider _provider;
    private readonly ITestHarness _harness;

    public ProcessPaymentConsumerTests()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider();

        _harness = _provider.GetRequiredService<ITestHarness>();
    }

    public async ValueTask InitializeAsync()
    {
        await _harness.Start();
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.Stop();
    }

    [Fact]
    public async Task Consume_ProcessPayment_PublishesPaymentProcessedWithCorrectOrderId()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var amount = 100.50m;
        var message = new ProcessPayment(orderId, amount);

        // Act
        await _harness.Bus.Publish(message);

        // Assert
        var consumed = await _harness.Consumed.Any<ProcessPayment>();
        var published = await _harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(() =>
        {
            Assert.True(consumed, "ProcessPayment message should be consumed");
            Assert.True(published, "PaymentProcessed event should be published");

            var paymentProcessed = published.Message;
            Assert.Equal(orderId, paymentProcessed.OrderId, "OrderId should match in published event");
            Assert.NotNull(paymentProcessed.TransactionId, "TransactionId should be set");
            Assert.NotEmpty(paymentProcessed.TransactionId, "TransactionId should not be empty");
        });
    }
}