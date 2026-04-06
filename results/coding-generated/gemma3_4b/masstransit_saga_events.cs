namespace Contracts;

global using MassTransit;
using System.Diagnostics.CodeAnalysis;

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
    public State Pending { get; private set; } = null!;
    public State Approved { get; private set; } = null!;
    public State Paid { get; private set; } = null!;

    public Event<InvoiceCreated> InvoiceCreated { get; private set; } = null!;
    public Event<InvoiceApproved> InvoiceApproved { get; private set; } = null!;
    public Event<InvoicePaid> InvoicePaid { get; private set; } = null!;

    public InvoiceStateMachine()
    {
        Initially(
            When(InvoiceCreated)
                .TransitionTo(Pending)
                .CorrelateById(m => m.Message.InvoiceId)
        );
        During(Pending,
            When(InvoiceApproved)
                .TransitionTo(Approved)
                .CorrelateById(m => m.Message.InvoiceId)
        );
        During(Approved,
            When(InvoicePaid)
                .TransitionTo(Paid)
                .Finalize()
                .CorrelateById(m => m.Message.InvoiceId)
        );
    }
}