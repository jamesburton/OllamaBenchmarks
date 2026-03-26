using MassTransit;
namespace Contracts;
using System;

public class PlaceOrderConsumer : IConsumer<PlaceOrder> {
    public async Task Consume(ConsumeContext<PlaceOrder> context) {
        var msg = context.Message;
        await context.Publish(new PlaceOrder(msg.OrderId, msg.CustomerName));
    }
}

public class NotifyCustomerConsumer : IConsumer<OrderPlaced> {
    public async Task Consume(ConsumeContext<OrderPlaced> context) {
        var msg = context.Message;
        var notification = $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}";
        await context.Publish(new CustomerNotified(msg.OrderId, notification));
    }
}

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Processing { get; private set; }
    public Event<OrderPlaced> OrderPlaced { get; private set; }

    public OrderStateMachine()
    {
        Initially(
            When(OrderPlaced)
                .TransitionTo(Processing));
        During(Processing,
            When(OrderShipped)
                .Finalize());
    }
}

// Test harness setup (not part of the required output, but per user instruction)
// services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<PlaceOrderConsumer>());
// harness.Consumed.Any<PlaceOrder>().Any<OrderPlaced>().Any<CustomerNotified>();