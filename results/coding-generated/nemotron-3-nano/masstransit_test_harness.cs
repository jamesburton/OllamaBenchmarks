using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

[Fact]
public async Task ProcessPaymentConsumer_Handles_ProcessPayment_And_Publishes_PaymentProcessed()
{
    var services = new ServiceCollection();
    services.AddMassTransitTestHarness(cfg =>
    {
        cfg.AddConsumer<ProcessPaymentConsumer>();
    });
    var provider = services.BuildServiceProvider(true);

    var harness = provider.GetRequiredService<ITestHarness>();

    await harness.Start();

    var paymentMessage = new ProcessPayment(Guid.NewGuid(), 100.50m);
    await harness.Bus.Publish(paymentMessage);

    var consumerHandled = await harness.Consumed.Any<ProcessPayment>();
    Assert.True(consumerHandled);

    var publishedProcessed = await harness.Published.Any<PaymentProcessed>();
    Assert.True(publishedProcessed);

    Assert.Multiple(
        () => Assert.True(consumerHandled),
        () => Assert.True(publishedProcessed)
    );
}