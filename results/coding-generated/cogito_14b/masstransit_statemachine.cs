global using Contracts;

namespace Contracts;

public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
public record OrderCompleted(Guid OrderId);

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = default!;
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

        Submitted = InitialState();
        Accepted = CreateState("Accepted");
        Completed = CreateFinalState();

        OrderSubmittedEvent = When(() => new OrderSubmitted(default, default))
            .CorrelateById(m => m.OrderId)
            .SetSaga(saga =>
            {
                saga.OrderDate = m.OrderDate;
            });

        OrderAcceptedEvent = When(() => new OrderAccepted(default, default))
            .CorrelateById(m => m.OrderId);

        OrderCompletedEvent = Event<OrderCompleted>();

        Initially(
            When(OrderSubmittedEvent)
                .Then(context =>
                {
                    context.Saga.OrderDate = context.Data.OrderDate;
                })
                .TransitionTo(Submitted));

        During(Submitted,
            When(OrderAcceptedEvent)
                .Then(context =>
                {
                    context.Saga.AcceptedAt = context.Data.AcceptedAt;
                })
                .TransitionTo(Accepted));

        During(Accepted,
            When(OrderCompletedEvent)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}