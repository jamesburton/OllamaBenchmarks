global using Contracts;

using MassTransit;

namespace Contracts
{
    public record SubmitOrder(Guid OrderId, string CustomerName, decimal Amount);
    public record OrderSubmitted(Guid OrderId, DateTime SubmittedAt);

    public class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            var orderSubmitted = new OrderSubmitted(context.Message.OrderId, DateTime.UtcNow);
            await context.Publish(orderSubmitted);
        }
    }

    public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
    {
        public SubmitOrderConsumerDefinition() : base(typeof(SubmitOrderConsumer))
        {
            ConcurrentMessageLimit = 4;
            ep.UseMessageRetry(r => r.Interval(3, 1000));
        }
    }
}