namespace Contracts;

global using Contracts;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "Submitted";
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

    public Event<OrderSubmitted> OrderSubmitted { get; } = null!;
    public Event<OrderAccepted> OrderAccepted { get; } = null!;
    public Event<OrderCompleted> OrderCompleted { get; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Initially(
            When(OrderSubmitted)
                .TransitionTo(Submitted)
                .Then(context =>
                {
                    context.Instance.OrderDate = context.Message.OrderDate;
                }));

        During(Submitted,
            When(OrderAccepted)
                .TransitionTo(Accepted)
                .Then(context =>
                {
                    context.Instance.AcceptedAt = context.Message.AcceptedAt;
                }));

        During(Accepted,
            When(OrderCompleted)
                .Finalize()
                .Then(context => 
                {
                    context.SetCompletedWhenFinalized();
                }));
    }
}

namespace Contracts;

global using Contracts;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "Submitted";
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

    public Event<OrderSubmitted> OrderSubmitted { get; } = null!;
    public Event<OrderAccepted> OrderAccepted { get; } = null!;
    public Event<OrderCompleted> OrderCompleted { get; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Initially(
            When(OrderSubmitted)
                .TransitionTo(Submitted)
                .Then(context =>
                {
                    context.Instance.OrderDate = context.Message.OrderDate;
                }));

        During(Submitted,
            When(OrderAccepted)
                .TransitionTo(Accepted)
                .Then(context =>
                {
                    context.Instance.AcceptedAt = context.Message.AcceptedAt;
                }));

        During(Accepted,
            When(OrderCompleted)
                .Finalize()
                .Then(context => 
                {
                    context.SetCompletedWhenFinalized();
                }));
    }
}

namespace Contracts;

global using Contracts;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "Submitted";
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

    public Event<OrderSubmitted> OrderSubmitted { get; } = null!;
    public Event<OrderAccepted> OrderAccepted { get; } = null!;
    public Event<OrderCompleted> OrderCompleted { get; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Initially(
            When(OrderSubmitted)
                .TransitionTo(Submitted)
                .Then(context =>
                {
                    context.Instance.OrderDate = context.Message.OrderDate;
                }));

        During(Submitted,
            When(OrderAccepted)
                .TransitionTo(Accepted)
                .Then(context =>
                {
                    context.Instance.AcceptedAt = context.Message.AcceptedAt;
                }));

        During(Accepted,
            When(OrderCompleted)
                .Finalize()
                .Then(context => 
                {
                    context.SetCompletedWhenFinalized();
                }));
    }
}

namespace Contracts;

global using Contracts;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "Submitted";
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

    public Event<OrderSubmitted> OrderSubmitted { get; } = null!;
    public Event<OrderAccepted> OrderAccepted { get; } = null!;
    public Event<OrderCompleted> OrderCompleted { get; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Initially(
            When(OrderSubmitted)
                .TransitionTo(Submitted)
                .Then(context =>
                {
                    context.Instance.OrderDate = context.Message.OrderDate;
                }));

        During(Submitted,
            When(OrderAccepted)
                .TransitionTo(Accepted)
                .Then(context =>
                {
                    context.Instance.AcceptedAt = context.Message.AcceptedAt;
                }));

        During(Accepted,
            When(OrderCompleted)
                .Finalize()
                .Then(context => 
                {
                    context.SetCompletedWhenFinalized();
                }));
    }
}

namespace Contracts;

global using Contracts;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "Submitted";
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

    public Event<OrderSubmitted> OrderSubmitted { get; } = null!;
    public Event<OrderAccepted> OrderAccepted { get; } = null!;
    public Event<OrderCompleted> OrderCompleted { get; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Initially(
            When(OrderSubmitted)
                .TransitionTo(Submitted)
                .Then(context =>
                {
                    context.Instance.OrderDate = context.Message.OrderDate;
                }));

        During(Submitted,
            When(OrderAccepted)
                .TransitionTo(Accepted)
                .Then(context =>
                {
                    context.Instance.AcceptedAt = context.Message.AcceptedAt;
                }));

        During(Accepted,
            When(OrderCompleted)
                .Finalize()
                .Then(context => 
                {
                    context.SetCompletedWhenFinalized();
                }));
    }
}

namespace Contracts;

global using Contracts;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "Submitted";
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

    public Event<OrderSubmitted> OrderSubmitted { get; } = null!;
    public Event<OrderAccepted> OrderAccepted { get; } = null!;
    public Event<OrderCompleted> OrderCompleted { get; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Initially(
            When(OrderSubmitted)
                .TransitionTo(Submitted)
                .Then(context =>
                {
                    context.Instance.OrderDate = context.Message.OrderDate;
                }));

        During(Submitted,
            When(OrderAccepted)
                .TransitionTo(Accepted)
                .Then(context =>
                {
                    context.Instance.AcceptedAt = context.Message.AcceptedAt;
                }));

        During(Accepted,
            When(OrderCompleted)
                .Finalize()
                .Then(context => 
                {
                    context.SetCompletedWhenFinalized();
                }));
    }
}

namespace Contracts;

global using Contracts;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "Submitted";
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

    public Event<OrderSubmitted> OrderSubmitted { get; } = null!;
    public Event<OrderAccepted> OrderAccepted { get; } = null!;
    public Event<OrderCompleted> OrderCompleted { get; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Initially(
            When(OrderSubmitted)
                .TransitionTo(Submitted)
                .Then(context =>
                {
                    context.Instance.OrderDate = context.Message.OrderDate;
                }));

        During(Submitted,
            When(OrderAccepted)
                .TransitionTo(Accepted)
                .Then(context =>
                {
                    context.Instance.AcceptedAt = context.Message.AcceptedAt;
                }));

        During(Accepted,
            When(OrderCompleted)
                .Finalize()
                .Then(context => 
                {
                    context.SetCompletedWhenFinalized();
                }));
    }
}