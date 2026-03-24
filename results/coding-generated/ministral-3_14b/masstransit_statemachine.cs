using MassTransit;
using System;

public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
public record OrderCompleted(Guid OrderId);

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

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
        InstanceState(x => x.CurrentState);

        Event(() => OrderSubmitted, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => OrderAccepted, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => OrderCompleted, x => x.CorrelateById(context => context.Message.OrderId));

        Initially(
            When(OrderSubmitted)
                .Then(context =>
                {
                    context.Instance.OrderDate = context.Message.OrderDate;
                })
                .TransitionTo(Submitted));

        During(Submitted,
            When(OrderAccepted)
                .Then(context =>
                {
                    context.Instance.AcceptedAt = context.Message.AcceptedAt;
                })
                .TransitionTo(Accepted));

        During(Accepted,
            When(OrderCompleted)
                .TransitionTo(Completed));

        SetCompletedWhenFinalized();
    }
}