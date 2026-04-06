namespace Contracts;

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
    public State Submitted { get; private set; }
    public State Accepted { get; private set; }
    public State Completed { get; private set; }

    public Event<OrderSubmitted> OrderSubmitted { get; private set; }
    public Event<OrderAccepted> OrderAccepted { get; private set; }
    public Event<OrderCompleted> OrderCompleted { get; private set; }

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        var submitted = Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));
        var accepted = Event(() => OrderAccepted, x => x.CorrelateById(m => m.Message.OrderId));
        var completed = Event(() => OrderCompleted, x => x.CorrelateById(m => m.Message.OrderId));

        Submitted = State.Status.Pending.WithEquals(submitted);
        Accepted = State.Status.Pending.WithEquals(accepted);
        Completed = State.Status.Pending.WithEquals(completed);

        Initially(
            When(OrderSubmitted)
                .TransitionTo(Submitted)
                .Then(context => { context.Saga.OrderDate = context.Message.OrderDate; }));
        During(Submitted,
            When(OrderAccepted)
                .TransitionTo(Accepted)
                .Then(context => { context.Saga.AcceptedAt = context.Message.AcceptedAt; }));
        During(Accepted,
            When(OrderCompleted)
                .Finalize()
                .Then(context => { context.Saga.CurrentState = "Completed"; }));
    }
}