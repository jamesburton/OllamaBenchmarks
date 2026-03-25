using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Xunit;

public record OrderPlaced(Guid OrderId, DateTime OrderDate);
public record PaymentReceived(Guid OrderId, DateTime PaymentDate);
public record OrderShipped(Guid OrderId, DateTime ShippedDate);
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

    public Event<OrderPlaced> OrderPlacedEvent { get; private set; }
    public Event<PaymentReceived> PaymentReceivedEvent { get; private set; }
    public Event<OrderShipped> OrderShippedEvent { get; private set; }
    public Event<OrderCompleted> OrderCompletedEvent { get; private set; }

    public OrderFulfillmentStateMachine()
    {
        OrderPlacedEvent = DefineEvent<OrderPlaced>(e => e.CorrelationId = x => x.OrderId);
        PaymentReceivedEvent = DefineEvent<PaymentReceived>(e => e.CorrelationId = x => x.OrderId);
        OrderShippedEvent = DefineEvent<OrderShipped>(e => e.CorrelationId = x => x.OrderId);
        OrderCompletedEvent = DefineEvent<OrderCompleted>(e => e.CorrelationId = x => x.OrderId);

        Placed = DefineState(Placed);
        Paid = DefineState(Paid);
        Shipped = DefineState(Shipped);
        Completed = DefineState(Completed);

        Initially(
            When(OrderPlacedEvent)
                .TransitionTo(Placed)
                .SetSagaState(s => s.OrderDate = context.Message.OrderDate)
        );

        During(Placed,
            When(PaymentReceivedEvent)
                .TransitionTo(Paid)
                .SetSagaState(s => s.PaymentDate = context.Message.PaymentDate)
        );

        During(Paid,
            When(OrderShippedEvent)
                .TransitionTo(Shipped)
                .SetSagaState(s => s.ShippedDate = context.Message.ShippedDate)
        );

        During(Shipped,
            When(OrderCompletedEvent)
                .Finalize()
        );
    }
}

public class OrderFulfillmentTest
{
    [Fact]
    public async Task TestOrderFulfillmentSaga()
    {
        var harness = new TestHarness();
        await harness.Start();

        harness.ConfigureMassTransit(cfg =>
        {
            cfg.AddSagaStateMachine<OrderFulfillmentStateMachine, OrderFulfillmentState>()
               .InMemoryRepository();
        });

        var orderId = Guid.NewGuid();
        var orderDate = DateTime.UtcNow;
        var paymentDate = DateTime.UtcNow.AddMinutes(1);
        var shippedDate = DateTime.UtcNow.AddMinutes(2);

        await harness.Bus.Publish(new OrderPlaced(orderId, orderDate));
        await harness.Bus.Publish(new PaymentReceived(orderId, paymentDate));
        await harness.Bus.Publish(new OrderShipped(orderId, shippedDate));
        await harness.Bus.Publish(new OrderCompleted(orderId));

        var saga = await harness.Sagas.GetSaga<OrderFulfillmentState>(orderId);
        Assert.NotNull(saga);
        Assert.Equal("Completed", saga.CurrentState);
    }
}