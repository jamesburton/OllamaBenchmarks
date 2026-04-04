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
    public Event<OrderSubmitted> OrderSubmitted { get; private set; }
    public Event<OrderAccepted> OrderAccepted { get; private set; }
    public Event<OrderCompleted> OrderCompleted { get; private set; }

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        // Configure Correlation for all events
        OrderSubmitted = Event<OrderSubmitted>(
            cfg => cfg.CorrelateById(m => m.Message.OrderId));

        OrderAccepted = Event<OrderAccepted>(
            cfg => cfg.CorrelateById(m => m.Message.OrderId));

        OrderCompleted = Event<OrderCompleted>(
            cfg => cfg.CorrelateById(m => m.Message.OrderId));

        // Initial State Flow
        Initially(
            When(OrderSubmitted)
                .Then(context =>
                {
                    context.Saga.OrderDate = context.Message.OrderDate;
                })
                .TransitionTo(Submitted)
                .OnEnter(context =>
                {
                    // Set initial state property
                    context.Saga.CurrentState = "Submitted";
                })
        );

        // Submitted State Flow
        During(Submitted,
            When(OrderAccepted)
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

        // Accepted State Flow
        During(Accepted,
            When(OrderCompleted)
                .Then(context =>
                {
                    // No specific property update required for this transition, just finalizing
                })
                .Finalize()
                .OnEnter(context =>
                {
                    context.Saga.CurrentState = "Completed";
                })
        );

        SetCompletedWhenFinalized();
    }
}