using MassTransit;
using System;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
public record OrderCompleted(Guid OrderId);

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Initially(
            When(OrderSubmitted)
                .SetSaga(s => s.OrderDate = context.Message.OrderDate)
                .TransitionTo(Submitted));

        During(Submitted,
            When(OrderAccepted)
                .SetSaga(s => s.AcceptedAt = context.Message.AcceptedAt)
                .TransitionTo(Accepted));

        During(Accepted,
            When(OrderCompleted)
                .Finalize());

        SetCompletedWhenFinalized();
    }

    public State Submitted { get; private set; }
    public State Accepted { get; private set; }
    public State Completed { get; private set; }

    public Event<OrderSubmitted> OrderSubmitted { get; private set; }
    public Event<OrderAccepted> OrderAccepted { get; private set; }
    public Event<OrderCompleted> OrderCompleted { get; private set; }
}