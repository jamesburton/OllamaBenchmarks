public ProcessPaymentConsumerDefinition() : base() { } // Or just default

using MassTransit;

    global using Contracts;

    namespace Contracts
    {
        // Records
        public record ProcessPayment(Guid OrderId, decimal Amount, string Currency);
        public record PaymentProcessed(Guid OrderId, DateTime ProcessedAt);

        // Consumer
        public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
        {
            public async Task Consume(ConsumeContext<ProcessPayment> context)
            {
                if (context.Message.Amount <= 0)
                {
                    return;
                }
                await context.Publish(new PaymentProcessed(context.Message.OrderId, DateTime.UtcNow));
            }
        }

        // Consumer Definition
        public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
        {
            private int _concurrentMessageLimit = 2;

            public ProcessPaymentConsumerDefinition()
            {
                // Sets ConcurrentMessageLimit = 2 in the constructor (via field)
            }

            protected override void ConfigureConsumer(
                IReceiveEndpointConfigurator ep,
                IConsumerConfigurator<ProcessPaymentConsumer> consumer,
                IRegistrationContext ctx)
            {
                ep.UseConcurrentMessageLimit(_concurrentMessageLimit); // Using the field set in constructor
                ep.UseMessageRetry(r => r.Interval(2, 500));
            }
        }
    }