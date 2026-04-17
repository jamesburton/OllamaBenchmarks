using System;
using MassTransit;

namespace Contracts;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Submitted { get; }
    public State Accepted { get; }
    public State Completed { get; }

    public Event<OrderSubmitted> OrderSubmitted { get; }
    public Event<OrderAccepted> OrderAccepted { get; }
    public Event<OrderCompleted> OrderCompleted { get; }

    public OrderStateMachine()
    {
        Submitted = new State("Submitted");
        Accepted = new State("Accepted");
        Completed = new State("Completed");

        OrderSubmitted = new Event<OrderSubmitted>();
        OrderAccepted = new Event<OrderAccepted>();
        OrderCompleted = new Event<OrderCompleted>();

        InstanceState(x => x.CurrentState);

        // Initial state setup
        Initially(
            When(OrderSubmitted)
                .TransitionTo(Submitted)
                .Then(context =>
                {
                    var msg = context.Message;
                    context.Saga.OrderDate = msg.OrderDate;
                })
        );

        // Transitions during Submitted state
        During(Submitted,
            When(OrderAccepted)
                .TransitionTo(Accepted)
                .Then(context =>
                {
                    var msg = context.Message;
                    context.Saga.AcceptedAt = msg.AcceptedAt;
                })
        );

        // Transition to Completed state
        During(Accepted,
            When(OrderCompleted)
                .TransitionTo(Completed)
                .Finalize()
        );

        // Finalization logic (optional, but good practice if needed)
        SetCompletedWhenFinalized();
    }
}