using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

global using Contracts;

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
        var message = $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}";
        await context.Publish(new CustomerNotified(context.Message.OrderId, message));
    }
}

public class PipelineTests
{
    [Fact]
    public async Task PlaceOrder_Consumed_And_OrderPlaced_Published()
    {
        var provider = new ServiceCollection();
        provider.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        var harness = provider.BuildServiceProvider().GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new PlaceOrder(orderId, "Alice"));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task Full_Pipeline_PlaceOrder_To_CustomerNotified()
    {
        var provider = new ServiceCollection();
        provider.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        var harness = provider.BuildServiceProvider().GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new PlaceOrder(orderId, "Bob"));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());
    }
}