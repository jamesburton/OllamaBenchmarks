using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Saga;
using MassTransit.Testing;
using Xunit;
using Xunit.Abstractions;

public record OrderPlaced(Guid OrderId);
public record PaymentReceived(Guid OrderId);
public record OrderShipped(Guid OrderId);
public record OrderCompleted(Guid OrderId);

public class OrderFulfillmentState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? ShippedDate { get; set; }
}

public class OrderFulfillmentStateMachine : MassTransitStateMachine<OrderFulfillmentState>
{
    public State Placed { get; private set; }
    public State Paid { get; private set; }
    public State Shipped { get; private set; }
    public State Completed { get; private set; }

    public Event<OrderPlaced> OrderPlaced { get; private set; }
    public Event<PaymentReceived> PaymentReceived { get; private set; }
    public Event<OrderShipped> OrderShipped { get; private set; }
    public Event<OrderCompleted> OrderCompleted { get; private set; }

    public OrderFulfillmentStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderPlaced, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => PaymentReceived, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => OrderShipped, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => OrderCompleted, x => x.CorrelateById(context => context.Message.OrderId));

        Initially(
            When(OrderPlaced)
                .Then(context =>
                {
                    context.Instance.OrderDate = DateTime.UtcNow;
                })
                .TransitionTo(Placed));

        During(Placed,
            When(PaymentReceived)
                .Then(context =>
                {
                    context.Instance.PaymentDate = DateTime.UtcNow;
                })
                .TransitionTo(Paid));

        During(Paid,
            When(OrderShipped)
                .Then(context =>
                {
                    context.Instance.ShippedDate = DateTime.UtcNow;
                })
                .TransitionTo(Shipped));

        During(Shipped,
            When(OrderCompleted)
                .TransitionTo(Completed)
                .Finalize());
    }
}

public class OrderFulfillmentSagaTests : IAsyncLifetime
{
    private readonly ITestHarness _harness;
    private readonly IBusControl _busControl;

    public OrderFulfillmentSagaTests(ITestOutputHelper output)
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddSagaStateMachine<OrderFulfillmentStateMachine, OrderFulfillmentState>()
                .InMemoryRepository();
        });

        var provider = services.BuildServiceProvider();
        _harness = provider.GetRequiredService<ITestHarness>();
        _busControl = provider.GetRequiredService<IBusControl>();
    }

    public async ValueTask InitializeAsync()
    {
        await _busControl.StartAsync();
        await _harness.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.StopAsync(TimeSpan.FromSeconds(5));
        await _busControl.StopAsync();
    }

    [Fact]
    public async Task Saga_Should_Reach_Completed_State()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new OrderPlaced(orderId));
        await _harness.Bus.Publish(new PaymentReceived(orderId));
        await _harness.Bus.Publish(new OrderShipped(orderId));
        await _harness.Bus.Publish(new OrderCompleted(orderId));

        var sagaRepository = _harness.GetSagaRepository<OrderFulfillmentState>();
        var saga = await sagaRepository.GetAsync(orderId);

        Assert.Equal("Completed", saga.CurrentState);
        Assert.NotNull(saga.PaymentDate);
        Assert.NotNull(saga.ShippedDate);
    }
}