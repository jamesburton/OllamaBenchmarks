global using Contracts;
using MassTransit;
namespace Contracts;

// --- Message contracts ---
public record InvoiceCreated(Guid InvoiceId, decimal Amount);
public record InvoiceApproved(Guid InvoiceId);
public record InvoicePaid(Guid InvoiceId);

// --- Saga state ---
public class InvoiceSaga : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

// --- State machine ---
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

        Event(() => InvoiceCreated, m => m.CorrelateById(c => c.Message.InvoiceId));
        Event(() => InvoiceApproved, m => m.CorrelateById(c => c.Message.InvoiceId));
        Event(() => InvoicePaid, m => m.CorrelateById(c => c.Message.InvoiceId));

        Initially(
            When(InvoiceCreated)
                .Then(ctx => ctx.Saga.Amount = ctx.Message.Amount)
                .TransitionTo(Pending));

        During(Pending,
            When(InvoiceApproved)
                .TransitionTo(Approved));

        During(Approved,
            When(InvoicePaid)
                .Finalize()
                .Then(_ => SetCompletedWhenFinalized()));

        SetCompletedWhenFinalized();
    }
}