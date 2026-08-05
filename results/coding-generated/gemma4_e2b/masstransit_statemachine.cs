global using Contracts;

namespace Contracts
{
    // 1. Event Records
    public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
    public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
    public record OrderCompleted(Guid OrderId);

    // 2. Saga State Machine Instance
    public class OrderState : MassTransitStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }

    // 3. Saga State Machine
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        // States
        public State Submitted { get; private set; }
        public State Accepted { get; private set; }
        public State Completed { get; private set; }

        // Events
        public Event<OrderSubmitted> OrderSubmitted { get; private set; }
        public Event<OrderAccepted> OrderAccepted { get; private set; }
        public Event<OrderCompleted> OrderCompleted { get; private set; }

        public OrderStateMachine()
        {
            // Initial State setup
            InstanceState(x => x.CurrentState);

            // Initial transition: When OrderSubmitted event occurs
            Initially(
                When(OrderSubmitted)
                    .Then(context =>
                    {
                        // Set OrderDate property
                        context.Saga.OrderDate = context.Message.OrderDate;
                        // Transition to Submitted state
                        context.Saga.CurrentState = Submitted;
                    })
                .TransitionTo(Submitted)
            );

            // During Submitted state
            During(Submitted,
                When(OrderAccepted)
                    .Then(context =>
                    {
                        // Set AcceptedAt property
                        context.Saga.AcceptedAt = context.Message.AcceptedAt;
                        // Transition to Accepted state
                        context.Saga.CurrentState = Accepted;
                    })
                .TransitionTo(Accepted)
            );

            // During Accepted state
            During(Accepted,
                When(OrderCompleted)
                    .Finalize()
            );
        }
    }
}