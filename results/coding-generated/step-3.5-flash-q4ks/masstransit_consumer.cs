global using Contracts;

using MassTransit;

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
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<SubmitOrderConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Interval(3, TimeSpan.FromMilliseconds(1000)));
    }
}