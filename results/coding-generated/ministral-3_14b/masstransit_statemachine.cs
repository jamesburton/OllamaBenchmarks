global using Contracts;

namespace Contracts;

file record OrderSubmitted(Guid OrderId, DateTime OrderDate);
file record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
file record OrderCompleted(Guid OrderId);

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

        Event(() => OrderSubmitted,
            x => x.CorrelateById(m => m.OrderId)
                .Then(context => { context.Saga.OrderDate = context.Message.OrderDate; }));

        Event(() => OrderAccepted,
            x => x.CorrelateById(m => m.OrderId)
                .Then(context => { context.Saga.AcceptedAt = context.Message.AcceptedAt; }));

        Event(() => OrderCompleted,
            x => x.CorrelateById(m => m.OrderId));

        Initially(
            When(OrderSubmitted)
                .TransitionTo(Submitted));

        During(Submitted,
            When(OrderAccepted)
                .TransitionTo(Accepted));

        During(Accepted,
            When(OrderCompleted)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}