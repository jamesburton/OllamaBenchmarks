global using Contracts;

namespace Contracts
{
    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }

    public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
    public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
    public record OrderCompleted(Guid OrderId);

    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public State Submitted { get; private set; }
        public State Accepted { get; private set; }
        public State Completed { get; private set; }

        public Event<OrderSubmitted> OrderSubmittedEvent { get; private set; }
        public Event<OrderAccepted> OrderAcceptedEvent { get; private set; }
        public Event<OrderCompleted> OrderCompletedEvent { get; private set; }

        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderSubmittedEvent, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => OrderAcceptedEvent, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => OrderCompletedEvent, x => x.CorrelateById(m => m.Message.OrderId));

            Initially(
                When(OrderSubmittedEvent)
                    .TransitionTo(Submitted)
                    .Then(context =>
                    {
                        context.Saga.OrderDate = context.Message.OrderDate;
                    }));

            During(Submitted,
                When(OrderAcceptedEvent)
                    .TransitionTo(Accepted)
                    .Then(context =>
                    {
                        context.Saga.AcceptedAt = context.Message.AcceptedAt;
                    }));

            During(Accepted,
                When(OrderCompletedEvent)
                    .Finalize()
                    .Then(context =>
                    {
                        context.Saga.CurrentState = Completed.ToString();
                    }));

            SetCompletedWhenFinalized();
        }
    }
}