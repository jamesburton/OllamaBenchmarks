global using Contracts;

namespace Contracts
{
    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public DateTime OrderDate { get; private set; }
        public DateTime? AcceptedAt { get; private set; }

        [EventRecord]
        public record OrderSubmitted(Guid OrderId, DateTime OrderDate);

        [EventRecord]
        public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);

        [EventRecord]
        public record OrderCompleted(Guid OrderId);
    }

    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        private State Submitted;
        private State Accepted;
        private State Completed;

        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState)
                .Initially(
                    When(OrderSubmitted)
                        .TransitionTo(Submitted).Then(context =>
                            context.Saga.OrderDate = (OrderPlaced as Event<OrderPlaced>).Data.OrderDate
                        )
                );

            Submitted = new State("Submitted");
            Accepted = new State("Accepted");
            Completed = new State("Completed");

            During(Submitted,
                When(OrderAccepted)
                    .TransitionTo(Accepted).Then(context =>
                            context.Saga.AcceptedAt = (OrderPlaced as Event<OrderPlaced>).Data.AcceptedAt
                        )
            );

            During(Submitted, Accepted);

            OnFinalized().Finalize();
        }

        public State Submitted { get; private set; }
        public State Accepted { get; private set; }
        public State Completed { get; private set; }

        [Event]
        public event Event<OrderPlaced> OrderPlaced;

        [Event]
        public event Event<OrderAccepted> OrderAccepted;
    }
}