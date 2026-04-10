global using Contracts;

namespace Contracts;

// 1. Saga State Machine Instance
public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

// 2. Event Records
public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
public record OrderCompleted(Guid OrderId);

// 3. State Machine
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

        // Configure Correlation
        OrderSubmittedEvent = Event<OrderSubmitted>(
            cfg => cfg.CorrelateById(m => m.Message.OrderId));

        OrderAcceptedEvent = Event<OrderAccepted>(
            cfg => cfg.CorrelateById(m => m.Message.OrderId));

        OrderCompletedEvent = Event<OrderCompleted>(
            cfg => cfg.CorrelateById(m => m.Message.OrderId));

        // Initial State: OrderSubmitted -> Submitted
        Initially(
            When(OrderSubmittedEvent)
                .Then(context =>
                {
                    context.Saga.OrderDate = context.Message.OrderDate;
                })
                .TransitionTo(Submitted)
                .Send(context => new OrderAccepted(context.Message.OrderId, DateTime.UtcNow)) // Example action
                .Publish(context => new OrderAccepted(context.Message.OrderId, DateTime.UtcNow))
                .OnEnter(context =>
                {
                    context.Saga.CurrentState = "Submitted";
                })
        );

        // Transition: Submitted -> Accepted
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
        );

        // Transition: Accepted -> Completed (Finalize)
        During(Accepted,
            When(OrderCompletedEvent)
                .Then(context =>
                {
                    // No specific property update needed here, but we must use the required syntax
                })
                .TransitionTo(Completed)
                .Finalize()
                .OnEnter(context =>
                {
                    context.Saga.CurrentState = "Completed";
                })
        );
    }
}