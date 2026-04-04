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

        // Act
        await _harness.Bus.Publish(new ProcessPayment(orderId, 99.99m));

        // Assert
        var published = await _harness.Published.Any<PaymentProcessed>(p => p.OrderId == orderId);
        Assert.True(published);

        var consumed = await _harness.Consumed.Any<ProcessPayment>();
        Assert.True(consumed);

        // Use Assert.Multiple to validate both assertions
        var result = Assert.Multiple(
            () => Assert.Equal(orderId, ((PaymentProcessed)await _harness.Published.FirstOrDefault(p => p.OrderId == orderId)).OrderId),
            () => Assert.Equal(Guid.NewGuid().ToString(), ((PaymentProcessed)await _harness.Published.FirstOrDefault(p => p.OrderId == orderId)).TransactionId)
        );

        result.Should().BeTrue();
    }
}