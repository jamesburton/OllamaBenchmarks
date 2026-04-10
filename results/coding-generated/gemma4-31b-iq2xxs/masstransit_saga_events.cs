public class InvoiceSaga : MassTransit.SagaStateMachineInstance { ... } // Wait, the prompt says "implementing SagaStateMachineInstance" but usually it's a class inheriting from a specific base or just a POCO. The prompt says "SagaStateMachineInstance".

public record InvoiceCreated(Guid InvoiceId, decimal Amount);
        public record InvoiceApproved(Guid InvoiceId);
        public record InvoicePaid(Guid InvoiceId);

public class InvoiceStateMachine : MassTransitStateMachine<InvoiceSaga> {
            public State Pending { get; private set; } = null!;
            public State Approved {Saga...} // and so on.
        }