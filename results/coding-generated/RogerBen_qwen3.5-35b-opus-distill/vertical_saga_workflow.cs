using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Xunit;

// Message records
public record OrderPlaced(Guid OrderId);
public record PaymentReceived(Guid OrderId);
public record OrderShipped(Guid OrderId);
public record OrderCompleted(Guid OrderId);

// Saga state
public class OrderFulfillmentState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? ShippedDate { get; set; }
}

// State machine
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
        InstanceState(x => x.CurrentState);

        Event(x => OrderPlacedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(x => PaymentReceivedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(x => OrderShippedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(x => OrderCompletedEvent, x => x.CorrelateById(m => m.Message.OrderId));

        Initially(
            When(OrderPlacedEvent)
                .SetState(x => x.CurrentState, "Placed")
                .SetState(x => x.OrderDate, DateTime.UtcNow)
                .TransitionTo(Placed));

        During(Placed,
            When(PaymentReceivedEvent)
                .SetState(x => x.CurrentState, "Paid")
                .SetState(x => x.PaymentDate, DateTime.UtcNow)
                .TransitionTo(Paid));

        During(Paid,
            When(OrderShippedEvent)
                .SetState(x => x.CurrentState, "Shipped")
                .SetState(x => x.ShippedDate, DateTime.UtcNow)
                .TransitionTo(Shipped));

        During(Shipped,
            When(OrderCompletedEvent)
                .SetState(x => x.CurrentState, "Completed")
                .Finalize());
    }
}

// Test class
public class OrderFulfillmentTests : IAsyncLifetime
{
    private ITestHarness _harness;
    private IBus _bus;

    public ValueTask InitializeAsync()
    {
        var services = new ServiceCollection();
        services.AddMassTransit(x =>
        {
            x.AddSagaStateMachine<OrderFulfillmentStateMachine, OrderFulfillmentState>()
                .InMemoryRepository();
        });

        _harness = new TestHarness(services.BuildServiceProvider());
        _bus = _harness.Bus;

        return default;
    }

    public ValueTask DisposeAsync()
    {
        _harness?.Dispose();
        return default;
    }

    [Fact]
    public async Task OrderFulfillmentSaga_CompletesSuccessfully()
    {
        var orderId = Guid.NewGuid();

        await _harness.Start();

        await _bus.Publish(new OrderPlaced(orderId));
        await _bus.Publish(new PaymentReceived(orderId));
        await _bus.Publish(new OrderShipped(orderId));
        await _bus.Publish(new OrderCompleted(orderId));

        var saga = await _harness.Saga<OrderFulfillmentState>().Where(x => x.CorrelationId == orderId);

        Assert.NotNull(saga);
        Assert.Equal("Completed", saga.CurrentState);
    }
}