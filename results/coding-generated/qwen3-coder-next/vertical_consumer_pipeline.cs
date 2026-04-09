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

// Test classes (outside namespace)
public class ConsumerPipelineTests
{
    private readonly ITestHarness _harness;

    public ConsumerPipelineTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        var provider = services.BuildServiceProvider();
        _harness = provider.GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task PlaceOrderConsumer_Publishes_OrderPlaced()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerName = "John Doe";

        // Act
        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        // Assert
        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());

        var publishedOrderPlaced = await _harness.Published.First<OrderPlaced>();
        Assert.Equal(orderId, publishedOrderPlaced.Message.OrderId);
        Assert.Equal(customerName, publishedOrderPlaced.Message.CustomerName);
    }

    [Fact]
    public async Task FullConsumerPipeline_Publishes_CustomerNotified()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerName = "Jane Smith";

        // Act
        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        // Assert
        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Consumed.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());

        var publishedCustomerNotified = await _harness.Published.First<CustomerNotified>();
        Assert.Equal(orderId, publishedCustomerNotified.Message.OrderId);
        Assert.Equal($"Order {orderId} confirmed for {customerName}", publishedCustomerNotified.Message.NotificationMessage);
    }
}