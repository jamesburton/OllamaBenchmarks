using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

[Fact]
public void ProcessPaymentConsumer_ProcessesMessage_PublishesPaymentProcessed()
{
    var provider = new ServiceCollection()
        .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
        .BuildServiceProvider(true);

    var harness = provider.GetRequiredService<ITestHarness>();
    await harness.Start();

    var orderId = Guid.NewGuid();
    var msg = new ProcessPayment(orderId, 0m);
    await harness.Bus.Publish(msg);

    var publishedEvent = await harness.Bus.GetMessages(PaymentProcessed);
    Assert.NotNull(publishedEvent, "A PaymentProcessed event should have been published");
    Assert.Equal(orderId, publishedEvent.OrderId);

    var consumed = await harness.Consumed.Any<ProcessPayment>().ConfigureAwait(false);
    var published = await harness.Published.Any<PaymentProcessed>().ConfigureAwait(false);
    Assert.True(consumed);
    Assert.True(published);
}