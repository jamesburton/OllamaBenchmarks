public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Submitted { get; private set; }
    public State Accepted { get; private set; }
    public State Completed { get; private set; }

    public Event<OrderSubmitted> OrderSubmitted { get; private set; }
    public Event<OrderAccepted> OrderAccepted { get; private set; }
    public Event<OrderCompleted> OrderCompleted { get; private set; }

    public EventSchedule<SubmitOrder> SubmitOrderSchedule => ScheduleFactory.CreateSchedule(x => x
        .When(OrderSubmitted)
        .Then(context => context.Saga.OrderDate = context.Message.OrderDate));

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderSubmitted, x => x.CorrelateById(m => m.OrderId));
        Event(() => OrderAccepted, x => x.CorrelateById(m => m.OrderId));
        Event(() => OrderCompleted, x => x.CorrelateById(m => m.OrderId));

        Initially(
            When(OrderSubmitted)
                .Then(context => context.Saga.OrderDate = context.Message.OrderDate)
                .TransitionTo(Submitted));

        During(Submitted,
            When(OrderAccepted)
                .Then(context => context.Saga.AcceptedAt = context.Message.AcceptedAt)
                .TransitionTo(Accepted));

        During(Accepted,
            When(OrderCompleted)
                .Finalize()
                .SetCompletedWhenFinalized());
    }
}

During(Accepted,
    When(OrderCompleted)
        .Finalize()
        .SetCompletedWhenFinalized());

global using Contracts;

namespace Contracts;

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
        // Configure correlation for each event
        Event(() => OrderSubmitted, x => x.CorrelateById(m => m.OrderId));
        Event(() => OrderAccepted, x => x.CorrelateById(m => m.OrderId));
        Event(() => OrderCompleted, x => x.CorrelateById(m => m.OrderId));

        InstanceState(x => x.CurrentState);

        Initially(
            When(OrderSubmitted)
                .Then(context => context.Saga.OrderDate = context.Message.OrderDate)
                .TransitionTo(Submitted));

        During(Submitted,
            When(OrderAccepted)
                .Then(context => context.Saga.AcceptedAt = context.Message.AcceptedAt)
                .TransitionTo(Accepted));

        During(Accepted,
            When(OrderCompleted)
                .Finalize()
                .SetCompletedWhenFinalized());
    }
}

During(Accepted,
    When(OrderCompleted)
        .Finalize()
        .SetCompletedWhenFinalized());

global using Contracts;

namespace Contracts;

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
        Event(() => OrderSubmitted, x => x.CorrelateById(m => m.OrderId));
        Event(() => OrderAccepted, x => x.CorrelateById(m => m.OrderId));
        Event(() => OrderCompleted, x => x.CorrelateById(m => m.OrderId));

        InstanceState(x => x.CurrentState);

        Initially(
            When(OrderSubmitted)
                .Then(context => context.Saga.OrderDate = context.Message.OrderDate)
                .TransitionTo(Submitted));

        During(Submitted,
            When(OrderAccepted)
                .Then(context => context.Saga.AcceptedAt = context.Message.AcceptedAt)
                .TransitionTo(Accepted));

        During(Accepted,
            When(OrderCompleted)
                .Finalize()
                .SetCompletedWhenFinalized());
    }
}

using System;
using MassTransit;

global using Contracts;

global using Contracts;

using System;
using MassTransit;

namespace Contracts;

global using Contracts;

using System;
using MassTransit;

namespace Contracts;

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
        Event(() => OrderSubmitted, x => x.CorrelateById(m => m.OrderId));
        Event(() => OrderAccepted, x => x.CorrelateById(m => m.OrderId));
        Event(() => OrderCompleted, x => x.CorrelateById(m => m.OrderId));

        InstanceState(x => x.CurrentState);

        Initially(
            When<OrderSubmitted>()
                .Then(context => context.Saga.OrderDate = context.Message.OrderDate)
                .TransitionTo(Submitted));

        During(Submitted,
            When<OrderAccepted>()
                .Then(context => context.Saga.AcceptedAt = context.Message.AcceptedAt)
                .TransitionTo(Accepted));

        During(Accepted,
            When<OrderCompleted>()
                .Finalize()
                .SetCompletedWhenFinalized());
    }
}

Initially(
    When(OrderSubmitted)
        .Then(...)
        .TransitionTo(Submitted));

During(Submitted,
    When(OrderAccepted)
        .Then(...)
        .TransitionTo(Accepted));

During(Accepted,
    When(OrderCompleted)
        .Finalize()
        .SetCompletedWhenFinalized());