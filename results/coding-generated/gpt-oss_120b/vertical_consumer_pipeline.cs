global using Contracts;
using MassTransit;

namespace Contracts;

public record PlaceOrder(Guid OrderId, string CustomerName);
public record OrderPlaced(Guid OrderId, string CustomerName);
public record CustomerNotified(Guid OrderId, string NotificationMessage);

public class PlaceOrderConsumer : IConsumer<PlaceOrder>
{
    public async Task Consume(ConsumeContext<PlaceOrder> context)
    {
        await context.Publish(new OrderPlaced(context.Message.OrderId, context.Message.CustomerName));
    }
}

public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var msg = context.Message;
        var notification = $"Order {msg.OrderId} confirmed for {msg.CustomerName}";
        await context.Publish(new CustomerNotified(msg.OrderId, notification));
    }
}

using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class OrderPipelineTests : IAsyncLifetime
{
    private ITestHarness _harness = null!;
    private IBus _bus = null!;

    public async ValueTask InitializeAsync()
    {
        var services = new ServiceCollection();

        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        var provider = services.BuildServiceProvider(true);
        _harness = provider.GetRequiredService<ITestHarness>();
        _bus = provider.GetRequiredService<IBus>();

        await _harness.Start();
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.Stop();
    }

    [Fact]
    public async Task PlaceOrder_Should_Consume_And_Publish_OrderPlaced()
    {
        var orderId = Guid.NewGuid();

        await _bus.Publish(new PlaceOrder(orderId, "Alice"));

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipeline_Should_Publish_OrderPlaced_And_CustomerNotified()
    {
        var orderId = Guid.NewGuid();

        await _bus.Publish(new PlaceOrder(orderId, "Bob"));

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }
}