namespace Contracts;

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

        Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId))
            .Then(context => { context.Saga.OrderDate = context.Data.OrderDate; })
            .TransitionTo(Submitted);

        Event(() => OrderAccepted, x => x.CorrelateById(m => m.Message.OrderId))
            .Then(context => { context.Saga.AcceptedAt = context.Data.AcceptedAt; })
            .TransitionTo(Accepted);

        Event(() => OrderCompleted, x => x.CorrelateById(m => m.Message.OrderId))
            .Finalize();

        Initially(
            When(OrderSubmitted)
                .Then(context => { context.Saga.CurrentState = "Submitted"; })
                .TransitionTo(Submitted));

        During(Submitted,
            When(OrderAccepted)
                .Then(context => { context.Saga.CurrentState = "Accepted"; })
                .TransitionTo(Accepted));

        During(Accepted,
            When(OrderCompleted)
                .Then(context => { context.Saga.CurrentState = "Completed"; })
                .Finalize());
    }
}