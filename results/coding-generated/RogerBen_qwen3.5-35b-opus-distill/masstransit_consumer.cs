using System;
using MassTransit;

// 1. SubmitOrder message record
public record SubmitOrder(Guid OrderId, string CustomerName, decimal Amount);

// 2. OrderSubmitted event record
public record OrderSubmitted(Guid OrderId, DateTime SubmittedAt);

// 3. SubmitOrderConsumer class
public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        var msg = context.Message;
        await context.Publish(new OrderSubmitted(msg.OrderId, DateTime.UtcNow));
    }
}

// 4. SubmitOrderConsumerDefinition class
public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
{
    public SubmitOrderConsumerDefinition()
    {
        ConcurrentMessageLimit = 4;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator ep,
        IConsumerConfigurator<SubmitOrderConsumer> consumer,
        IRegistrationContext ctx)
    {
        ep.UseMessageRetry(r => r.Interval(3, 1000));
    }
}