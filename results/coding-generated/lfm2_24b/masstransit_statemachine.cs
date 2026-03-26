global using Contracts;
using MassTransit;
using System;

namespace Contracts
{
    // Saga State
    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }

    // Event records
    public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
    public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
    public record OrderCompleted(Guid OrderId);

    // State machine
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public State Submitted { get; private set; }
        public State Accepted { get; private set; }
        public State Completed { get; private set; }

        public Event<OrderSubmitted> OrderSubmitted { get; private set; }
        public Event<OrderAccepted> OrderAccepted { get; private set; }
        public Event<OrderCompleted> OrderCompleted { get; private set; }

        public OrderStateMachine()
        {
            // Set the saga instance property for correlation
            InstanceState(x => x.CurrentState);

            // Define states
            Submitted = State.AwaitingAcceptance;
            Accepted = State.Completed;
            Completed = State.Final;

            // Initially: OrderSubmitted → Submitted
            When(OrderSubmitted)
                .Then(context =>
                {
                    context.Saga.OrderDate = context.Message.OrderDate;
                    context.Instance.CorrelationId = context.Message.OrderId;
                })
                .TransitionTo(Submitted);

            // During Submitted: OrderAccepted → Accepted
            When(OrderAccepted)
                .Then(context =>
                {
                    context.Saga.AcceptedAt = context.Message.AcceptedAt;
                })
                .TransitionTo(Accepted);

            // During Accepted: OrderCompleted → Finalize
            When(OrderCompleted)
                .Finalize();

            // Finalize when final event is received
            During(Accepted,
                When(OrderCompleted)
                    .Finalize()
                    .Then(context => context.Publish(new OrderCompleted(context.Saga.Instance.OrderId))));

            // Set completion when finalized
            SetCompletedWhenFinalized();
        }
    }
}