using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ProcessesMessage_AndPublishesEvent()
    {
        var orderId = Guid.NewGuid();

        var services = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<ProcessPaymentConsumer>(); })
            .BuildServiceProvider(true);

        var harness = services.GetRequiredService<ITestHarness>();
        await harness.Start();

        await harness.Bus.Publish(new ProcessPayment(orderId, 100.0m));

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();
        var paymentCtx = await harness.Published.Select<PaymentProcessed>().FirstOrDefault();

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment message was not consumed"),
            () => Assert.True(published, "PaymentProcessed event was not published"),
            () => Assert.NotNull(paymentCtx, "PaymentProcessed context is null"),
            () => Assert.Equal(orderId, paymentCtx?.Message.OrderId)
        );
    }
}