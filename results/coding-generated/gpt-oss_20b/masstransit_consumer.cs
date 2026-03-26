global using Contracts;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Contracts;

public record SubmitOrder(Guid OrderId, string CustomerName, decimal Amount);

public record OrderSubmitted(Guid OrderId, DateTime SubmittedAt);

public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        var msg = context.Message;
        await context.Publish(new OrderSubmitted(msg.OrderId, DateTime.UtcNow));
    }
}

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