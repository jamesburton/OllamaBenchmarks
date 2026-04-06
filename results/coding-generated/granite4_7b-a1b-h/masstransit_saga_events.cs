global using Contracts;
using MassTransit;

namespace Contracts;

public record InvoiceCreated(Guid InvoiceId, decimal Amount);
public record InvoiceApproved(Guid InvoiceId);
public record InvoicePaid(Guid InvoiceId);

public class InvoiceSaga : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public decimal Amount { get; set; }
}

public class InvoiceStateMachine : MassTransitStateMachine<InvoiceSaga>
{
    public State Pending { get; private set; }
    public State Approved { get; private set; }
    public State Paid { get; private set; }

    public InvoiceStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => InvoiceCreated, x => x.CorrelateById(m => m.InvoiceId));
        Event(() => InvoiceApproved, x => x.CorrelateById(m => m.InvoiceId));
        Event(() => InvoicePaid, x => x.CorrelateById(m => m.InvoiceId));

        Initially(
            When(InvoiceCreated)
                .Then(ctx => ctx.Amount = ctx.Message.Amount)
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