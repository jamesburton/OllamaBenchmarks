namespace Contracts;

using MassTransit;
using MassTransit.Saga;

public record OrderPlaced(Guid OrderId, DateTime OrderDate);
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
    public static State Submitted { get; } = new("Submitted");
    public static State Accepted { get; } = new("Accepted");
    public static State Completed { get; } = new("Completed");

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);
        Initially(
            When(OrderPlaced)
                .CorrelateById(m => m.OrderId)
                .Do(context => { context.OrderDate = context.Message.OrderDate; })
                .TransitionTo(Submitted));
        During(Submitted,
            When(OrderAccepted)
                .CorrelateById(m => m.OrderId)
                .Do(context => { context.AcceptedAt = context.Message.AcceptedAt; })
                .TransitionTo(Accepted));
        During(Accepted,
            When(OrderCompleted)
                .CorrelateById(m => m.OrderId)
                .Then(context => { context.Then(context.Saga.CorrelationId = context.Saga.CorrelationId); })
                .Finalize());
    }
}