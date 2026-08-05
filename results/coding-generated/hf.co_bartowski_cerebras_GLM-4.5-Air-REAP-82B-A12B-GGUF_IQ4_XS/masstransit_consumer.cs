using MassTransit;

global using Contracts;

namespace Contracts;

public record SubmitOrder(Guid OrderId, string CustomerName, decimal Amount);
public record OrderSubmitted(Guid OrderId, DateTime SubmittedAt);

public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        await context.Publish(new OrderSubmitted(context.Message.OrderId, DateTime.UtcNow));
    }
}

public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
{
    public SubmitOrderConsumerDefinition()
    {
        ConcurrentMessageLimit = 4;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpoint,
        IConsumerConfigurator<SubmitOrderConsumer> consumer)
    {
        endpoint.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(1)));
    }
}