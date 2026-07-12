public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public st