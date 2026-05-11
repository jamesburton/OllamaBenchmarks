using MassTransit;

global using Contracts;

namespace Contracts
{
    // 1. ProcessPayment message record
    public record ProcessPayment(Guid OrderId, decimal Amount, string Currency);

    // 2. PaymentProcessed event record
    public record PaymentProcessed(Guid OrderId, DateTime ProcessedAt);

    // 3. ProcessPaymentConsumer class
    public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
    {
        public async Task Consume(ConsumeContext<ProcessPayment> context)
        {
            var msg = context.Message;

            // Check if Amount <= 0, return immediately if true
            if (msg.Amount <= 0)
            {
                return;
            }

            // Otherwise, publish the PaymentProcessed event
            await context.Publish(new PaymentProcessed(msg.OrderId, DateTime.UtcNow));
        }
    }

    // 4. ProcessPaymentConsumerDefinition class
    public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
    {
        public ProcessPaymentConsumerDefinition()
        {
            // Set ConcurrentMessageLimit = 2 in the constructor
            this.ConcurrentMessageLimit = 2;
        }

        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator ep,
            IConsumerConfigurator<ProcessPaymentConsumer> consumer,
            IRegistrationContext ctx)
        {
            // Call ep.UseMessageRetry(r => r.Interval(2, 500))
            ep.UseMessageRetry(r => r.Interval(2, 500));
        }
    }
}