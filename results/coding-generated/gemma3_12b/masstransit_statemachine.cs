using MassTransit;
using System;

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
        var initialState = new InitialState();
        Submitted = new State(initialState);
        Accepted = new State();
        Completed = new State();

        OrderSubmitted = new Event<OrderSubmitted>(initialState);
        OrderAccepted = new Event<OrderAccepted>(initialState);
        OrderCompleted = new Event<OrderCompleted>(initialState);

        Initially(
            When(OrderSubmitted)
                .SetOrderDate(x => x.OrderDate = x.Message.OrderDate)
                .TransitionTo(Submitted));

        During(Submitted,
            When(OrderAccepted)
                .SetAcceptedAt(x => x.AcceptedAt = x.Message.AcceptedAt)
                .TransitionTo(Accepted));

        During(Accepted,
            When(OrderCompleted)
                .Finalize());

        SetCompletedWhenFinalized();
    }

    protected override void ConfigureStateMachineActions()
    {
        // Define actions for setting properties
        SetOrderDate = new Action<ISetStateContext<OrderState>, OrderSubmitted>(context, message) =>
        {
            context.SagaState.OrderDate = message.OrderDate;
        };

        SetAcceptedAt = new Action<ISetStateContext<OrderState>, OrderAccepted>(context, message) =>
        {
            context.SagaState.AcceptedAt = message.AcceptedAt;
        };
    }

    private Action<ISetStateContext<OrderState>, OrderSubmitted> SetOrderDate;
    private Action<ISetStateContext<OrderState>, OrderAccepted> SetAcceptedAt;
}