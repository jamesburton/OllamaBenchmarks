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
        await context.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
    }
}

public class MassTransitPipelineTests
{
    [Fact]
    public async Task PlaceOrderConsumer_Should_Consume_And_Publish_OrderPlaced()
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

        var orderId = Guid.NewGuid();
        var customerName = "John Doe";
        await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipeline_Should_Consume_PlaceOrder_And_Publish_CustomerNotified()
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

        var orderId = Guid.NewGuid();
        var customerName = "Jane Doe";
        await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Consumed.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());
    }
}