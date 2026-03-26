using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Saga;
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
                .Then(context =>
                {
                    context.Instance.OrderDate = DateTime.UtcNow;
                    context.Instance.CurrentState = "Placed";
                })
                .TransitionTo(Placed));

        During(Placed,
            When(PaymentReceivedEvent)
                .Then(context =>
                {
                    context.Instance.PaymentDate = DateTime.UtcNow;
                    context.Instance.CurrentState = "Paid";
                })
                .TransitionTo(Paid));

        During(Paid,
            When(OrderShippedEvent)
                .Then(context =>
                {
                    context.Instance.ShippedDate = DateTime.UtcNow;
                    context.Instance.CurrentState = "Shipped";
                })
                .TransitionTo(Shipped));

        During(Shipped,
            When(OrderCompletedEvent)
                .Finalize());
    }
}

public class OrderFulfillmentSagaTests : IClassFixture<ITestHarness>
{
    private readonly ITestHarness _harness;

    public OrderFulfillmentSagaTests(ITestHarness harness)
    {
        _harness = harness;
    }

    [Fact]
    public async Task Should_complete_saga()
    {
        var orderId = Guid.NewGuid();
        await _harness.Bus.Publish(new OrderPlaced { OrderId = orderId });
        await _harness.Bus.Publish(new PaymentReceived { OrderId = orderId });
        await _harness.Bus.Publish(new OrderShipped { OrderId = orderId });
        await _harness.Bus.Publish(new OrderCompleted { OrderId = orderId });

        Assert.True(await _harness.Consumed.Any<OrderCompleted>());
    }
}

public static class TestHarnessExtensions
{
    public static async Task<bool> Consumed(this ITestHarness harness, Type messageType)
    {
        return await harness.Consumed.Any(messageType);
    }
}