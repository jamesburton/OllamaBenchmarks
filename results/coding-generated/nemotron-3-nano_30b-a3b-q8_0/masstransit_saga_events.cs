global using Contracts;
using MassTransit;
namespace Contracts;

public class InvoiceSaga : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public decimal Amount { get; set; }
    public Guid CurrentOrderId { get; set; }
}

public record InvoiceCreated(Guid InvoiceId, decimal Amount);
public record InvoiceApproved(Guid InvoiceId);
public record InvoicePaid(Guid InvoiceId);

public class InvoiceStateMachine : MassTransitStateMachine<InvoiceSaga>
{
    public State Pending { get; private set; }
    public State Approved { get; private set; }
    public State Paid { get; private set; }

    public Event<InvoiceCreated> InvoiceCreated { get; private set; }
    public Event<InvoiceApproved> InvoiceApproved { get; private set; }
    public Event<InvoicePaid> InvoicePaid { get; private set; }

    public InvoiceStateMachine()
    {
        InstanceState(x => x.CurrentState);
        Event(() => InvoiceCreated, ctx => ctx.CorrelateById(m => m.Message.InvoiceId));
        Event(() => InvoiceApproved, ctx => ctx.CorrelateById(m => m.Message.InvoiceId));
        Event(() => InvoicePaid, ctx => ctx.CorrelateById(m => m.Message.InvoiceId));

        Initially(
            When(InvoiceCreated)
                .Then(ctx => ctx.Saga.Amount = ctx.Message.Amount)
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