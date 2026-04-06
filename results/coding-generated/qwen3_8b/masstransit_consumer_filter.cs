using MassTransit;
global using Contracts;

namespace Contracts
{
    // Positional record for ProcessPayment message
    record ProcessPayment(Guid OrderId, decimal Amount, string Currency);

    // Positional record for PaymentProcessed event
    record PaymentProcessed(Guid OrderId, DateTime ProcessedAt);

    // Consumer for handling ProcessPayment messages
    public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
    {
        public async Task Consume(ConsumeContext<ProcessPayment> context)
        {
            if (context.Message.Amount <= 0)
                return;

            await context.Publish(new PaymentProcessed(
                context.Message.OrderId, 
                DateTime.UtcNow));
        }
    }

    // ConsumerDefinition for configuring retry and message limit
    public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
    {
        public ProcessPaymentConsumerDefinition()
        {
            // Set the maximum number of concurrent messages to process
            ConcurrentMessageLimit = 2;
        }

        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator ep,
            IConsumerConfigurator<ProcessPaymentConsumer> consumer,
            IRegistrationContext ctx)
        {
            // Configure message retry with 2 retries and 500ms interval
            ep.UseMessageRetry(r => r.Interval(2, TimeSpan.FromMilliseconds(500)));
        }
    }
}