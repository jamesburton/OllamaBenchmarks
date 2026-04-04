using MassTransit;
using System;
using System.Threading.Tasks;

// Global using statement to make types accessible without explicit using directives
global using Contracts;

namespace Contracts;

// 1. Message Record
public record SubmitOrder(Guid OrderId, string CustomerName, decimal Amount);

// 2. Event Record
public record OrderSubmitted(Guid OrderId, DateTime SubmittedAt);

// 3. Consumer Implementation
public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        var message = context.Message;

        // Publish the OrderSubmitted event
        await context.Publish(new OrderSubmitted(message.OrderId, DateTime.UtcNow));
    }
}

// 4. Consumer Definition
public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
{
    public SubmitOrderConsumerDefinition()
    {
        // Set ConcurrentMessageLimit = 4
        this.ConcurrentMessageLimit = 4;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator ep,
        IConsumerConfigurator<SubmitOrderConsumer> consumer,
        IRegistrationContext ctx)
    {
        // Set retry policy: Interval(3, 1000)
        ep.UseMessageRetry(r => r.Interval(3, 1000));
    }
}