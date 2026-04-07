global using Contracts;
using System;
using MassTransit;

namespace Contracts;

public class InvoiceSaga : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = default!;
    public decimal Amount { get; set; }
}

public record InvoiceCreated(Guid InvoiceId, decimal Amount);
public record InvoiceApproved(Guid InvoiceId);
public record InvoicePaid(Guid InvoiceId);

public class InvoiceStateMachine : MassTransitStateMachine<InvoiceSaga>
{
    public State Pending { get; private set; } = null!;
    public State Approved { get; private set; } = null!;
    public State Paid { get; private set; } = null!;

    public Event<InvoiceCreated> InvoiceCreated { get; private set; } = null!;
    public Event<InvoiceApproved> InvoiceApproved { get; private set; } = null!;
    public Event<InvoicePaid> InvoicePaid { get; private set; } = null!;

    public InvoiceStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => InvoiceCreated, x => x.CorrelateById(m => m.Message.InvoiceId));
        Event(() => InvoiceApproved, x => x.CorrelateById(m => m.Message.InvoiceId));
        Event(() => InvoicePaid, x => x.CorrelateById(m => m.Message.InvoiceId));

        Initially(
            When(InvoiceCreated)
                .Then(ctx => ctx.Saga.Amount = ctx.Message.Amount)
                .TransitionTo(Pending));

        During(Pending,
            When(InvoiceApproved)
                .TransitionTo(Approved));

        During(Approved,
            When(InvoicePaid)
                .TransitionTo(Paid)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}