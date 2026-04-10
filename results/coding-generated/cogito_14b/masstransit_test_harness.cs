using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    private readonly ServiceCollection _services;
    private readonly IServiceProvider _provider;
    private readonly ITestHarness _harness;

    public ProcessPaymentTests()
    {
        _services = new ServiceCollection();
        _services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>());
        _provider = _services.BuildServiceProvider(true);
        _harness = _provider.GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task ProcessPayment_Consumer_Received_PaymentProcessed_Published()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var amount = 100.00m;
        var payment = new ProcessPayment(orderId, amount);

        // Act
        await _harness.Start();
        await _harness.Bus.Publish(payment);

        // Assert
        var consumed = await _harness.Consumed.Any<ProcessPayment>();
        var published = await _harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(() =>
        {
            Assert.True(consumed, "ProcessPayment message was consumed");
            Assert.True(published, "PaymentProcessed message was published");
        });
    }
}