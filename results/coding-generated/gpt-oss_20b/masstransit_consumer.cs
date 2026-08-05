global using Contracts;

using MassTransit;

namespace Contracts;

// Message record for submitting an order
public record SubmitOrder(Guid OrderId, string CustomerName, decimal Amount);

// Event record for when an order has been submitted
public record OrderSubmitted(Guid OrderId, DateTime SubmittedAt);

// Consumer that handles SubmitOrder messages
public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        var msg = context.Message;
        await context.Publish(new OrderSubmitted(msg.OrderId, DateTime.UtcNow));
    }
}

// Consumer definition that configures retry and concurrency
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