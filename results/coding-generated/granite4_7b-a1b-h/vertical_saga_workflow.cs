using MassTransit;
using MassTransit.Testing;
using System;
using System.Threading.Tasks;

public class OrderFulfillmentSaga
{
    public static async Task RunSagaTest()
    {
        var provider = new TestHarnessProvider();
        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderPlaced = new OrderPlaced { OrderId = Guid.NewGuid() };
        var paymentReceived = new PaymentReceived { OrderId = orderPlaced.OrderId };
        var orderShipped = new OrderShipped { OrderId = orderPlaced.OrderId };
        var orderCompleted = new OrderCompleted { OrderId = orderPlaced.OrderId };

        await harness.Bus.Publish(orderPlaced);
        await harness.Bus.Publish(paymentReceived);
        await harness.Bus.Publish(orderShipped);
        await harness.Bus.Publish(orderCompleted);

        Assert.True(await harness.Consumed.Any<OrderCompleted>());
        Assert.Equal(1, harness.Consumed.Count<OrderCompleted>());
    }
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
    public OrderFulfillmentStateMachine()
    {
        Initially(
            When(OrderPlaced)
                .TransitionTo(Paid)
                .CorrelateById(m => m.CorrelationId)
                .Set(m => m.OrderDate, () => DateTime.Now));
        State(Paid,
            When(OrderShipped)
                .TransitionTo(Shipped)
                .Set(m => m.ShippedDate, () => DateTime.Now));
        State(Shipped,
            When(OrderCompleted)
                .Finalize());
        During(Paid,
            When(OrderShipped)
                .TransitionTo(Shipped)
                .CorrelateById(m => m.CorrelationId));
        During(Shipped,
            When(OrderCompleted)
                .Finalize());
    }
}

[assembly: AssemblyFixture(typeof(OrderFulfillmentSaga))]