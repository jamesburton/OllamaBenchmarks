global using Contracts;

namespace Contracts;

using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.StateMachines;

public record OrderSubmitted(Guid OrderId, DateTime OrderDate);
public record OrderAccepted(Guid OrderId, DateTime AcceptedAt);
public record OrderCompleted(Guid OrderId);

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Submitted { get; private set; }
    public State Accepted { get; private set; }
    public State Completed { get; private set; }

    public Event<OrderSubmitted> OrderSubmittedEvent { get; private set; }
    public Event<OrderAccepted> OrderAcceptedEvent { get; private set; }
    public Event<OrderCompleted> OrderCompletedEvent { get; private set; }

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event<OrderSubmitted>(x => x.CorrelateById(m => m.Message.OrderId), x => { });
        Event<OrderAccepted>(x => x.CorrelateById(m => m.Message.OrderId), x => { });
        Event<OrderCompleted>(x => x.CorrelateById(m => m.Message.OrderId), x => { });

        WhenStateSubmitted();
        WhenStateAccepted();
        WhenStateCompleted();
    }

    private void WhenStateSubmitted()
    {
        During<Submitted>(
            x => x.CorrelateById(m => m.Message.OrderId),
            async (context, next) =>
            {
                context.Saga.OrderDate = context.Message.OrderDate;
                context.Saga.AcceptedAt = context.Message.AcceptedAt;
                await next();
            });
    }

    private void WhenStateAccepted()
    {
        During<Accepted>(
            x => x.CorrelateById(m => m.Message.OrderId),
            async (context, next) =>
            {
                context.Saga.OrderDate = context.Message.OrderDate;
                context.Saga.AcceptedAt = context.Message.AcceptedAt;
                await next();
            });
    }

    private void WhenStateCompleted()
    {
        During<Completed>(
            x => x.CorrelateById(m => m.Message.OrderId),
            async (context, next) =>
            {
                await next();
            });
    }

    protected override void ConfigureInStateSubmitted(StateMachineState<OrderState> state)
    {
        state.Then(context =>
        {
            context.Saga.OrderDate = context.Message.OrderDate;
            context.Saga.AcceptedAt = context.Message.AcceptedAt;
            context.Saga.CurrentState = "Submitted";
        });
    }

    protected override void ConfigureInStateAccepted(StateMachineState<OrderState> state)
    {
        state.Then(context =>
        {
            context.Saga.OrderDate = context.Message.OrderDate;
            context.Saga.AcceptedAt = context.Message.AcceptedAt;
            context.Saga.CurrentState = "Accepted";
        });
    }

    protected override void ConfigureInStateCompleted(StateMachineState<OrderState> state)
    {
        state.Then(context =>
        {
            context.Saga.CurrentState = "Completed";
        });
    }

    protected override void ConfigureEvents()
    {
        Event<OrderSubmitted>(x => x.CorrelateById(m => m.Message.OrderId), x => { });
        Event<OrderAccepted>(x => x.CorrelateById(m => m.Message.OrderId), x => { });
        Event<OrderCompleted>(x => x.CorrelateById(m => m.Message.OrderId), x => { });
    }

    protected override void ConfigureStateMachine()
    {
        Initially(
            When<OrderSubmitted>(x => x.CorrelateById(m => m.Message.OrderId), x => { })
                .Then(context => { context.Saga.OrderDate = context.Message.OrderDate; context.Saga.CurrentState = "Submitted"; })
                .TransitionTo(Submitted)
        );

        During(Submitted,
            When<OrderAccepted>(x => x.CorrelateById(m => m.Message.OrderId), x => { })
                .Then(context => { context.Saga.OrderDate = context.Message.OrderDate; context.Saga.AcceptedAt = context.Message.AcceptedAt; context.Saga.CurrentState = "Accepted"; })
                .TransitionTo(Accepted)
        );

        During(Accepted,
            When<OrderCompleted>(x => x.CorrelateById(m => m.Message.OrderId), x => { })
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }
}