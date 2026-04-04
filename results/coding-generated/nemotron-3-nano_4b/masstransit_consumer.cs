using MassTransit;
global using Contracts;

namespace Contracts
{
    // Message types
    public record SubmitOrder(Guid OrderId, string CustomerName, decimal Amount);
    public record OrderPlaced(Guid OrderId);
    public record OrderSubmitted(Guid OrderId, DateTime SubmittedAt);

    // Consumer that handles SubmitOrder messages
    public class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            await context.Publish(new OrderSubmitted(context.Message.OrderId, DateTime.UtcNow));
        }
    }

    // Consumer configuration
    public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
    {
        private SubmitOrderConsumer _default;

        public SubmitOrderConsumerDefinition()
        {
            _default = this;
            ConcurrentMessageLimit = 4;
        }

        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator ep,
            IConsumerConfigurator<SubmitOrderConsumer> consumer,
            IRegistrationContext ctx)
        {
            (_default, ep).UseMessageRetry(r => r.Interval(3, 1000));
        }

        public void Default => _default;
    }

    // Saga state machine (optional – required by sample but not used in SubmitOrder)
    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }

        public OrderState() { }
    }

    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public State Processing { get; private set; }
        public Event<OrderPlaced> OrderPlaced { get; private set; }

        public OrderStateMachine()
        {
            Initially(
                When(OrderPlaced)
                    .TransitionTo(Processing));
            During(Processing,
                When(OrderShipped)
                    .Finalize());
        }
    }
}