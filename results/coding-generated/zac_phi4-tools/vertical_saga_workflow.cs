using System;
using MassTransit;
using MassTransit.Definition;
using MassTransit.Testing;
using Xunit;

public record OrderPlaced { public Guid OrderId { get; init; } }
public record PaymentReceived { public Guid OrderId { get; init; } }
public record OrderShipped { public Guid OrderId { get; init; } }
public record OrderCompleted { public Guid OrderId { get; init; } }

public class OrderFulfillmentState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? ShippedDate { get; set; }

    public static Property<Guid> CorrelationProperty = new(x => x.CorrelationId);
}

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

        Initially(
            When(OrderPlacedEvent)
                .Then(context =>
                {
                    context.Instance.OrderDate = DateTime.UtcNow;
                })
                .TransitionTo(Placed));

        During(Placed,
            When(PaymentReceivedEvent)
                .Then(context =>
                {
                    context.Instance.PaymentDate = DateTime.UtcNow;
                })
                .TransitionTo(Paid));

        During(Paid,
            When(OrderShippedEvent)
                .Then(context =>
                {
                    context.Instance.ShippedDate = DateTime.UtcNow;
                })
                .TransitionTo(Shipped));

        During(Shipped,
            When(OrderCompletedEvent)
                .Finalize());

        Event(() => OrderPlacedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentReceivedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderShippedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderCompletedEvent, x => x.CorrelateById(m => m.Message.OrderId));
    }
}

public class OrderFulfillmentTests : IClassFixture<MassTransitTestHarness>
{
    private readonly MassTransitTestHarness _harness;

    public OrderFulfillmentTests(MassTransitTestHarness harness)
    {
        _harness = harness;
    }

    [Fact]
    public async Task CompleteOrderFulfillmentSaga()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new OrderPlaced { OrderId = orderId });
        await _harness.Bus.Publish(new PaymentReceived { OrderId = orderId });
        await _harness.Bus.Publish(new OrderShipped { OrderId = orderId });
        await _harness.Bus.Publish(new OrderCompleted { OrderId = orderId });

        var saga = await _harness.SagaProvider.GetSaga<OrderFulfillmentState>(orderId);

        Assert.Equal("Completed", saga.CurrentState);
    }
}

public class Program
{
    public static void Main()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddSagaStateMachine<OrderFulfillmentStateMachine, OrderFulfillmentState>()
                .InMemoryRepository();
        });

        var provider = services.BuildServiceProvider();

        using var harness = provider.GetRequiredService<ITestHarness>();
        harness.Start();

        var testClassFixture = new OrderFulfillmentTests(harness);
        testClassFixture.CompletedOrderFulfillmentSaga().GetAwaiter().GetResult();
    }
}