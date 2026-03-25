using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Saga;
using MassTransit.Testing;
using Xunit;
using Xunit.Abstractions;

public class OrderFulfillment
{
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

        public Event<OrderPlaced> OrderPlacedEvent { get; private set; }
        public Event<PaymentReceived> PaymentReceivedEvent { get; private set; }
        public Event<OrderShipped> OrderShippedEvent { get; private set; }
        public Event<OrderCompleted> OrderCompletedEvent { get; private set; }

        public OrderFulfillmentStateMachine()
        {
            var c = new CorrelateBy<OrderFulfillmentState>(x => x.CorrelationId);

            Initially(
                When(OrderPlacedEvent)
                    .Then((context, state) =>
                    {
                        state.OrderDate = DateTime.UtcNow;
                        context.Publish(new OrderPlaced(state.CorrelationId));
                    })
                    .TransitionTo(Placed));

            AroundContext(
                c
            );

            Placed(
                When(PaymentReceivedEvent)
                    .Then((context, state) =>
                    {
                        state.PaymentDate = DateTime.UtcNow;
                        context.Publish(new PaymentReceived(state.CorrelationId));
                    })
                    .TransitionTo(Paid));

            Paid(
                When(OrderShippedEvent)
                    .Then((context, state) =>
                    {
                        state.ShippedDate = DateTime.UtcNow;
                        context.Publish(new OrderShipped(state.CorrelationId));
                    })
                    .TransitionTo(Shipped));

            Shipped(
                When(OrderCompletedEvent)
                    .Then((context, state) =>
                    {
                        context.Publish(new OrderCompleted(state.CorrelationId));
                    })
                    .Finalize());
        }
    }

    [Fact(DisplayName = "Order Fulfillment Saga Test")]
    public async Task OrderFulfillmentSagaTest(ITestOutputHelper output)
    {
        var harness = new TestHarness();
        var bus = harness.Bus;

        harness.AddSagaStateMachine<OrderFulfillmentStateMachine, OrderFulfillmentState>()
            .InMemoryRepository();

        await harness.Start();

        Guid correlationId = Guid.NewGuid();

        bus.Publish(new OrderPlaced(correlationId)).Wait();
        await harness.Consumed.Any<OrderPlaced>().WaitAsync();

        bus.Publish(new PaymentReceived(correlationId)).Wait();
        await harness.Consumed.Any<PaymentReceived>().WaitAsync();

        bus.Publish(new OrderShipped(correlationId)).Wait();
        await harness.Consumed.Any<OrderShipped>().WaitAsync();

        bus.Publish(new OrderCompleted(correlationId)).Wait();
        await harness.Consumed.Any<OrderCompleted>().WaitAsync();

        var state = await harness.GetSagaState<OrderFulfillmentState>(correlationId);
        Assert.NotNull(state);
        Assert.Equal("Completed", state.CurrentState);
        Assert.NotNull(state.OrderDate);
        Assert.NotNull(state.PaymentDate);
        Assert.NotNull(state.ShippedDate);

        await harness.Stop();
    }
}