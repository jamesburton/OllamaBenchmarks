global using Contracts;
using MassTransit;

namespace Contracts;

global using Contracts;
using MassTransit;
namespace Contracts {

global using Contracts;
using MassTransit;
namespace Contracts;

global using Contracts;
using MassTransit;
namespace Contracts
{

global using Contracts;
using MassTransit;
namespace Contracts;

namespace MyNamespace;
public class MyClass { ... }

public class InvoiceSaga : SagaStateMachineInstance
{
    public Guid CorrelationId { get; private set; }
    public string CurrentState { get; private set; } = null!;
    public decimal Amount { get; private set; }
}

public record InvoiceCreated(Guid InvoiceId, decimal Amount);
public record InvoiceApproved(Guid InvoiceId);
public record InvoicePaid(Guid InvoiceId);

public class ExampleState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
}

public class ExampleStateMachine : MassTransitStateMachine<ExampleState>
{
    public State NotStarted { get; private set; }
    public State Processing { get; private set; }
    public State Completed { get; private set; }

    public Event<SomeEvent> SomeEvent { get; private set; }

    public Event<SomeEvent> Event1 { get; private set; }

    public ExampleStateMachine()
    {
        InstanceState(x => x.CurrentState);
        Event(() => SomeEvent, x => x.CorrelateById(m => m.Message.SomeId));
        Initially(StartWith(NotStarted));
        During(NotStarted, When(SomeEvent).TransitionTo(Processing));
        // etc.
    }
}

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State NotAccepted { get; private set; }
    public State Accepted { get; private set; }
    public State Rejected { get; private set; }

    public Event<OrderAccepted> OrderAcceptedEvent { get; private set; }
    // ...

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);
        Event(() => OrderAcceptedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Initially(When(OrderAcceptedEvent).Then(x => x.Instance.CurrentState = "Accepted"));
        // etc.
    }
}