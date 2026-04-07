global using Contracts;

namespace Contracts;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
public record OrderCompleted(Guid OrderId);

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Submitted { get; private set; } = null!;
    public State Accepted { get; private set; } = null!;
    public State Completed { get; private set; } = null!;

    public Event<OrderSubmitted> OrderSubmittedEvent { get; private set; } = null!;
    public Event<OrderAccepted> OrderAcceptedEvent { get; private set; } = null!;
    public Event<OrderCompleted> OrderCompletedEvent { get; private set; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderSubmittedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderAcceptedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderCompletedEvent, x => x.CorrelateById(m => m.Message.OrderId));

        Initially(
            When(OrderSubmittedEvent)
                .Then(context => context.Saga.OrderDate = context.Message.OrderDate)
                .TransitionTo(Submitted));

        During(Submitted,
            When(OrderAcceptedEvent)
                .Then(context => context.Saga.AcceptedAt = context.Message.AcceptedAt)
                .TransitionTo(Accepted));

        During(Accepted,
            When(OrderCompletedEvent)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}