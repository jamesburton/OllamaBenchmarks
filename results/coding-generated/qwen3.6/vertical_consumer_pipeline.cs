global using Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Contracts;

public record PlaceOrder(Guid OrderId, string CustomerName);
public record OrderPlaced(Guid OrderId, string CustomerName);
public record CustomerNotified(Guid OrderId, string NotificationMessage);

public class PlaceOrderConsumer : IConsumer<PlaceOrder>
{
    public Task Consume(ConsumeContext<PlaceOrder> context)
    {
        return context.Publish(new OrderPlaced(context.Message.OrderId, context.Message.CustomerName));
    }
}

public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public Task Consume(ConsumeContext<OrderPlaced> context)
    {
        return context.Publish(new CustomerNotified(
            context.Message.OrderId,
            $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"));
    }
}

public class OrderPipelineTests
{
    private readonly ITestHarness _harness;
    private readonly IBus _bus;

    public OrderPipelineTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
            cfg.UsingInMemory((ctx, ep) => ep.ConfigureEndpoints(ctx));
        });
        var provider = services.BuildServiceProvider();
        _harness = provider.GetRequiredService<ITestHarness>();
        _bus = provider.GetRequiredService<IBus>();
    }

    [Fact]
    public async Task PlaceOrderConsumer_should_consume_and_publish_OrderPlaced()
    {
        await _harness.Start();
        var orderId = Guid.NewGuid();
        await _bus.Publish(new PlaceOrder(orderId, "Alice"));

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task Full_pipeline_should_consume_and_publish_both_events()
    {
        await _harness.Start();
        var orderId = Guid.NewGuid();
        await _bus.Publish(new PlaceOrder(orderId, "Bob"));

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }
}