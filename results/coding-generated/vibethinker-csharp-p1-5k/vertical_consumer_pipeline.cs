public record PlaceOrder(Guid OrderId, string CustomerName);
public record OrderPlaced Guid OrderId, string CustomerName;
public record CustomerNotified Guid OrderId, string NotificationMessage;

namespace Contracts;

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
        await context.publish(new CustomerNotified(context.Message.OrderId, $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}");
    }
}

using MassTransit;
using MassTransit.Testing;

public void TestPlaceOrderConsumer()