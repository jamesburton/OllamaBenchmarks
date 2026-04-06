global using Contracts;
using MassTransit;
namespace Contracts;

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
    public Event<InvoiceCreated> InvoiceCreated { get; private set; }
    public Event<InvoiceApproved> InvoiceApproved { get; private set; }
    public Event<InvoicePaid> InvoicePaid { get; private set; }
}

public class InvoiceStateMachine
{
    public InvoiceStateMachine() : this(When(InvoiceCreated).Then(ctx => ctx.Saga.Amount = ctx.Message.Amount)
    {
        Initially(When(InvoiceCreated).Then(ctx => ctx.Saga.Amount = ctx.Message.Amount).TransitionTo(Pending));
    }
    // Additional initialization steps if needed
}

InvoiceStateMachine.Initialize();
InstanceState(x => x.CurrentState);