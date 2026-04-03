using MassTransit;

namespace Contracts;

global using Contracts;

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

        // Publish the OrderSubmitted event
        await context.Publish(new OrderSubmitted(msg.OrderId, DateTime.UtcNow));
    }
}

// 4. SubmitOrderConsumerDefinition class
public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
{
    public SubmitOrderConsumerDefinition()
    {
        // Set ConcurrentMessageLimit = 4
        this.ConcurrentMessageLimit = 4;
    }

    protected override void ConfigureConsumer(
        IRecieveEndpointConfigurator ep,
        IConsumerConfigurator<SubmitOrderConsumer> consumer,
        IRegistrationContext ctx)
    {
        // Set retry interval
        ep.UseMessageRetry(r => r.Interval(3, 1000));
    }
}