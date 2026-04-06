global using Contracts;
        namespace Contracts {
            // types
        }

global using Contracts;
        namespace Contracts {
            // ...
        }

public class OrderState : SagaStateMachineInstance
        {
            public Guid CorrelationId { get; set; }
            public string CurrentState { get; set; }
            public DateTime OrderDate { get; set; }
            public DateTime? AcceptedAt { get; set; }
        }

public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
        public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
        public record OrderCompleted(Guid OrderId);

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
                InstanceState(x => x.CurrentState); // Wait, the prompt says "Call InstanceState(x => x.CurrentState)".
                // Actually, looking at MassTransit v8 docs, InstanceState is used to configure the instance state.
                // The prompt says: "In the constructor, call InstanceState(x => x.CurrentState)".
                // I need to check if this is valid syntax. Usually, it's `InstanceState(x => x.CurrentState)`.
                // Wait, looking at the provided reference, it doesn't show InstanceState usage in the constructor.
                // But the prompt explicitly asks for it.
                // I will add it.

                Initially(
                    When(OrderSubmitted)
                        .Then(context => { context.Saga.OrderDate = context.Message.OrderDate; })
                        .TransitionTo(Submitted));

                During(Submitted,
                    When(OrderAccepted)
                        .Then(context => { context.Saga.AcceptedAt = context.Message.AcceptedAt; })
                        .TransitionTo(Accepted));

                During(Accepted,
                    When(OrderCompleted)
                        .Then(context => { context.Saga.CurrentState = "Completed"; }) // Or just Finalize
                        .Finalize());
            }
        }