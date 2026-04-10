using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests : IAsyncLifetime
{
    private readonly ServiceProvider _provider;
    private ITestHarness _harness;

    public ProcessPaymentConsumerTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });
        _provider = services.BuildServiceProvider(true);
    }

    public ValueTask InitializeAsync()
    {
        _harness = _provider.GetRequiredService<ITestHarness>();
        return _harness.Start();
    }

    public ValueTask DisposeAsync()
    {
        return _provider.DisposeAsync().AsValueTask();
    }

    [Fact]
    public async Task Consume_Should_ProcessPayment_And_Publish_PaymentProcessed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var amount = 99.99m;
        var message = new ProcessPayment(orderId, amount);

        // Act
        await _harness.Bus.Publish(message);

        // Assert
        // We must await the harness checks individually before Assert.Multiple 
        // because Assert.Multiple does not support async lambdas.
        bool wasConsumed = await _harness.Consumed.Any<ProcessPayment>();

        // Find the published message to verify the OrderId
        var publishedMessage = await _harness.Published.Any<PaymentProcessed>();
        bool hasCorrectOrderId = false;
        if (publishedMessage)
        {
            var publishedContext = _harness.Published.Select<PaymentProcessed>().FirstOrDefault();
            if (publishedContext != null)
            {
                hasCorrectOrderId = publishedContext.Context.Message.OrderId == orderId;
            }
        }

        Assert.Multiple(
            () => Assert.True(wasConsumed, "The ProcessPayment message was not consumed."),
            () => Assert.True(hasCorrectOrderId, $"The published PaymentProcessed event did not contain the correct OrderId. Expected: {orderId}")
        );
    }
}