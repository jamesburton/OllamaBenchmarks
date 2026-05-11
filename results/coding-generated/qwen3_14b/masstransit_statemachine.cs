global using Contracts;

namespace Contracts;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

record OrderSubmitted(Guid OrderId, DateTime OrderDate);
record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
record OrderCompleted(Guid OrderId);

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

        OrderSubmitted = DefineEvent<OrderSubmitted>();
        OrderAccepted = DefineEvent<OrderAccepted>();
        OrderCompleted = DefineEvent<OrderCompleted>();

        OrderSubmitted.CorrelateById(context => context.Message.OrderId);
        OrderAccepted.CorrelateById(context => context.Message.OrderId);
        OrderCompleted.CorrelateById(context => context.Message.OrderId);

        Initially(
            When(OrderSubmitted)
                .Then(context => context.Saga.OrderDate = context.Message.OrderDate)
                .TransitionTo(Submitted));

        During(Submitted,
            When(OrderAccepted)
                .Then(context => context.Saga.AcceptedAt = context.Message.AcceptedAt)
                .TransitionTo(Accepted));

        During(Accepted,
            When(OrderCompleted)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}