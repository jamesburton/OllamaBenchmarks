global using Contracts;

using System;
using MassTransit;

namespace Contracts;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = default!;

    public DateTime OrderDate { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

// Event records
public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
public record OrderCompleted(Guid OrderId);

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Submitted { get; private set; } = default!;
    public State Accepted { get; private set; } = default!;
    public State Completed { get; private set; } = default!;

    public Event<OrderSubmitted> OrderSubmitted { get; private set; } = default!;
    public Event<OrderAccepted> OrderAccepted { get; private set; } = default!;
    public Event<OrderCompleted> OrderCompleted { get; private set; } = default!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderAccepted,  x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderCompleted, x => x.CorrelateById(m => m.Message.OrderId));

        Initially(
            When(OrderSubmitted)
                .Then(context => { context.Saga.OrderDate = context.Message.OrderDate; })
                .TransitionTo(Submitted));

        During(Submitted,
            When(OrderAccepted)
                .Then(context => { context.Saga.AcceptedAt = context.Message.AcceptedAt; })
                .TransitionTo(Accepted));

        During(Accepted,
            When(OrderCompleted)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}