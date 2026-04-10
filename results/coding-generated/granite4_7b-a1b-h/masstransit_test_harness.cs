using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using MassTransit.Testing.Xunit;
using Xunit;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ProcessesMessageAndPublishesEvent()
    {
        var serviceCollection = new ServiceCollection();
        var serviceProvider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true);

        var harness = serviceProvider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderPayment = new ProcessPayment(Guid.NewGuid(), 100.00m);
        harness.Bus.Publish(orderPayment);

        await Task.Delay(100); // Ensure message processing completes

        Assert.True(await harness.Consumed.Any<ProcessPayment>());
        Assert.True(await harness.Published.Any<PaymentProcessed>());

        var consumed = await harness.Consumed.Take<ProcessPayment>();
        var published = await harness.Published.Take<PaymentProcessed>();

        Assert.Equal(consumed.Message.OrderId, published.Message.OrderId);
        Assert.NotEqual(published.Message.TransactionId, Guid.Empty.ToString());
    }
}