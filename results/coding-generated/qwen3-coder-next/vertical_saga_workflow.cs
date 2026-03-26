using MassTransit;
using MassTransit.Testing;

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
                .Then(context => context.Saga.OrderDate = DateTime.UtcNow)
                .TransitionTo(Placed));

        During(Placed,
            When(PaymentReceivedEvent)
                .Then(context => context.Saga.PaymentDate = DateTime.UtcNow)
                .TransitionTo(Paid));

        During(Paid,
            When(OrderShippedEvent)
                .Then(context => context.Saga.ShippedDate = DateTime.UtcNow)
                .TransitionTo(Shipped));

        During(Shipped,
            When(OrderCompletedEvent)
                .Finalize());
    }

    protected override void ConfigureStateMachine(IStateMachineSagaDefinition<OrderFulfillmentState> definition)
    {
        // Not used - per instructions, do not override ConfigureStateMachine
    }
}

public class OrderFulfillmentSagaTests
{
    [Fact]
    public async Task Saga_Completes_When_All_Events_Are_Processed()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddSagaStateMachine<OrderFulfillmentStateMachine, OrderFulfillmentState>()
                .InMemoryRepository();
            cfg.UseInMemoryOutbox();
        });

        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderId = Guid.NewGuid();

        // Act
        await harness.Bus.Publish(new OrderPlaced { OrderId = orderId });
        await harness.Bus.Publish(new PaymentReceived { OrderId = orderId });
        await harness.Bus.Publish(new OrderShipped { OrderId = orderId });
        await harness.Bus.Publish(new OrderCompleted { OrderId = orderId });

        // Wait for saga to complete
        var sagaRepository = harness.GetSagaRepository<OrderFulfillmentState>();
        OrderFulfillmentState saga = null;
        await TestContext.Await(async () =>
        {
            saga = await sagaRepository.Get(orderId);
            return saga?.CurrentState == "Completed";
        });

        // Assert
        Assert.NotNull(saga);
        Assert.Equal("Completed", saga.CurrentState);
        Assert.Equal(orderId, saga.CorrelationId);
        Assert.NotEqual(default, saga.OrderDate);
        Assert.NotEqual(default, saga.PaymentDate);
        Assert.NotEqual(default, saga.ShippedDate);
    }
}