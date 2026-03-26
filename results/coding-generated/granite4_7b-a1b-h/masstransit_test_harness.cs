using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using MassTransit.DependencyInjection.Extensions;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Test_ProcessPayment_ConsumerProcessesMessage()
    {
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        })
        .BuildServiceProvider(true);

        var harness = serviceProvider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderGuid = Guid.NewGuid();
        var amount = 100.00m;
        harness.Bus.Publish(new ProcessPayment(orderGuid, amount));

        Assert.True(await harness.Consumed.Any<ProcessPayment>());
        Assert.True(await harness.Published.Any<PaymentProcessed>(msg => msg.OrderId == orderGuid));

        await harness.Stop();
    }

    [Fact]
    public async Task Test_ProcessPayment_PublishesCorrectTransactionId()
    {
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        })
        .BuildServiceProvider(true);

        var harness = serviceProvider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderGuid = Guid.NewGuid();
        var amount = 100.00m;
        harness.Bus.Publish(new ProcessPayment(orderGuid, amount));

        Assert.True(await harness.Consumed.Any<ProcessPayment>());
        Assert.True(await harness.Published.Any<PaymentProcessed>(msg => msg.OrderId == orderGuid));

        await harness.Stop();
    }

    [Fact]
    public async Task Test_ProcessPayment_ConsumerPublishesCorrectTransactionId()
    {
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        })
        .BuildServiceProvider(true);

        var harness = serviceProvider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderGuid = Guid.NewGuid();
        var amount = 100.00m;
        harness.Bus.Publish(new ProcessPayment(orderGuid, amount));

        Assert.True(await harness.Consumed.Any<ProcessPayment>());
        Assert.True(await harness.Published.Any<PaymentProcessed>(msg => msg.OrderId == orderGuid));

        await harness.Stop();
    }
}