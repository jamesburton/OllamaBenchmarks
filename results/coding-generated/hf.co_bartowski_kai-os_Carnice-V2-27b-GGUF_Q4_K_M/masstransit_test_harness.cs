using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Consume_ProcessPayment_PublishesPaymentProcessed()
    {
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<ProcessPaymentConsumer>(); })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new ProcessPayment(orderId, 99.99m));

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();
        var publishedMessages = harness.Published.Messages<PaymentProcessed>();
        var publishedOrderId = publishedMessages.FirstOrDefault()?.OrderId;

        Assert.Multiple(
            () => Assert.True(consumed, "ProcessPayment should be consumed"),
            () => Assert.True(published, "PaymentProcessed should be published"),
            () => Assert.Equal(orderId, publishedOrderId, "OrderId should match")
        );
    }
}

services.AddMassTransit(x =>
{
    x.AddConsumer<OrderConsumer>();
    x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
});
// Use IBusRegistrationConfigurator (NOT IServiceCollectionBusConfigurator)