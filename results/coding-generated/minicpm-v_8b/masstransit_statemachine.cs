using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Saga;

public class OrderState : SagaStateMachineInstance<OrderState>
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }

    protected override void ConfigureSaga(SagaStateMachine<OrderState> stateMachine)
    {
        stateMachine
            .When(x => x.OrderSubmitted)
                .SetProperty("OrderDate", DateTime.UtcNow);
            .When(x => x.OrderAccepted)
                .SetProperty("AcceptedAt", DateTime.UtcNow);

        stateMachine
            .During("Submitted")
                .When(x => x.OrderAccepted)
                    .SetState("Accepted");
            .When(x => x.OrderCompleted)
                .Finalize();
    }
}

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Submitted { get; private set; }
    public Event<OrderSubmitted> OrderSubmitted { get; private set; }

    protected override void ConfigureSaga(OrderStateMachine stateMachine)
    {
        stateMachine
            .When(x => x.OrderSubmitted)
                .SetProperty("OrderDate", DateTime.UtcNow);
            .When(x => x.OrderAccepted)
                .SetState("Accepted");
            .When(x => x.OrderCompleted)
                .Finalize();
    }

    protected override void ConfigureSagaStates(OrderStateMachine stateMachine)
    {
        stateMachine
            .During("Submitted")
                .When(x => x.OrderAccepted)
                    .SetState("Accepted");
            .When(x => x.OrderCompleted)
                .Finalize();
    }
}

public class OrderSubmitted : Event<OrderState>
{
    public Guid OrderId { get; set; }

    protected override void ConfigureSaga(OrderSubmitted eventRecord, OrderState state)
    {
        if (state.OrderDate == null) throw new InvalidOperationException("Order submitted without date");
        eventRecord.OrderId = state.OrderDate;
    }
}

public class OrderAccepted : Event<OrderState>
{
    public Guid OrderId { get; set; }

    protected override void ConfigureSaga(OrderAccepted eventRecord, OrderState state)
    {
        if (state.AcceptedAt == null) throw new InvalidOperationException("Order accepted without date");
        eventRecord.OrderId = state.AcceptedAt;
    }
}

public class OrderCompleted : Event<OrderState>
{
    public Guid OrderId { get; set; }

    protected override void ConfigureSaga(OrderCompleted eventRecord, OrderState state)
    {
        if (state.AcceptedAt == null) throw new InvalidOperationException("Order completed without date");
        eventRecord.OrderId = state.AcceptedAt;
    }
}