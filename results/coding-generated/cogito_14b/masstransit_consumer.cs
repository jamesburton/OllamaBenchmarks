using MassTransit;
using System;

namespace OrderProcessing
{
    public record SubmitOrder
    {
        public Guid OrderId { get; init; }
        public string CustomerName { get; init; }
        public decimal Amount { get; init; }
    }

    public record OrderSubmitted
    {
        public Guid OrderId { get; init; }
        public DateTime SubmittedAt { get; init; }
    }

    public class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            var order = context.Message;
            await context.Publish(new OrderSubmitted
            {
                OrderId = order.OrderId,
                SubmittedAt = DateTime.UtcNow
            });
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
}