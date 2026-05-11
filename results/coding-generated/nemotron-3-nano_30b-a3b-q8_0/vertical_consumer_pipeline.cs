using MassTransit;
using Contracts;

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
        var msg = $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}";
        await context.Publish(new CustomerNotified(context.Message.OrderId, msg));
    }
}

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class OrderPipelineTests
{
    [Fact]
    public async Task PlaceOrder_is_consumed_and_OrderPlaced_published()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();
        await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Customer"));

        Assert.True(harness.Consumed.Any<OrderPlaced>());
        Assert.True(harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipeline_verified()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();
        await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Alice"));

        Assert.True(harness.Consumed.Any<OrderPlaced>());
        Assert.True(harness.Published.Any<OrderPlaced>());
        Assert.True(harness.Published.Any<CustomerNotified>());
    }
}