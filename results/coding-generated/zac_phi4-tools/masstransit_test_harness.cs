using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Consumer_Processes_Payment_And_Publishes_Event()
    {
        var orderId = Guid.NewGuid();
        var amount = 100.0m;

        var serviceProvider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true);

        using var scope = serviceProvider.CreateScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();

        await harness.Start();

        await harness.InputQueueSendEndpoint.Send<ProcessPayment>(new ProcessPayment(orderId, amount));

        bool paymentProcessedReceived = await harness.Consumed.Any<ProcessPayment>();
        bool paymentEventPublished = await harness.Published.Any<PaymentProcessed>(x => x.OrderId == orderId);

        Assert.Multiple(() =>
        {
            Assert.True(paymentProcessedReceived, "The consumer did not receive the ProcessPayment message.");
            Assert.True(paymentEventPublished, "The PaymentProcessed event was not published with the correct OrderId.");
        });
    }
}