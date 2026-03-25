using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Xunit;

// -----------------------------------------------------------------
// 1. Message contracts (all carry the same OrderId)
// -----------------------------------------------------------------
public record OrderPlaced(Guid OrderId);
public record PaymentReceived(Guid OrderId);
public record OrderShipped(Guid OrderId);
public record OrderCompleted(Guid OrderId);

// -----------------------------------------------------------------
// 2. Saga instance – holds state and data
// -----------------------------------------------------------------
public class OrderFulfillmentState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? ShippedDate { get; set; }
}

// -----------------------------------------------------------------
// 3. Saga state machine definition
// -----------------------------------------------------------------
public class OrderFulfillmentStateMachine :
    MassTransitStateMachine<OrderFulfillmentState>
{
    // States (exposed for test assertions)
    public State Placed { get; private set; }
    public State Paid { get; private set; }
    public State Shipped { get; private set; }
    public State Completed { get; private set; }

    // Events
    public Event<OrderPlaced> OrderPlacedEvent { get; private set; }
    public Event<PaymentReceived> PaymentReceivedEvent { get; private set; }
    public Event<OrderShipped> OrderShippedEvent { get; private set; }
    public Event<OrderCompleted> OrderCompletedEvent { get; private set; }

    public OrderFulfillmentStateMachine()
    {
        // Use the saga state property for tracking
        InstanceState(x => x.CurrentState);

        // All events share the same correlation id (the OrderId)
        CorrelateBy((context, _) => context.Message.OrderId);

        // -----------------------------------------------------------------
        // Events definition
        // -----------------------------------------------------------------
        Event(() => OrderPlacedEvent);
        Event(() => PaymentReceivedEvent);
        Event(() => OrderShippedEvent);
        Event(() => OrderCompletedEvent);

        // -----------------------------------------------------------------
        // State transitions
        // -----------------------------------------------------------------
        Initially(
            When(OrderPlacedEvent)
                .TransitionTo(Placed)
                .SetProperty(state => state.OrderDate, ctx => DateTime.UtcNow)
        );

        During(Placed,
            When(PaymentReceivedEvent)
                .TransitionTo(Paid)
                .SetProperty(state => state.PaymentDate, ctx => DateTime.UtcNow)
        );

        During(Paid,
            When(OrderShippedEvent)
                .TransitionTo(Shipped)
                .SetProperty(state => state.ShippedDate, ctx => DateTime.UtcNow)
        );

        During(Shipped,
            When(OrderCompletedEvent)
                .Finalize()
        );
    }

    // Explicit enum for easier assertions
    public enum State
    {
        Placed,
        Paid,
        Shipped,
        Completed
    }
}

// -----------------------------------------------------------------
// 4. xUnit test using the MassTransit test harness
// -----------------------------------------------------------------
public class OrderFulfillmentSagaTests : IAsyncLifetime
{
    private IServiceProvider _serviceProvider = null!;
    private ITestHarness _harness = null!;

    // -----------------------------------------------------------------
    // Test initialization – build the in‑memory test harness
    // -----------------------------------------------------------------
    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        services.AddMassTransitTestHarness(testConfigurator =>
        {
            testConfigurator.AddSagaStateMachine<OrderFulfillmentStateMachine, OrderFulfillmentState>()
                .InMemoryRepository();               // <-- saga persistence
        });

        _serviceProvider = services.BuildServiceProvider();

        _harness = _serviceProvider.GetRequiredService<ITestHarness>();
        await _harness.Start();                     // start the bus & repositories
    }

    // -----------------------------------------------------------------
    // Test cleanup
    // -----------------------------------------------------------------
    public async Task DisposeAsync()
    {
        await _harness.Stop();
        if (_serviceProvider is IDisposable d) d.Dispose();
    }

    // -----------------------------------------------------------------
    // The actual test
    // -----------------------------------------------------------------
    [Fact]
    public async Task Saga_Should_Progress_To_Completed_State()
    {
        // Use a deterministic order id so we can retrieve the saga later
        var orderId = Guid.NewGuid();

        // Publish the four events in the required order
        await _harness.Bus.Publish(new OrderPlaced(orderId));
        await _harness.Bus.Publish(new PaymentReceived(orderId));
        await _harness.Bus.Publish(new OrderShipped(orderId));
        await _harness.Bus.Publish(new OrderCompleted(orderId));

        // Give the saga a moment to finish (the in‑memory outbox is synchronous,
        // but a tiny delay makes the test more robust)
        await Task.Delay(TimeSpan.FromMilliseconds(200));

        // -------------------------------------------------------------
        // Retrieve the saga instance and assert its final state
        // -------------------------------------------------------------
        var saga = await _harness.GetSagaInstanceAsync<OrderFulfillmentStateMachine, OrderFulfillmentState>(orderId);

        Assert.NotNull(saga);                                      // saga must exist
        Assert.Equal(OrderFulfillmentStateMachine.State.Completed.ToString(),
                     saga.CurrentState);                         // final state is Completed

        // (Optional) verify that each message was indeed consumed
        Assert.True(await _harness.Consumed.Any<OrderPlaced>());
        Assert.True(await _harness.Consumed.Any<PaymentReceived>());
        Assert.True(await _harness.Consumed.Any<OrderShipped>());
        Assert.True(await _harness.Consumed.Any<OrderCompleted>());
    }
}