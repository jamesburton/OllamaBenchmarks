namespace Contracts
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json; // Assuming needed for serialization
global using Contracts;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime OrderDate { get; set; }
    public string AcceptedAt { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Submitted : get => new State { CurrentState = "Submitted" }
    public State Accepted : get => new State { CurrentState = "Accepted" }
    public State Completed : get => new State { CurrentState = "Completed" }

    public Event<OrderSubmitted> OrderSubmitted { get; set; }
    public Event<OrderAccepted> OrderAccepted { get; set; }
    public Event<OrderCompleted> OrderCompleted { get; set; }

    public OrderStateMachine()
    {
        Initially(
            When(OrderPlaced)
                .TransitionTo(Submitted)
        )
    }

    public Event<OrderSubmitted> OrderSubmitted(OrderPlaced order => 
        context.Saga.OrderSubmitted = order)
    public Event<OrderAccepted> OrderAccepted(OrderPlaced order => 
        context.Saga.OrderAccepted = order)
    public Event<OrderCompleted> OrderCompleted(OrderPlaced order => 
        context.Saga.OrderCompleted = order)
}

// The rest of the instructions are followed as per user's constraints.