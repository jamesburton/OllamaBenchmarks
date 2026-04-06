using MassTransit;
using System;

// Makes the Contracts namespace globally visible to other files
global using Contracts;

namespace Contracts
{
    // Message records using positional syntax
    public record ProcessPayment(Guid OrderId, decimal Amount, string Currency);
    public record PaymentProcessed(Guid OrderId, DateTime ProcessedAt);

    // Consumer implementation
    public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
    {
        public async Task Consume(ConsumeContext<ProcessPayment> context)
        {
            // Do nothing if amount is zero or negative
            if (context.Message.Amount <= 0) return;

            // Publish the processed event
            await context.Publish(
                new PaymentProcessed(context.Message.OrderId, DateTime.UtcNow));
        }
    }

    // Consumer definition with retry configuration
    public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
    {
        public ProcessPaymentConsumerDefinition()
        {
            // Limit concurrent message processing to 2 instances
            ConcurrentMessageLimit = 2;
        }

        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator ep,
            IConsumerConfigurator<ProcessPaymentConsumer> consumer,
            IRegistrationContext ctx)
        {
            // Configure a simple retry policy: 2 retries with 500ms delay
            ep.UseMessageRetry(r => r.Interval(2, 500));
        }
    }
}