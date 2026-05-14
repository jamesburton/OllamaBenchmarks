global using Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts;

public record PlaceOrder(Guid OrderId, string CustomerName);
public record OrderPlaced(Guid OrderId, string CustomerName);
public record CustomerNotified(Guid OrderId, string NotificationMessage);

public class PlaceOrderConsumer : IConsumer<PlaceOrder>
{
    public async Task Consume(ConsumeContext<PlaceOrder> context)
    {
        var msg = context.Message;
        await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
    }
}

public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var msg = context.Message;
        await context.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
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
                .Finalize()));
}

public class TestOrchestrations
{
    [Fact]
    public async Task PlaceOrder_ProducesOrderPlacedEvent()
    {
        using var services = MassTransitContainer.Create();
        services.AddMassTransit(x => x.AddConsumer<PlaceOrderConsumer>());
        services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<NotifyCustomerConsumer>());

        var harness = services.GetRequiredService<ITestHarness>();
        await harness.Start();
        await serviceProvider.TryRegisterAssemblyContainers<Contracts>();

        var placeholder = new PlaceOrder(Guid.NewGuid(), "John Doe");
        await harness.Bus.Publish(placeholder);

        bool consumed = harness.Consumed.Any<OrderPlaced>();
        Assert.True(consumed);
    }

    [Fact]
    public async Task FullPipeline_ProducesBothEvents()
    {
        using var services = MassTransitContainer.Create();
        services.AddMassTransit(x => x.AddConsumer<PlaceOrderConsumer>());
        services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<NotifyCustomerConsumer>());

        var harness = services.GetRequiredService<ITestHarness>();
        await harness.Start();
        await serviceProvider.TryRegisterAssemblyContainers<Contracts>();

        var placeholder = new PlaceOrder(Guid.NewGuid(), "Jane Smith");
        await harness.Bus.Publish(placeholder);

        bool consumedOrderPlaced = harness.Consumed.Any<OrderPlaced>();
        bool publishedCustomerNotified = harness.Published.Any<CustomerNotified>();

        Assert.True(consumedOrderPlaced);
        Assert.True(publishedCustomerNotified);
    }
}