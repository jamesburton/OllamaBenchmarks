global using Contracts;
        using MassTransit;

namespace Contracts;

public record InvoiceCreated(Guid InvoiceId, decimal Amount);
        public record InvoiceApproved(Guid InvoiceId);
        public record InvoicePaid(Guid InvoiceId);

public class InvoiceSaga : SagaStateMachineInstance
        {
            public Guid CorrelationId { get; set; }
            public string CurrentState { get; set; }
            public decimal Amount { get; set; }
        }

public class OrderStateMachine : MassTransitStateMachine<OrderState>
        {
            public State Processing { get; private set; }
            public Event<OrderPlaced> OrderPlaced { get; private set; }

            public OrderStateMachine()
            {
                // ...
            }
        }

public class OrderStateMachine : MassTransitStateMachine<OrderState>
        {
            // ...
            public OrderStateMachine()
            {
                Initially(
                    When(OrderPlaced)
                        .TransitionTo(Processing));
                // ...
            }
        }

global using Contracts;
        using MassTransit;
        namespace Contracts;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
        {
            // ...
            public OrderStateMachine()
            {
                Initially(
                    When(OrderPlaced)
                        .TransitionTo(Processing));
                // ...
            }
        }

global using Contracts;
        using MassTransit;
        namespace Contracts;

global using Contracts;
        using MassTransit;
        namespace Contracts;

public class InvoiceSaga : SagaStateMachineInstance
        {
            public Guid CorrelationId { get; set; }
            public string CurrentState { get; set; }
            public decimal Amount { get; set; }
        }