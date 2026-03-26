using System;
using MassTransit;

// Event records
public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
public record OrderCompleted(Guid OrderId);

// Saga state machine instance
public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

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
        // Define states
        Submitted = SetState("Submitted");
        Accepted = SetState("Accepted");
        Completed = SetState("Completed");

        // Define events
        OrderSubmitted = Event<OrderSubmitted>();
        OrderAccepted = Event<OrderAccepted>();
        OrderCompleted = Event<OrderCompleted>();

        // Configure initial state
        Initially(
            When(OrderSubmitted)
                .Set(s => s.OrderDate = Context.Message.OrderDate)
                .TransitionTo(Submitted));

        // Configure Submitted state
        During(Submitted,
            When(OrderAccepted)
                .Set(s => s.AcceptedAt = Context.Message.AcceptedAt)
                .TransitionTo(Accepted));

        // Configure Accepted state
        During(Accepted,
            When(OrderCompleted)
                .Finalize());

        // Set completed when finalized
        SetCompletedWhenFinalized();
    }
}