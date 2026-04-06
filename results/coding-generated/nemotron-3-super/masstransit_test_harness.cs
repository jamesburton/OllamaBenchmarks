using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

[Fact]
public async Task ProcessPaymentConsumer_ReceivesAndPublishes()
{
    var orderId = Guid.NewGuid();
    var amount = 100m;

    var services = new ServiceCollection()
        .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<ProcessPaymentConsumer>(); })
        .BuildServiceProvider(true);

    var harness = services.GetRequiredService<ITestHarness>();
    await harness.Start();

    await harness.Bus.Publish(new ProcessPayment(orderId, amount));

    var consumed = await harness.Consumed.Any<ProcessPayment>();
    var publishedMessages = await harness.Published.Select<PaymentProcessed>();
    var publishedCorrect = publishedMessages.Any() && publishedMessages[0].Message.OrderId == orderId;

    Assert.Multiple(
        () => Assert.True(consumed, "Consumer did not receive the ProcessPayment message"),
        () => Assert.True(publishedCorrect, "PaymentProcessed event with correct OrderId was not published")
    );
}