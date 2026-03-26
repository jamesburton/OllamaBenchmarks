using System;
using System.Threading.Tasks;
using MassTransit;

global using Contracts = GlobalNamespace.Contracts;

namespace Contracts
{
    public record SubmitOrder(Guid OrderId, string CustomerName, decimal Amount);

    public record OrderSubmitted(Guid OrderId, DateTime SubmittedAt);
}

public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    private readonly IBus _bus;

    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        var order = context.Message;
        await _bus.Publish(new OrderSubmitted(order.OrderId, DateTime.UtcNow));
    }
}

public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
{
    private readonly int ConcurrentMessageLimit;

    public SubmitOrderConsumerDefinition(int concurrentMessageLimit) => ConcurrentMessageLimit = concurrentMessageLimit ?? throw new ArgumentNullException(nameof(concurrentMessageLimit));

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator ep,
        IConsumerConfigurator<SubmitOrderConsumer> consumer,
        IRegistrationContext ctx)
    {
        base.ConfigureConsumer(ep, consumer, ctx);

        if (ConcurrentMessageLimit > 0) // Set ConcurrentMessageLimit only for non-zero values
            ep.UseMessageRetry(r => r.Interval(3, 1000));

        Consumer.SetMaxConcurrency(ConcurrentMessageLimit); // Apply the concurrency limit to this specific consumer instance

        base.ConfigureConsumer(ep, consumer, ctx);
    }
}