global using Contracts;

using MassTransit;
using System;
using System.Threading.Tasks;

namespace Contracts;

// 1. OrderState class implementing SagaStateMachineInstance
public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

// 2. Event records
public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
public record OrderCompleted(Guid OrderId);

// 3. OrderStateMachine class
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

        // Initially: When OrderSubmitted -> set OrderDate, TransitionTo Submitted
        Initially(
            When(OrderSubmitted)
                .Then(context =>
                {
                    context.Saga.OrderDate = context.Message.OrderDate;
                })
                .TransitionTo(Submitted)
                .CorrelateById(context => context.Message.OrderId)
        );

        // During Submitted: When OrderAccepted -> set AcceptedAt, TransitionTo Accepted
        During(Submitted,
            When(OrderAccepted)
                .Then(context =>
                {
                    context.Saga.AcceptedAt = context.Message.AcceptedAt;
                })
                .TransitionTo(Accepted)
                .CorrelateById(context => context.Message.OrderId)
        );

        // During Accepted: When OrderCompleted -> Finalize
        During(Accepted,
            When(OrderCompleted)
                .Finalize()
                .CorrelateById(context => context.Message.OrderId)
        );
    }
}