global using Contracts;

namespace Contracts;

using MassTransit;
using System;
using System.Threading.Tasks;

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
        InstanceState(x => x.CurrentState);

        EventSubmitted();
        EventAccepted();
        EventCompleted();

        Initially(
            When(OrderSubmittedEvent)
                .Then(context =>
                {
                    context.Saga.OrderDate = context.Message.OrderDate;
                    context.Saga.CurrentState = nameof(Submitted);
                })
                .TransitionTo(Submitted)
        );

        During(Submitted,
            When(OrderAcceptedEvent)
                .Then(context =>
                {
                    context.Saga.AcceptedAt = context.Message.AcceptedAt;
                    context.Saga.CurrentState = nameof(Accepted);
                })
                .TransitionTo(Accepted)
        );

        During(Accepted,
            When(OrderCompletedEvent)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }

    private void EventSubmitted()
    {
        OrderSubmittedEvent = Event(() => OrderSubmitted, x =>
            x.CorrelateById(m => m.Message.OrderId));
    }

    private void EventAccepted()
    {
        OrderAcceptedEvent = Event(() => OrderAccepted, x =>
            x.CorrelateById(m => m.Message.OrderId));
    }

    private void EventCompleted()
    {
        OrderCompletedEvent = Event(() => OrderCompleted, x =>
            x.CorrelateById(m => m.Message.OrderId));
    }
}