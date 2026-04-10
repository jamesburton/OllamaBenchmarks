global using Contracts;
using MassTransit;

namespace Contracts;

// 1. Saga State Machine Instance
public class InvoiceSaga : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public decimal Amount { get; set; }
}

// 2. Event Records
public record InvoiceCreated(Guid InvoiceId, decimal Amount);
public record InvoiceApproved(Guid InvoiceId);
public record InvoicePaid(Guid InvoiceId);

// 3. State Machine
public class InvoiceStateMachine : MassTransitStateMachine<InvoiceSaga>
{
    // States
    public State Pending { get; private set; } = null!;
    public State Approved { get; private set; } = null!;
    public State Paid { get; private set; } = null!;

    // Events
    public Event<InvoiceCreated> InvoiceCreated { get; private set; } = null!;
    public Event<InvoiceApproved> InvoiceApproved { get; private set; } = null!;
    public Event<InvoicePaid> InvoicePaid { get; private set; } = null!;

    public InvoiceStateMachine()
    {
        InstanceState(x => x.CurrentState);

        // Configure Event Correlation
        Event(InvoiceCreated, x => x.CorrelateById(m => m.Message.InvoiceId));
        Event(InvoiceApproved, x => x.CorrelateById(m => m.Message.InvoiceId));
        Event(InvoicePaid, x => x.CorrelateById(m => m.Message.InvoiceId));

        // Define State Machine Flow
        Initially(
            When(InvoiceCreated)
                .Then(ctx => { ctx.Saga.Amount = ctx.Message.Amount; })
                .TransitionTo(Pending)
        );

        During(Pending,
            When(InvoiceApproved)
                .TransitionTo(Approved)
        );

        During(Approved,
            When(InvoicePaid)
                .TransitionTo(Paid)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }
}