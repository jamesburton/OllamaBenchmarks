global using Contracts;
        using MassTransit;
        // ... other usings
        namespace Contracts;
        // ... classes

global using Contracts;
        using MassTransit;
        using System;
        using System.Threading.Tasks;

        namespace Contracts;

        public class ...

public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
        public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
        public record OrderCompleted(Guid OrderId);

public class OrderState : SagaStateMachineInstance
        {
            public Guid CorrelationId { get; set; }
            public string CurrentState { get; set; } = null!;
            public DateTime OrderDate { get; set; }
            public DateTime? AcceptedAt { get; set; }
        }

Initially(
                When(OrderSubmitted)
                    .CorrelateById(m => m.Message.OrderId) // Wait, the prompt says "Configure event correlation using Event(() => ..., x => x.CorrelateById(m => m.Message.OrderId))"
                    .Then(context => { context.Saga.OrderDate = context.Message.OrderDate; })
                    .TransitionTo(Submitted)
            );

Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId))

Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));

Initially(
                When(OrderSubmitted)
                    .Then(...)
                    .TransitionTo(...)
            );

Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));

global using Contracts;
    using MassTransit;
    using System;

    namespace Contracts;

    public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
    public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
    public record OrderCompleted(Guid OrderId);

    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }

    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public State Submitted { get; private set; }
        public State Accepted { get; private set; }
        public State Completed { get; private set; }

        public Event<OrderSubmitted> OrderSubmitted { get; private set; }
        public Event<OrderAccepted> OrderAccepted { get; private set; }
        public Event<OrderCompleted> OrderCompleted { get; private set; }

        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);
            SetCompletedWhenFinalized();

            Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => OrderAccepted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => OrderCompleted, x => x.CorrelateById(m => m.Message.OrderId));

            Initially(
                When(OrderSubmitted)
                    .Then(context => { context.Saga.OrderDate = context.Message.OrderDate; })
                    .TransitionTo(Submitted)
            );

            During(Submitted,
                When(OrderAccepted)
                    .Then(context => { context.Saga.AcceptedAt = context.Message.AcceptedAt; })
                    .TransitionTo(Accepted)
            );

            During(Accepted,
                When(OrderCompleted)
                    .Finalize()
            );
        }
    }

global using Contracts;
    using MassTransit;
    using System;

    namespace Contracts;

    public class ...