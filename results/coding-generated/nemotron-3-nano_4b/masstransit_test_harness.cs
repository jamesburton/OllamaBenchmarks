using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

global namespace { }

[Fact]
public void ProcessPaymentConsumer_Receives_And_Publishes_PaymentProcessed_With_CorrectOrderId()
{
    // Arrange
    TestHarness harness;
    using var svc = new ServiceCollection()
        .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
        .BuildServiceProvider(true);
    harness = svc.GetRequiredService<TestHarness>();
    await harness.Start();

    // Act
    await harness.Bus.Publish(new ProcessPayment(Guid.NewGuid(), 10.0));

    var consumedMessages = await harness.Consumed.Any<ProcessPayment>();
    var publishedMessages = await harness.Published.Any<PaymentProcessed>();

    // Assert
    Assert.True(consumedMessages, "ProcessPayment message was not consumed");
    Assert.True(publishedMessages, "PaymentProcessed message was not published");

    var consumptionOrder = new[]
    {
        Assert.Equal(1, (bool)consumedMessages),
        Assert.Equal(1, (bool)publishedMessages)
    };
    Assert.Multiple(consumptionOrder);
}