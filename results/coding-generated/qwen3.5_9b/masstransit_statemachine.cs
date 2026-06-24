namespace Contracts;

global using Contracts;

public record OrderSubmitted(Guid OrderId, DateTime OrderDate);

public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);

public record OrderCompleted(Guid OrderId);

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
    public DateTime OrderDate { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Submitted => Defined(new StateDefinition("Submitted", x => x.IsInitial));
    public State Accepted => Defined(new StateDefinition("Accepted"));
    public State Completed => Defined(new StateDefinition("Completed"));

    public Event<OrderSubmitted> OrderPlaced { get; private set; } = null!;
    public Event<OrderAccepted> OrderShipped { get; private set; } = null!;
    public Event<OrderCompleted> OrderFinalized { get; private set; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        OrderPlaced = DefineEvent<OrderSubmitted>(x => x.CorrelateById(m => m.Message.OrderId));

        Initially(When(OrderPlaced)
            .Then(context => context.Saga.OrderDate = context.Message.OrderDate)
            .TransitionTo(Submitted));

        During(Submitted, When(OrderShipped)
            .Then(context => context.Saga.AcceptedAt = DateTime.UtcNow)
            .TransitionTo(Accepted));

        During(Accepted, When(OrderFinalized)
            .Finalize());

        SetCompletedWhenFinalized();
    }
}