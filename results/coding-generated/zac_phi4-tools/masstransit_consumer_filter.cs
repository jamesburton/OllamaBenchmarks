global using Contracts;

using MassTransit;

namespace Contracts
{
    public record ProcessPayment(Guid OrderId, decimal Amount, string Currency);

    public record PaymentProcessed(Guid OrderId, DateTime ProcessedAt);

    public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
    {
        public async Task Consume(ConsumeContext<ProcessPayment> context)
        {
            var message = context.Message;

            if (message.Amount <= 0)
            {
                return;
            }

            await context.Publish(new PaymentProcessed(message.OrderId, DateTime.UtcNow));
        }
    }

    public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
    {
        public ProcessPaymentConsumerDefinition()
        {
            ConcurrentMessageLimit = 2;
        }

        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator endpointConfig,
            IConsumerConfigurator<ProcessPaymentConsumer> consumerConfig,
            IRegistrationContext context)
        {
            endpointConfig.UseMessageRetry(r => r.Interval(2, 500));
        }
    }
}