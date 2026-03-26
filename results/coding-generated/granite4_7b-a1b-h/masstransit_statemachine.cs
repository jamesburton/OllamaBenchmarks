namespace Contracts;

using MassTransit;
using MassTransit.Saga;

public record OrderPlaced(Guid OrderId, DateTime OrderDate);
public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
public record OrderCompleted(Guid OrderId);

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "Submitted";
    public DateTime? OrderDate { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public static State Submitted { get; private set; }
    public static State Accepted { get; private set; }
    public static State Completed { get; private set; }

    public static Event<OrderPlaced> OrderSubmitted { get; private set; }
    public static Event<OrderAccepted> OrderAccepted { get; private set; }
    public static Event<OrderCompleted> OrderCompleted { get; private set; }

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Submitted = new State("Submitted");
        Accepted = new State("Accepted");
        Completed = new State("Completed");

        Initially(
            When(OrderSubmitted)
                .Execute(context =>
                {
                    context.Saga.CorrelationId = context.Message.OrderId;
                    SetCompletedWhenFinalized();
                })
                .TransitionTo(Submitted));

        During(Submitted,
            When(OrderAccepted)
                .Then(context => { context.AcceptedAt = context.Message.AcceptedAt; })
                .TransitionTo(Accepted));

        During(Accepted,
            When(OrderCompleted)
                .Finalize());

        SetCompletedWhenFinalized();
    }

    private void SetCompletedWhenFinalized()
    {
        Completed = new State("Completed");
        Finalize();
    }
}