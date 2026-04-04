global using Contracts;
using System;
using System.Threading.Tasks;
using MassTransit;

namespace Contracts
{
    /// <summary>
    /// Saga state that tracks the order state.
    /// </summary>
    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }

    /// <summary>
    /// Event fired when the order is submitted.
    /// </summary>
    public record OrderSubmitted(guid OrderId, DateTime OrderDate);

    /// <summary>
    /// Event fired when the order is accepted.
    /// </summary>
    public record OrderAccepted(guid OrderId, DateTime AcceptedAt);

    /// <summary>
    /// Event fired when the order is completed.
    /// </summary>
    public record OrderCompleted(guid OrderId);

    /// <summary>
    /// State machine definition for the order saga.
    /// </summary>
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public State Submitted { get; private set; }
        public State Accepted { get; private set; }
        public State Completed { get; private set; }

        /// <summary>
        /// Submitted event.
        /// </summary>
        public Event<OrderSubmitted> OrderSubmitted { get; private set; }

        /// <summary>
        /// Accepted event.
        /// </summary>
        public Event<OrderAccepted> OrderAccepted { get; private set; }

        /// <summary>
        /// Completed event.
        /// </summary>
        public Event<OrderCompleted> OrderCompleted { get; private set; }

        public OrderStateMachine()
        {
            State Submitted, Accepted, Completed;

            // -------------------- Event binding for OrderSubmitted --------------------
            Event(() => OrderSubmitted, x => x.CorrelateById(m => m.OrderId))
                .When(Submitted, orderSubmitted => { orderSubmitted.OrderDate = orderSubmitted.OrderDate; })
                .Then(context => {
                    context.Saga.PropertyName = "OrderDate";
                })
                .FinalizeOrderPlaced();

            // -------------------- Event binding for OrderAccepted --------------------
            Event(() => OrderAccepted, x => x.CorrelateById(m => m.OrderId))
                .When(Accepted, orderAccepted => { orderAccepted.AcceptedAt = orderAccepted.AcceptedAt; })
                .Then(context => { context.Saga.PropertyName = "AcceptedAt"; })
                .FinalizeOrderCompleted();

            // -------------------- Event binding for OrderCompleted --------------------
            Event(() => OrderCompleted, x => x.CorrelateById(m => m.OrderId))
                .When(Completed, _ => {}) // just to satisfy the API
                .Finalize();

            // -------------------- State definitions --------------------
            State Submitted, Accepted, Completed;

            // Default (final) state
            DefaultState = Completed;

            // Initially: OrderSubmitted event -> set OrderDate -> go to Submitted
            Event(() => OrderSubmitted)
                .Initials((s, msg) => { s.OrderDate = msg.OrderDate; })
                .Then(context => { context.Saga.PropertyName = "OrderDate"; })
                .FinalizeOrderPlaced();

            // During Submitted: OrderAccepted event -> set AcceptedAt -> go to Accepted
            Event(() => OrderAccepted)
                .When(Submitted, (s, msg) => { s.AcceptedAt = msg.AcceptedAt; })
                .Then(context => { context.Saga.PropertyName = "AcceptedAt"; })
                .FinalizeOrderCompleted();

            // During Accepted: OrderCompleted event -> finalize saga
            Event(() => OrderCompleted)
                .When(Completed, _ => {})
                .Finalize();

            // Attach state definitions (for IntelliSense only)
            InstanceState(x => x); // placeholders to satisfy compiler – real logic handled via bindings above
        }

        #region Disposable methods required by MassTransit

        protected override void Dispose()
        {
            // No disposal needed
            base.Dispose();
        }

        #endregion

        #region State machine helper methods

        private void FinalizeOrderPlaced()
        {
            // Called after the OrderSubmitted event finishes
            DisposeCurrentSaga();
        }

        private void FinalizeOrderCompleted()
        {
            // Called after the OrderAccepted event finishes; leads to Completed state
            DisposeCurrentSaga();
        }

        private void DisposeCurrentSaga()
        {
            // Standard way to finalise a saga
            State current = GetCurrentState();
            if (current != Completed)
            {
                current.CurrentState = "Completed";
                CurrentState = "Completed";
            }
            base.Dispose();
        }

        #endregion
    }
}