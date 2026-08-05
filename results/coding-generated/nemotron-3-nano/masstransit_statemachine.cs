global using Contracts;

namespace Contracts;

record OrderSubmitted(Guid OrderId, DateTime OrderDate);
record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
record OrderCompleted(Guid OrderId);

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

    public Event<OrderSubmitted> OrderSubmittedEvent { get; private set; }
    public Event<OrderAccepted> OrderAcceptedEvent { get; private set; }
    public Event<OrderCompleted> OrderCompletedEvent { get; private set; }

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);
        Event(() => OrderSubmittedEvent,
            x => x.CorrelateById(m => m.OrderId));
        Event(() => OrderAcceptedEvent,
            x => x.CorrelateById(m => m.OrderId));
        Event(() => OrderCompletedEvent,
            x => x.CorrelateById(m => m.OrderId));

        Initially(
            When(OrderSubmittedEvent)
                .Subscribe(x => { x.Saga.OrderDate = x.Message.OrderDate; })
                .Then(context => context.Saga.CurrentState = "Submitted")
        );

        During(Submitted,
            When(OrderAcceptedEvent)
                .Subscribe(x => { x.Saga.AcceptedAt = x.Message.AcceptedAt; })
                .Then(context => context.Saga.CurrentState = "Accepted")
        );

        During(Accepted,
            When(OrderCompletedEvent)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }
}