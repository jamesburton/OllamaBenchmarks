using MassTransit;

global using Contracts;

namespace Contracts;
{
    // ProcessPayment message record
    public record ProcessPayment(Guid OrderId, decimal Amount, string Currency);

    // PaymentProcessed event record
    public record PaymentProcessed(Guid OrderId, DateTime ProcessedAt);

    // ProcessPaymentConsumer class implementing IConsumer<ProcessPayment>
    public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
    {
        public async Task Consume(ConsumeContext<ProcessPayment> context)
        {
            if (context.Message.Amount <= 0)
                return;

            await context.Publish(new PaymentProcessed(
                context.Message.OrderId,
                DateTime.UtcNow
            ));
        }
    }

    // ProcessPaymentConsumerDefinition class extending ConsumerDefinition<ProcessPaymentConsumer>
    public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
    {
        public ProcessPaymentConsumerDefinition()
        {
            // Set the concurrent message limit to 2
            ConcurrentMessageLimit = 2;
        }

        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator ep,
            IConsumerConfigurator<ProcessPaymentConsumer> consumer,
            IRegistrationContext ctx)
        {
            // Configure message retry with interval of 2 seconds, 500 ms
            ep.UseMessageRetry(r => r.Interval(2, 500));
        }
    }
}