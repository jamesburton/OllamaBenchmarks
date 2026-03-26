using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;

public record OrderPlaced 
{
    public Guid OrderId { get; init; }
}

public record PaymentReceived 
{
    public Guid OrderId { get; init; }
}

public record OrderShipped 
{
    public Guid OrderId { get; init; }
}

public record OrderCompleted 
{
    public Guid OrderId { get; init; }
}

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
        Initially(
            When(OrderPlacedEvent)
                .TransitionTo(Placed, context => 
                    context.Saga.OrderDate = DateTime.UtcNow));

        During(Placed,
            When(PaymentReceivedEvent)
                .TransitionTo(Paid, context =>
                    context.Saga.PaymentDate = DateTime.UtcNow));

        During(Paid,
            When(OrderShippedEvent)
                .TransitionTo(Shipped, context =>
                    context.Saga.ShippedDate = DateTime.UtcNow));

        During(Shipped,
            When(OrderCompletedEvent)
                .Finalize());

        CorrelateById(context => context.Message.OrderId);
    }
}

public class OrderFulfillmentTests
{
    [Fact]
    public async Task TestOrderFulfillmentSaga()
    {
        var harness = new InMemoryTestHarness();

        harness.AddConsumer<OrderFulfillmentConsumer>();
        harness.AddKebabCaseMessageScheduler();
        harness.AddBus(harness.Bus);

        var stateMachine = harness.AddSagaStateMachine<OrderFulfillmentStateMachine, OrderFulfillmentState>()
            .InMemoryRepository();

        await harness.Start();

        var orderId = Guid.NewGuid();

        await harness.Bus.Publish(new OrderPlaced { OrderId = orderId });
        await harness.Bus.Publish(new PaymentReceived { OrderId = orderId });
        await harness.Bus.Publish(new OrderShipped { OrderId = orderId });
        await harness.Bus.Publish(new OrderCompleted { OrderId = orderId });

        var completedState = await harness.GetSaga<OrderFulfillmentState>(orderId);

        Assert.Equal("Completed", completedState.CurrentState);
    }
}