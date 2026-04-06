global using Contracts;

namespace Contracts;

using System;
using MassTransit;
using MassTransit.StateMachine;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
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
        // Set the property that holds the current state
        InstanceState(x => x.CurrentState);

        Submitted = State.Submitted;
        Accepted = State.Accepted;
        Completed = State.Completed;

        // Define events and correlation
        Event<OrderSubmitted>(x => x.CorrelateById(m => m.Message.OrderId));
        Event<OrderAccepted>(x => x.CorrelateById(m => m.Message.OrderId));
        Event<OrderCompleted>(x => x.CorrelateById(m => m.Message.OrderId));

        // Initial transition: OrderSubmitted -> Submitted
        Initially(
            When<OrderSubmitted>()
                .Then(context =>
                {
                    context.Saga.OrderDate = context.Data.OrderDate;
                })
                .TransitionTo(Submitted)
        );

        // While in Submitted, receive OrderAccepted -> Accepted
        During(Submitted,
            When<OrderAccepted>()
                .Then(context =>
                {
                    context.Saga.AcceptedAt = context.Data.AcceptedAt;
                })
                .TransitionTo(Accepted)
        );

        // While in Accepted, receive OrderCompleted -> Finalize
        During(Accepted,
            When<OrderCompleted>()
                .Finalize()
        );

        // Mark the saga as completed when finalized
        SetCompletedWhenFinalized();
    }
}