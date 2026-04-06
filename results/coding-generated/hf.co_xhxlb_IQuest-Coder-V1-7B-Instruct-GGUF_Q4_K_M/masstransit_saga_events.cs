namespace Contracts;

using MassTransit;

public class InvoiceSaga : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public decimal Amount { get; set; }
}

public record InvoiceCreated(Guid InvoiceId, decimal Amount)
{
    public Guid InvoiceId { get; init; }
    public decimal Amount { get; init; }
}

public record InvoiceApproved(Guid InvoiceId)
{
    public Guid InvoiceId { get; init; }
}

public record InvoicePaid(Guid InvoiceId)
{
    public Guid InvoiceId { get; init; }
}

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

    public State Pending { get; private set; } = null!;
    public State Approved { get; private set; } = null!;
    public State Paid { get; private set; } = null!;
}