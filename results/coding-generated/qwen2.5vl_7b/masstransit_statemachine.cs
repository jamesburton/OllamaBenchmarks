using System;
using MassTransit;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

public class OrderSubmitted : Consume<OrderSubmitted>
{
    public Guid OrderId { get; set; }
    public DateTime OrderDate { get; set; }
}

public class OrderAccepted : Consume<OrderAccepted>
{
    public Guid OrderId { get; set; }
    public DateTime AcceptedAt { get; set; }
}

public class OrderCompleted : Consume<OrderCompleted>
{
    public Guid OrderId { get; set; }
}

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
        Initially(
            When(OrderSubmitted)
                .CorrelateBy(x => x.OrderId)
                .Send(new OrderSubmitted { OrderId = x.OrderId, OrderDate = x.OrderDate })
                .Then(t => t.SetOrderDate(t.Message.OrderDate))
                .TransitionTo(Submitted));

        During(Submitted,
            When(OrderAccepted)
                .CorrelateBy(x => x.OrderId)
                .Send(new OrderAccepted { OrderId = x.OrderId, AcceptedAt = DateTime.UtcNow })
                .Then(t => t.SetAcceptedAt(t.Message.AcceptedAt))
                .TransitionTo(Accepted));

        During(Accepted,
            When(OrderCompleted)
                .CorrelateBy(x => x.OrderId)
                .Send(new OrderCompleted { OrderId = x.OrderId })
                .Finalize());

        SetCompletedWhenFinalized();
    }
}