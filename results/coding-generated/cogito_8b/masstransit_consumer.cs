using MassTransit;

global using Contracts;

namespace Contracts
{
    public record SubmitOrder(
        Guid OrderId,
        string CustomerName,
        decimal Amount);

    public record OrderSubmitted(
        Guid OrderId,
        DateTime SubmittedAt);

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
        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator ep,
            IConsumerConfigurator<SubmitOrderConsumer> consumer,
            IRegistrationContext ctx)
        {
            ep.UseMessageRetry(r => r.Interval(3, 1000));
        }

        public SubmitOrderConsumerDefinition()
        {
            ConcurrentMessageLimit = 4;
        }
    }
}