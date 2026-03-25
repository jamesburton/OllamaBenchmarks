using System;
using System.Threading.Tasks;
using MassTransit;

namespace Sample
{
    // Message to submit an order
    public record SubmitOrder(Guid OrderId, string CustomerName, decimal Amount);

    // Event emitted when an order is submitted
    public record OrderSubmitted(Guid OrderId, DateTime SubmittedAt);

    // Consumer that handles SubmitOrder and publishes OrderSubmitted
    public class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            var msg = context.Message;
            await context.Publish(new OrderSubmitted(msg.OrderId, DateTime.UtcNow));
        }
    }

    // Consumer definition with retry configuration and concurrency limit
    public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
    {
        public SubmitOrderConsumerDefinition()
        {
            // Limit the number of concurrent messages processed by this consumer
            ConcurrentMessageLimit = 4;
        }

        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<SubmitOrderConsumer> consumerConfigurator,
            IRegistrationContext context)
        {
            // Configure a simple retry policy: 3 attempts, 1 second interval
            endpointConfigurator.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(1)));
        }
    }
}