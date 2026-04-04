using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
global using Contracts;

namespace Contracts;
public record PlaceOrder(Guid OrderId, string CustomerName);
public record OrderPlaced(Guid OrderId, string CustomerName);
public record CustomerNotified(Guid OrderId, string NotificationMessage);

// consumers
public class PlaceOrderConsumer : IConsumer<PlaceOrder>
{
    public async Task Consume(ConsumeContext<PlaceOrder> context)
    {
        await context.Publish(new OrderPlaced(context.Message.OrderId, context.Message.CustomerName));
    }
}

public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var message = new CustomerNotified(context.Message.OrderId,
            $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}");
        await context.Publish(message);
    }
}

// xUnit v3 test class (outside the Contracts namespace)
[Fact]
public async Task PlaceOrder_Consumer_Publishes_OrderPlaced()
{
    var provider = new ServiceCollection();
    provider.AddMassTransitTestHarness(cfg => cfg.AddConsumer<PlaceOrderConsumer>());
    var harness = provider.BuildServiceProvider().GetRequiredService<ITestHarness>();

    await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "John Doe"));

    Assert.True(await harness.Consumed.Any<PlaceOrder>());
    Assert.True(await harness.Published.Any<OrderPlaced>());
}

[Fact]
public async Task Full_Pipeline_Eventually_Completed()
{
    var provider = new ServiceCollection();
    provider.AddMassTransitTestHarness(cfg =>
        cfg.AddConsumer<PlaceOrderConsumer>()
        .AddConsumer<NotifyCustomerConsumer>());
    var harness = provider.BuildServiceProvider().GetRequiredService<ITestHarness>();

    await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Alice"));

    Assert.True(await harness.Consumed.Any<PlaceOrder>());
    Assert.True(await harness.Published.Any<OrderPlaced>());
    Assert.True(await harness.Published.Any<CustomerNotified>());
}