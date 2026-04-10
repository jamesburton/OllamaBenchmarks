public class OrderState : MassTransit.SagaStateMachineInstance
        {
            public Guid CorrelationId { get; set; }
            public string CurrentState { get; set; }
            public DateTime OrderDate { get; set; }
            public DateTime? AcceptedAt { get; set; }
        }