global using Contracts;

namespace Contracts
{
    using MassTransit;
    using System;

    // 1. Event Records
    public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
    public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
    public record OrderCompleted(Guid OrderId);

    // 2. Saga State Machine Instance
    public class OrderState : SagaStateMachineInstance
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
            // Initialize states
            Initially(
                When(OrderSubmitted)
                    .Then(context =>
                    {
                        // Set saga properties
                        context.Saga.OrderDate = context.Message.OrderDate;
                        context.Saga.CurrentState = nameof(Submitted);
                    })
                    .TransitionTo(Submitted)
            );

            During(Submitted,
                When(OrderAccepted)
                    .Then(context =>
                    {
                        // Set saga properties
                        context.Saga.AcceptedAt = context.Message.AcceptedAt;
                        context.Saga.CurrentState = nameof(Accepted);
                    })
                    .TransitionTo(Accepted)
            );

            During(Accepted,
                When(OrderCompleted)
                    .Finalize()
            );

            // Finalization step
            SetCompletedWhenFinalized();
        }
    }
}