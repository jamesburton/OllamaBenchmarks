using MassTransit;
using MassTransit.Testing;
using System;
using System.Threading.Tasks;

// Order records
public class OrderPlaced
{
    public Guid OrderId { get; set; }
}

public class PaymentReceived
{
    public Guid OrderId { get; set; }
}

public class OrderShipped
{
    public Guid OrderId { get; set; }
}

public class OrderCompleted
{
    public Guid OrderId { get; set; }
}

// Saga state
public class OrderFulfillmentState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? ShippedDate { get; set; }
}

// Saga state machine
public class OrderFulfillmentStateMachine : MassTransitStateMachine<OrderFulfillmentState>
{
    public State Placed { get; private set; }
    public State Paid { get; private set; }
    public State Shipped { get; private set; }
    public State Completed { get; private set; }

    public Event<OrderPlaced> OrderPlacedEvent { get; private set; }
    public Event<PaymentReceived> PaymentReceivedEvent { get; private set; }
    public Event<OrderShipped> OrderShippedEvent { get; private set; }
    public Event<OrderCompleted> OrderCompletedEvent { get; private set; }

    public OrderFulfillmentStateMachine()
    {
        Initially(
            When(OrderPlacedEvent)
                .TransitionTo(Placed)
                .SetProperty(s => s.OrderDate, context => context.Message.OrderId),
            When(PaymentReceivedEvent)
                .TransitionTo(Paid)
                .SetProperty(s => s.PaymentDate, context => context.Message.OrderId),
            When(OrderShippedEvent)
                .TransitionTo(Shipped)
                .SetProperty(s => s.ShippedDate, context => context.Message.OrderId),
            When(OrderCompletedEvent)
                .Finalize()
                .SetProperty(s => s.CurrentState, context => "Completed")
        );

        During(Placed,
            When(PaymentReceivedEvent)
                .TransitionTo(Paid));

        During(Paid,
            When(OrderShippedEvent)
                .TransitionTo(Shipped));

        During(Shipped,
            When(OrderCompletedEvent)
                .Finalize());
    }
}

// Consumer
public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var msg = context.Message;
        await context.Publish(new OrderPlacedEvent(msg.OrderId));
    }
}

public class PaymentReceivedConsumer : IConsumer<PaymentReceived>
{
    public async Task Consume(ConsumeContext<PaymentReceived> context)
    {
        var msg = context.Message;
        await context.Publish(new PaymentReceivedEvent(msg.OrderId));
    }
}

public class OrderShippedConsumer : IConsumer<OrderShipped>
{
    public async Task Consume(ConsumeContext<OrderShipped> context)
    {
        var msg = context.Message;
        await context.Publish(new OrderShippedEvent(msg.OrderId));
    }
}

public class OrderCompletedConsumer : IConsumer<OrderCompleted>
{
    public async Task Consume(ConsumeContext<OrderCompleted> context)
    {
        var msg = context.Message;
        await context.Publish(new OrderCompletedEvent(msg.OrderId));
    }
}

// Test harness
public class MyTests : IAsyncLifetime
{
    private readonly ITestHarness _harness;

    public MyTests(ITestHarness harness)
    {
        _harness = harness;
    }

    public async ValueTask InitializeAsync()
    {
        await _harness.Start();
    }

    public ValueTask DisposeAsync()
    {
        return _harness.DisposeAsync();
    }

    [Fact]
    public async Task OrderFulfillmentSaga()
    {
        _harness.Bus.Publish(new OrderPlaced { OrderId = Guid.NewGuid() });
        _harness.Bus.Publish(new PaymentReceived { OrderId = Guid.NewGuid() });
        _harness.Bus.Publish(new OrderShipped { OrderId = Guid.NewGuid() });
        _harness.Bus.Publish(new OrderCompleted { OrderId = Guid.NewGuid() });

        var saga = _harness.Saga<OrderFulfillmentState>();

        Assert.Equal("Completed", saga.CurrentState);
    }
}