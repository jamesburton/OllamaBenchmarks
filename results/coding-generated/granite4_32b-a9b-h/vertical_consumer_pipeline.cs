using MassTransit;
using MassTransit.Testing;

global using Contracts;

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
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var notificationMessage = $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}";
        await context.Publish(new CustomerNotified(context.Message.OrderId, notificationMessage));
    }
}

public class Tests
{
    [Fact]
    public async Task PlaceOrderConsumer_Publishes_OrderPlaced()
    {
        var harness = new TestHarness();
        harness.AddConsumer<PlaceOrderConsumer>();

        await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "John Doe"));

        Assert.True(await harness.Consumed.Any<OrderPlaced>());
    }

    [Fact]
    public async Task Full_Pipeline_Publishes_CustomerNotified()
    {
        var harness = new TestHarness();
        harness.AddConsumer<PlaceOrderConsumer>();
        harness.AddConsumer<NotifyCustomerConsumer>();

        await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "John Doe"));

        Assert.True(await harness.Consumed.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());
    }
}