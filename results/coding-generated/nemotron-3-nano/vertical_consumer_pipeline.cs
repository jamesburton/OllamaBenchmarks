using System;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Contracts;

public record PlaceOrder(Guid OrderId, string CustomerName);
public record OrderPlaced(Guid OrderId, string CustomerName);
public record CustomerNotified(Guid OrderId, string NotificationMessage);

public record PlaceOrderConsumer() : IConsumer<PlaceOrder>
{
    public async Task Consume(ConsumeContext<PlaceOrder> context)
    {
        await context.Publish(new OrderPlaced(context.Message.OrderId, context.Message.CustomerName));
    }
}

public record NotifyCustomerConsumer() : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        await context.Publish(new CustomerNotified(context.Message.OrderId,
            $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"));
    }
}

public class PlaceOrderPipelineTests
{
    [Fact]
    public async Task PlaceOrder_consumer_publishes_OrderPlaced()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<PlaceOrderConsumer>();
        });

        var provider = services.BuildServiceProvider();

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var msg = new PlaceOrder(orderId, "Alice");

        await harness.Bus.Publish(msg);
        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
    }

    [Theory, MemberData(nameof(PipelineData))]
    public async Task Full_pipeline_publishes_OrderPlaced_and_CustomerNotified(OrderPlaced order, CustomerNotified notified)
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

        var msg = new PlaceOrder(order.OrderId, order.CustomerName);
        await harness.Bus.Publish(msg);

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());
    }

    public static IEnumerable<object[]> PipelineData =>
        new[]
        {
            new object[] {
                new OrderPlaced(Guid.NewGuid(), "Bob"),
                new CustomerNotified(Guid.NewGuid(), "Order confirmed")
            }
        };
}