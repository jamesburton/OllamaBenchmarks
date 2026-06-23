public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }

    public OrderStateMachine()
    {
        Initially(When(OrderPlaced)
            .Then(ctx => ctx.Saga.OrderId = ctx.Message.OrderId)
            .TransitionTo(Processing));
    }
}