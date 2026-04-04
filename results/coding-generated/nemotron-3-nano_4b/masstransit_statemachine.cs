namespace Contracts
{
global using Contracts;

public record OrderSubmitted(Guid OrderId, DateTime OrderDate);

public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);

public record OrderCompleted(Guid OrderId);

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "Uninitialized";
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
        InstanceState(x => x.CurrentState);
        InstanceState(x => x.CorrelationId);
        InstanceState(x => x.OrderDate);
        InstanceState(x => x.AcceptedAt);

        Event(OrderSubmitted, x => x.CorrelateById(m => m.OrderId));
        Event(OrderAccepted, x => x.CorrelateById(m => m.OrderId));
        Event(OrderCompleted, x => x.CorrelateById(m => m.OrderId));

        Initially(event: OrderSubmitted,
            s => s.OrderDate = context.Message.OrderDate,
            s => s.CurrentState = "Submitted",
            s => s.CorrelationId = context.Message.OrderId,
            t => t.Submitted);
        During(Submitted,
            event: OrderAccepted,
            s => s.AcceptedAt = context.Message.AcceptedAt,
            s => s.CurrentState = "Accepted",
            t => t.Accepted);
        During(Accepted,
            event: OrderCompleted,
            s => s.CurrentState = "Completed",
            t => t.Completed);
        WhenEvent(OrderCompleted, (_, s) => s.CurrentState = "Completed")
            .SetNextState(Completed)
            .SetCompletedWhenFinalized());
        During(Completed, event: null, s => s.CurrentState = "Completed");
    }
}
}