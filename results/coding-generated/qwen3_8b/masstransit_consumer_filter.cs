using MassTransit;
global using Contracts;

namespace Contracts
{
    // Positional records for messages and events
    public record ProcessPayment(Guid OrderId, decimal Amount, string Currency);
    public record PaymentProcessed(Guid OrderId, DateTime ProcessedAt);

    // Consumer that handles ProcessPayment messages
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

    // Consumer definition with retry configuration
    public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
    {
        public ProcessPaymentConsumerDefinition()
        {
            ConcurrentMessageLimit = 2;
        }

        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator ep,
            IConsumerConfigurator<ProcessPaymentConsumer> consumer,
            IRegistrationContext ctx)
        {
            ep.UseMessageRetry(r => r.Interval(2, 500));
        }
    }
}