using System;
using System.Threading.Tasks;
using MassTransit;

public class SubmitOrder
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; }
    public decimal Amount { get; set; }
}

public event EventHandler<OrderSubmitted> Submitted;

public class OrderSubmitted : EventArgs
{
    public Guid OrderId { get; private set; }
    public DateTime SubmittedAt { get; }

    public OrderSubmitted(Guid orderId)
    {
        OrderId = orderId;
        SubmittedAt = DateTime.UtcNow;
    }
}

public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        var order = context.Message;
        await context.Publish(new OrderSubmitted(order.OrderId));
    }
}

public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator ep,
        IConsumerConfigurator<SubmitOrderConsumer> consumer)
    {
        base.ConfigureConsumer(ep, consumer);
        consumer.SetConcurrentMessageLimit(4);
        consumer.UseMessageRetry(r => r.Interval(3000));
    }
}