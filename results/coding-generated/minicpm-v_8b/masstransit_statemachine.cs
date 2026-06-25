using Contracts;
using MassTransit;
using System;

namespace OrderStateMachine
{
    public class OrderState : SagaStateMachineInstance<OrderState>
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine)
        {
            stateMachine.SetCompletedWhenFinalized();
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            if (state.OrderDate == null)
                throw new Exception("OrderDate is not set");

            if (!string.IsNullOrEmpty(state.CurrentState))
                return;

            if (state.OrderDate != null && !state.OrderDate.Equals(DateTime.MinValue))
                state.CurrentState = "Submitted";
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState");
            }
        }

        protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine, OrderState state)
        {
            switch (state.CurrentState)
            {
                case "Submitted":
                    if (state.AcceptedAt == null)
                        throw new Exception("AcceptedAt is not set");

                    state.CurrentState = "Accepted";
                    break;
                case "Accepted":
                    // Do nothing
                    break;
                default:
                    throw new Exception("Invalid CurrentState