global using Contracts;

namespace Contracts;

public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
public record OrderCompleted(Guid OrderId);

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
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

        OrderSubmitted = Event(() => new OrderSubmitted(Guid.NewGuid(), DateTime.UtcNow),
            x => x.CorrelateById(m => m.Message.OrderId));

        OrderAccepted = Event(() => new OrderAccepted(Guid.NewGuid(), DateTime.UtcNow),
            x => x.CorrelateById(m => m.Message.OrderId));

        OrderCompleted = Event(() => new OrderCompleted(Guid.NewGuid()),
            x => x.CorrelateById(m => m.Message.OrderId));

        Initially(
            When(OrderSubmitted)
                .TransitionTo(Submitted,
                    ctx => { ctx.Saga.OrderDate = ctx.Message.OrderDate; })
                .Then(ctx => { ctx.Saga.CurrentState = "Submitted"; }));

        During(Submitted,
            When(OrderAccepted)
                .TransitionTo(Accepted,
                    ctx => { ctx.Saga.AcceptedAt = ctx.Message.AcceptedAt; })
                .Then(ctx => { ctx.Saga.CurrentState = "Accepted"; }))
            .Then(ctx => { ctx.Saga.OrderDate = DateTime.UtcNow; });

        During(Accepted,
            When(OrderCompleted)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}