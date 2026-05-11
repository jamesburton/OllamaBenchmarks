global using Contracts;
using MassTransit;

namespace Contracts;

// 1. Event Records (Positional Records)
public record InvoiceCreated(Guid InvoiceId, decimal Amount);
public record InvoiceApproved(Guid InvoiceId);
public record InvoicePaid(Guid InvoiceId);

// 2. Saga State Machine Instance
public class InvoiceSaga : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "Pending";
    public decimal Amount { get; set; }

    // MassTransit requires an ID property for SagaStateMachineInstance
    public Guid Id => CorrelationId;
}

// 3. Saga State Machine
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
        // Configure state tracking
        InstanceState(x => x.CurrentState);

        // Configure event correlation using InvoiceId from the message payload
        Event(() => InvoiceCreated, x => x.CorrelateById(m => m.Message.InvoiceId));
        Event(() => InvoiceApproved, x => x.CorrelateById(m => m.Message.InvoiceId));
        Event(() => InvoicePaid, x => x.CorrelateById(m => m.Message.InvoiceId));

        // Initial transition: When InvoiceCreated, extract Amount and transition to Pending
        Initially(
            When(InvoiceCreated)
                .Then(ctx => ctx.Saga.Amount = ctx.Message.Amount)
                .TransitionTo(Pending));

        // Transitions during Pending state
        During(Pending,
            When(InvoiceApproved)
                .TransitionTo(Approved));

        // Transitions during Approved state
        During(Approved,
            When(InvoicePaid)
                .TransitionTo(Paid)
                .Finalize());

        // Mark the saga as completed when it reaches the final state
        SetCompletedWhenFinalized();
    }
}