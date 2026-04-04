using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

public class ProcessPaymentTests
{
    private readonly ITestHarness _harness;

    public ProcessPaymentTests()
    {
        var serviceProvider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true);

        _harness = serviceProvider.GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task ProcessPayment_ShouldPublishPaymentProcessedWithCorrectOrderId()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedTransactionId = Guid.NewGuid().ToString();

        // Act
        await _harness.Bus.Publish(new ProcessPayment(orderId, 100m));

        // Assert
        var published = await _harness.Published.Any<PaymentProcessed>(t => t.OrderId == orderId);
        var consumed = await _harness.Consumed.Any<ProcessPayment>();

        // Use Assert.Multiple to check both assertions
        var publishedResult = await published;
        var consumedResult = await consumed;

        Assert.True(publishedResult, "PaymentProcessed event was not published with the correct OrderId");
        Assert.True(consumedResult, "ProcessPayment message was not consumed");
        Assert.Equal(orderId, await _harness.Consumed.FirstOrDefault(c => c is ProcessPayment).Message.OrderId);
        Assert.Equal(expectedTransactionId, await _harness.Published.FirstOrDefault(p => p is PaymentProcessed).TransactionId);
    }
}