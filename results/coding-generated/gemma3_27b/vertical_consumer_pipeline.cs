using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

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
        string notificationMessage = $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}";
        await context.Publish(new CustomerNotified(context.Message.OrderId, notificationMessage));
    }
}

public class ConsumerTests
{
    [Fact]
    public async Task PlaceOrderConsumer_Publishes_OrderPlaced()
    {
        var harness = new MassTransitTestHarness();
        harness.Configure(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
        });

        await harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "Test Customer";
        var placeOrder = new PlaceOrder(orderId, customerName);

        await harness.Bus.Publish(placeOrder);

        Assert.True(await harness.Published.Any<OrderPlaced>());
        var publishedMessage = harness.Published.First<OrderPlaced>();
        Assert.Equal(orderId, publishedMessage.OrderId);
        Assert.Equal(customerName, publishedMessage.CustomerName);
    }

    [Fact]
    public async Task FullPipeline_PlaceOrder_Publishes_OrderPlaced_And_CustomerNotified()
    {
        var harness = new MassTransitTestHarness();
        harness.Configure(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        await harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "Test Customer";
        var placeOrder = new PlaceOrder(orderId, customerName);

        await harness.Bus.Publish(placeOrder);

        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());

        var orderPlaced = harness.Published.First<OrderPlaced>();
        Assert.Equal(orderId, orderPlaced.OrderId);
        Assert.Equal(customerName, orderPlaced.CustomerName);

        var customerNotified = harness.Published.First<CustomerNotified>();
        Assert.Equal(orderId, customerNotified.OrderId);
        Assert.Equal($"Order {orderId} confirmed for {customerName}", customerNotified.NotificationMessage);
    }
}