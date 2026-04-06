using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Saga;

global using Contracts;

namespace Contracts
{
    /*-------------------------------------------------*/
    /* 1️⃣ OrderState – saga instance state            */
    /*-------------------------------------------------*/
    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }

    /*-------------------------------------------------*/
    /* 2️⃣ Event records (positional)                  */
    /*-------------------------------------------------*/
    public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
    public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
    public record OrderCompleted(Guid OrderId);

    /*-------------------------------------------------*/
    /* 3️⃣ OrderStateMachine                           */
    /*-------------------------------------------------*/
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public State Submitted { get; set; }
        public State Accepted { get; set; }
        public State Completed { get; set; }

        /* Events – correlation built into registration */
        public Event<OrderSubmitted> OrderSubmitted { get; }
        public Event<OrderAccepted> OrderAccepted { get; }
        public Event<OrderCompleted> OrderCompleted { get; }

        public OrderStateMachine()
        {
            // initial state, set property on entry
            InstanceState(x => x.CurrentState);

            // Correlation for every event (by Message OrderId)
            Event((msg) => msg.CorrelateById(m => m.OrderId));

            // -----------------------------------------------------------------
            // Initial transition – place order
            // -----------------------------------------------------------------
            Initial(
                (ctx, msg) =>
                {
                    ctx.Saga.OrderDate = msg.OrderDate;   // set property on entry
                },
                () => Transition(ctx, Submitted));

            // -----------------------------------------------------------------
            // From Submitted to Accepted
            // -----------------------------------------------------------------
            During(Submitted,
                (ctx, msg) =>
                {
                    ctx.Saga.AcceptedAt = msg.AcceptedAt; // set property
                },
                () => Transition(ctx, Accepted));

            // -----------------------------------------------------------------
            // From Accepted to Completed – final step
            // -----------------------------------------------------------------
            During(Accepted,
                (ctx, msg) =>
                {
                    ctx.Saga.CurrentState = "Completed"; // final state
                    ctx.Saga.OrderDate = DateTime.Now;      // any extra property
                },
                () => Transition(ctx, Completed));

            // Persist final state in saga lifecycle
            // (No explicit SetCompletedWhenFinalized needed – just state closed)
        }

        /* Called when the saga reaches the final state */
        protected override void FinalizeState(
            ISagaEventBus bus,
            OrderState state,
            ISagaEventContext context,
            SagasTransaction transaction)
        {
            SetCompletedWhenFinalized();
        }

        private void SetCompletedWhenFinalized()
        {
            // Placeholder – ensures finalization logic runs.
            // No extra work required because Completed state already reflects final values.
        }
    }
}