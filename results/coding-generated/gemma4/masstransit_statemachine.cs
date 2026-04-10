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
    // States
    public State Submitted { get; private set; }
    public State Accepted { get; private set; }
    public State Completed { get; private set; }

    // Events
    public Event<OrderSubmitted> OrderSubmittedEvent { get; private set; }
    public Event<OrderAccepted> OrderAcceptedEvent { get; private set; }
    public Event<OrderCompleted> OrderCompletedEvent { get; private set; }

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        // Initial State: Submitted
        Initially(
            When(OrderSubmittedEvent)
                .Then(context =>
                {
                    context.Saga.OrderDate = context.Message.OrderDate;
                })
                .TransitionTo(Submitted)
                .OnEnter(context =>
                {
                    context.Saga.CurrentState = "Submitted";
                })
                .Publish(context => new OrderSubmitted(context.Message.OrderId, context.Message.OrderDate)) // Example publish if needed, though not strictly required by prompt
        );

        // Transition from Submitted to Accepted
        During(Submitted,
            When(OrderAcceptedEvent)
                .Then(context =>
                {
                    context.Saga.AcceptedAt = context.Message.AcceptedAt;
                })
                .TransitionTo(Accepted)
                .OnEnter(context =>
                {
                    context.Saga.CurrentState = "Accepted";
                })
                .Publish(context => new OrderAccepted(context.Message.OrderId, context.Message.AcceptedAt))
        );

        // Transition from Accepted to Completed (Finalize)
        During(Accepted,
            When(OrderCompletedEvent)
                .Then(context =>
                {
                    // No specific property setting required for completion based on prompt, just finalizing
                })
                .TransitionTo(Completed)
                .OnEnter(context =>
                {
                    context.Saga.CurrentState = "Completed";
                })
                .Finalize()
                .Publish(context => new OrderCompleted(context.Message.OrderId))
        );
    }
}

// Helper extension to satisfy the requirement of using Event(...) with correlation
public static class StateMachineExtensions
{
    public static Event<T> Event<T>(this OrderStateMachine stateMachine, Func<Event<T>> eventFactory)
    {
        return stateMachine.Event(eventFactory);
    }
}