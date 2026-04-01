global using Contracts;

namespace Contracts
{
    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }

        public string CurrentState { get; set; } = "Submitted";

        public DateTime OrderDate { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }

    public partial record OrderSubmitted(Guid OrderId, DateTime OrderDate)

    public partial record OrderAccepted(Guid OrderId, DateTime AcceptedAt)

    public partial record OrderCompleted(Guid OrderId)

    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public State Submitted { get; }
        public State Accepted { get; }
        public State Completed { get; }

        public Event<OrderSubmitted> OrderSubmittedEvent { get; }
        public Event<OrderAccepted> OrderAcceptedEvent { get; }
        public Event<OrderCompleted> OrderCompletedEvent { get; }

        public OrderStateMachine()
        {
            Submitted = State.FromDescription("Submitted");
            Accepted  = State.FromDescription("Accepted");
            Completed = State.FromDescription("Completed");

            OrderSubmittedEvent = Event(() => OrderSubmitted,
                order => order.CorrelateById(m => m.OrderId));

            OrderAcceptedEvent = Event(() => OrderAccepted,
                order => order.CorrelateById(m => m.OrderId));

            OrderCompletedEvent = Event(() => OrderCompleted,
                order => order.CorrelateById(m => m.OrderId));

            Initially(
                When(OrderSubmittedEvent)
                    .TransitionTo(Submitted)
                    .Then(context => 
                    {
                        var order = context.Message;
                        order.OrderDate = order.OrderDate;
                        context.Saga.PropertyName = "OrderDate";
                    }));

            During(Submitted,
                When(OrderAcceptedEvent)
                    .TransitionTo(Accepted)
                    .Then(context => 
                    {
                        var order = context.Message;
                        order.AcceptedAt = order.AcceptedAt;
                        context.Saga.PropertyName = "AcceptedAt";
                    }));

            During(Accepted,
                When(OrderCompletedEvent)
                    .TransitionTo(Completed)
                    .Then(context => 
                    {
                        context.Saga.Completed();
                    }));

            Completed = State.FromDescription("Completed")
                             .WhenFinalized()
                             .Then(context => context.Saga.Completed());

            End(
                When(OrderCompletionEvent)
                    .TransitionTo(Completed)
                    .Complete());
        }

        // The EventCompletionEvent is actually OrderCompletedEvent; Ensure finalization completes saga:
        Complete = When(OrderCompletedEvent)
                     .MakeCurrent(Completed)
                     .Complete();
    }
}