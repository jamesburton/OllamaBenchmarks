global using Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Contracts;

public record PlaceOrder(Guid OrderId, string CustomerName);
public record OrderPlaced(Guid OrderId, string CustomerName);
public record CustomerNotified(Guid OrderId, string NotificationMessage);

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
        await context.Publish(new CustomerNotified(
            context.Message.OrderId, 
            $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"));
    }
}

public class OrderPipelineTests
{
    private readonly IServiceProvider _provider;

    public OrderPipelineTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });
        _provider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task PlaceOrder_Should_Publish_OrderPlaced()
    {
        var harness = _provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "Alice";
        await harness.Bus.Publish(new PlaceOrder(orderlyId: orderId, CustomerName: customerName));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipeline_Should_Publish_CustomerNotified()
    {
        var harness = _provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "Bob";
        await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        // Verify first stage
        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());

        // Verify second stage
        Assert.True(await harness.Consumed.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());

        var publishedMessage = harness.Published.Select<CustomerNotified>().First();
        Assert.Equal(orderId, publishedMessage.Context.Message.OrderId);
        Assert.Contains(customerName, publishedMessage.Context.Message.NotificationMessage);
    }
}