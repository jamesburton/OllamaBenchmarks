global using Contracts;

namespace Contracts;

using System;
using MassTransit;

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

    public Event<OrderSubmitted> SubmitOrder { get; private set; }
    public Event<OrderAccepted> AcceptOrder { get; private set; }
    public Event<OrderCompleted> CompleteOrder { get; private set; }

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        CorrelateById<SubmitOrder>(x => x.Message.OrderId);
        CorrelateById<AcceptOrder>(x => x.Message.OrderId);
        CorrelateById<CompleteOrder>(x => x.Message.OrderId);

        Initially(
            When(SubmitOrder)
                .Then(context =>
                {
                    context.Instance.OrderDate = context.Data.OrderDate;
                })
                .TransitionTo(Submitted));

        During(Submitted,
            When(AcceptOrder)
                .Then(context =>
                {
                    context.Instance.AcceptedAt = context.Data.AcceptedAt;
                })
                .TransitionTo(Accepted));

        During(Accepted,
            When(CompleteOrder)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}