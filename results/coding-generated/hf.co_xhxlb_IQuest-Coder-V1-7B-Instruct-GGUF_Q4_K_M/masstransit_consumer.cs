using System;
using System.Threading.Tasks;
using MassTransit;
using Contracts;

namespace Contracts
{
    public record SubmitOrder(Guid OrderId, string CustomerName, decimal Amount);
    public record OrderSubmitted(Guid OrderId, DateTime SubmittedAt);
}

namespace Contracts
{
    public record OrderState(Guid CorrelationId, string CurrentState);
}

namespace Contracts
{
    public record OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            InstanceIdExpression = x => x.MessageId;
            InstanceIdType = typeof(Guid);

            Event(() => OrderSubmitted, x => x.CorrelateById(CorrelationId => OrderCorrelationId));
            InstanceId = OrderCorrelationId;

            Initially(
                When(OrderSubmitted)
                    .TransitionTo(Processing));
            During(Processing,
                When(OrderShipped)
                    .Finalize());
        }

        public State Processing { get; private set; }
        public Event<OrderSubmitted> OrderSubmitted { get; private set; }
        public Event<OrderShipped> OrderShipped { get; private set; }
    }
}

namespace Contracts
{
    public class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            var msg = context.Message;
            await context.Publish(new OrderSubmitted(msg.OrderId, DateTime.UtcNow));
        }
    }
}

namespace Contracts
{
    public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
    {
        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator ep,
            IConsumerConfigurator<SubmitOrderConsumer> consumer,
            IRegistrationContext ctx)
        {
            ep.UseMessageRetry(r => r.Interval(3, 1000));
            ep.ConcurrentMessageLimit = 4;
        }
    }
}