public class OrderStateMachine : MassTransitStateMachine<OrderState>
        {
            public State Submitted { get; private set; }
            public State Accepted { get; private set; }
            public State Completed { get; private set; }

            public OrderStateMachine()
            {
                InstanceIdExpression(x => x.CorrelationId);
                InstanceIdType(typeof(Guid));

                Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));
                Event(() => OrderAccepted, x => x.CorrelateById(m => m.Message.OrderId));
                Event(() => OrderCompleted, x => x.CorrelateById(m => m.Message.OrderId));

                Instance(x =>
                {
                    x.CorrelationId = OrderId;
                    x.CurrentState = "Submitted";
                    x.OrderDate = OrderDate;
                });

                Initially(
                    When(OrderSubmitted)
                        .Then(context =>
                        {
                            context.Saga.OrderDate = context.Data.OrderDate;
                            context.Saga.CurrentState = "Submitted";
                        })
                        .TransitionTo(Submitted)
                );

                During(Submitted,
                    When(OrderAccepted)
                        .Then(context =>
                        {
                            context.Saga.AcceptedAt = context.Data.AcceptedAt;
                            context.Saga.CurrentState = "Accepted";
                        })
                        .TransitionTo(Accepted)
                );

                During(Accepted,
                    When(OrderCompleted)
                        .Finalize()
                );

                SetCompletedWhenFinalized();
            }
        }

global using Contracts;

    namespace Contracts;

    public record OrderSubmitted(Guid OrderId, DateTime OrderDate) : IRequest<OrderSubmitted>;
    public record OrderAccepted(Guid OrderId, DateTime AcceptedAt) : IRequest<OrderAccepted>;
    public record OrderCompleted(Guid OrderId) : IRequest<OrderCompleted>;

    public record OrderState : SagaStateMachineInstance<Guid>
    {
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }

    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public State Submitted { get; private set; }
        public State Accepted { get; private set; }
        public State Completed { get; private set; }

        public OrderStateMachine()
        {
            InstanceIdExpression(x => x.CorrelationId);
            InstanceIdType(typeof(Guid));

            Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => OrderAccepted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => OrderCompleted, x => x.CorrelateById(m => m.Message.OrderId));

            Instance(x =>
            {
                x.CorrelationId = OrderId;
                x.CurrentState = "Submitted";
                x.OrderDate = OrderDate;
            });

            Initially(
                When(OrderSubmitted)
                    .Then(context =>
                    {
                        context.Saga.OrderDate = context.Data.OrderDate;
                        context.Saga.CurrentState = "Submitted";
                    })
                    .TransitionTo(Submitted)
            );

            During(Submitted,
                When(OrderAccepted)
                    .Then(context =>
                    {
                        context.Saga.AcceptedAt = context.Data.AcceptedAt;
                        context.Saga.CurrentState = "Accepted";
                    })
                    .TransitionTo(Accepted)
            );

            During(Accepted,
                When(OrderCompleted)
                    .Finalize()
            );

            SetCompletedWhenFinalized();
        }
    }