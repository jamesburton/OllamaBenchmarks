using System;
using System.Threading.Tasks;
using MassTransit;

global using Contracts = GlobalNamespace.Contracts;

public class ProcessPayment : ICarrier<ProcessPayment>
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }

    public static readonly ICarrier<IMessage, ProcessPayment> Message = new();
}

global using Contracts = GlobalNamespace.Contracts;

public class PaymentProcessed : ICarrier<PaymentProcessed>
{
    public Guid OrderId { get; set; }
    public DateTime ProcessedAt { get; set; }

    public static readonly ICarrier<IMessage, PaymentProcessed> Message = new();
}

namespace Contracts
{
    using MassTransit;

    [Serializable]
    public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
    {
        public Task Consume(ConsumeContext<ProcessPayment> context)
        {
            if (context.Message.Amount <= 0) return;

            var paymentProcessedEvent = new PaymentProcessed { OrderId = context.Message.OrderId, ProcessedAt = DateTime.UtcNow };
            context.Publish(paymentProcessedEvent);
            return Task.CompletedTask;
        }
    }

    [Serializable]
    public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
    {
        private readonly int _concurrentMessageLimit;

        public ProcessPaymentConsumerDefinition(int concurrentMessageLimit)
        {
            ConcurrentMessageLimit = consecutive => consecutive == _concurrentMessageLimit ? this : null;
            _concurrentMessageLimit = concurrentMessageLimit;
        }

        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator ep,
            IConsumerConfigurator<ProcessPaymentConsumer> consumerConfigurator,
            IRegistrationContext ctx)
        {
                base.ConfigureConsumer(ep, consumerConfigurator, ctx);

                if (ep is not null) // Check for non-null to avoid NullReferenceException
                {
                    ConsumerConfiguration configuration = consumerConfigurator.Current;

                    ep.UseMessageRetry(r => r.Interval(2, 500));
                }
            }
    }

    [Serializable]
    public class OrderPlaced : ICarrier<OrderPlaced>
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }

    [Serializable]
    public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
    {
        private readonly int _concurrentMessageLimit;

        public ProcessPaymentConsumerDefinition(int concurrentMessageLimit)
        {
            ConcurrentMessageLimit = consecutive => consecutive == _concurrentMessageLimit ? this : null;
            _concurrentMessageLimit = concurrentMessageLimit;
        }

        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator ep,
            IConsumerConfigurator<ProcessPaymentConsumer> consumerConfigurator,
            IRegistrationContext ctx)
        {
                base.ConfigureConsumer(ep, consumerConfigurator, ctx);

                if (ep is not null) // Check for non-null to avoid NullReferenceException
                {
                    ConsumerConfiguration configuration = consumerConfigurator.Current;

                    ep.UseMessageRetry(r => r.Interval(2, 500));
                }
            }
    }

}