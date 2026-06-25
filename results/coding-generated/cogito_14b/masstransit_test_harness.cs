using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITestHarness _harness;

    public ProcessPaymentConsumerTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>());
        _serviceProvider = services.BuildServiceProvider(true);
        _harness = _serviceProvider.GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task ProcessPayment_Consumer_Received_And_PaymentProcessed_Published()
    {
        await _harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 10.99m;

        // Publish the message
        await _harness.Bus.Publish(new ProcessPayment(orderId, amount));

        // Check if consumer received the message
        bool consumed = await _harness.Consumed.Any<ProcessPayment>();

        // Check if PaymentProcessed was published with correct OrderId
        var paymentProcessedMessages = await _harness.Published.Select<PaymentProcessed>()
            .Where(p => p.OrderId == orderId)
            .ToListAsync();

        bool published = paymentProcessedMessages.Count > 0;

        Assert.Multiple(() =>
        {
            Assert.True(consumed, "ProcessPayment message was not consumed");
            Assert.True(published, "No PaymentProcessed message with matching OrderId was published");
        });
    }
}