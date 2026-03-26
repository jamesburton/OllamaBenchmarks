global using Contracts;

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

    public Event<OrderSubmitted> SubmittedEvent { get; private set; }
    public Event<OrderAccepted> AcceptedEvent { get; private set; }
    public Event<OrderCompleted> CompletedEvent { get; private set; }

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => SubmittedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => AcceptedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => CompletedEvent, x => x.CorrelateById(m => m.Message.OrderId));

        Initially(
            When(SubmittedEvent)
                .Then(context => context.Saga.OrderDate = context.Data.OrderDate)
                .TransitionTo(Submitted));

        During(Submitted,
            When(AcceptedEvent)
                .Then(context => context.Saga.AcceptedAt = context.Data.AcceptedAt)
                .TransitionTo(Accepted));

        During(Accepted,
            When(CompletedEvent)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}