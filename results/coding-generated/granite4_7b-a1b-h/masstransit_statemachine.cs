using MassTransit;
using MassTransit.StateMachine;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Initially(
            When(OrderSubmitted)
                .CorrelatedBy(x => x.OrderId)
                .TransitionTo(Submitted)
                .Set(x => x.OrderDate, x => x.OrderDate));
        Submitted(
            When(OrderAccepted)
                .CorrelatedBy(x => x.OrderId)
                .TransitionTo(Accepted)
                .Set(x => x.AcceptedAt, x => x.AcceptedAt));
        Accepted(
            When(OrderCompleted)
                .CorrelatedBy(x => x.OrderId)
                .Finalize());
        During(Accepted,
            When(OrderCompleted)
                .Finalize());
    }
}

public static class StateMachineExtensions
{
    public static void SetCompletedWhenFinalized(this OrderStateMachine stateMachine)
    {
        During(Submitted,
            When(OrderCompleted)
                .Finalize());
        During(Accepted,
            When(OrderCompleted)
                .Finalize());
    }
}