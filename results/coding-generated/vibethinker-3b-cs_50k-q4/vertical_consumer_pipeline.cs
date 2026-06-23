using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.Logging;

namespace Contracts;

public record PlaceOrder(Guid OrderId, string CustomerName);
public record OrderPlaced Guid OrderId, string CustomerName;
public record CustomerNotified Guid OrderId, string NotificationMessage;

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
        var notificationMessage = $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}";
        await context.publish(new CustomerNotified(context.Message.OrderId, notificationMessage));
    }
}

public class Test harness
{
    public void Test() => TestPipeline();
}