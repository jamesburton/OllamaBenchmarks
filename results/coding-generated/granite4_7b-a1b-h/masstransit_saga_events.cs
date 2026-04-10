global using Contracts;
using MassTransit;
namespace Contracts;

public class InvoiceSaga : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public decimal Amount { get; set; }
}

public record InvoiceCreated(Guid InvoiceId, decimal Amount);
public record InvoiceApproved(Guid InvoiceId);
public record InvoicePaid(Guid InvoiceId);

public class InvoiceStateMachine : MassTransitStateMachine<InvoiceSaga>
{
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