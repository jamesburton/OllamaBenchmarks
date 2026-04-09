global using Contracts;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Contracts
{
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);
}

public class PlaceOrderConsumer : IConsumer<PlaceOrder>
{
    public async Task Consume(ConsumeContext<PlaceOrder> context)
    {
        var msg = context.Message;
        await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
    }
}

public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var msg = context.Message;
        await context.Publish(new CustomerNotified(
            msg.OrderId,
            $"Order {msg.OrderId} confirmed for {msg.CustomerName}"
        ));
    }
}

public class PlaceOrderConsumerTests : IAsyncLifetime
{
    private readonly ITestHarness _harness;

    public PlaceOrderConsumerTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        _harness = services.BuildServiceProvider().GetRequiredService<ITestHarness>();
    }

    public async ValueTask InitializeAsync() => await _harness.Start();

    public async ValueTask DisposeAsync() => await _harness.Stop();

    [Fact]
    public async Task PlaceOrder_ConsumedAndOrderPlacedPublished()
    {
        var orderId = Guid.NewGuid();
        var customerName = "Alice";

        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipeline_OrderPlacedAndCustomerNotifiedPublished()
    {
        var orderId = Guid.NewGuid();
        var customerName = "Bob";

        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }
}