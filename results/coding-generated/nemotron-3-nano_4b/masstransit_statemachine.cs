global using Contracts;

namespace Contracts
{
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
            InstanceState(x => x.CurrentState);
            StateSubmitted()
                .Then(context => context.OrderDate);

            StateAccepted()
                .Then(context => context.AcceptedAt)
                .Then(context => context.Saga.PropertyName = "Accepted");

            StateCompleted()
                .Then(this.SetCompletedWhenFinalized);

            Initially(
                When(OrderSubmitted)
                    .CorrelateById(m => m.OrderId)
                    .Add(State.Submitted));

            During(State.Submitted,
                When(OrderAccepted)
                    .CorrelateById(m => m.OrderId)
                    .Add(State.Accepted));

            During(State.Accepted,
                When(OrderCompleted)
                    .CorrelateById(m => m.OrderId)
                    .Finalize())
        }

        private void SetCompletedWhenFinalized()
        {
            CompleteState.Completed.Properties["Status"] = "Completed";
        }
    }
}