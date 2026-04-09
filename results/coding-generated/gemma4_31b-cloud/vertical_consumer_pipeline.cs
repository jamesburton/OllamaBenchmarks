global using Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Contracts;

// 1. Message records
public record PlaceOrder(Guid OrderId, string CustomerName);
public record OrderPlaced(Guid OrderId, string CustomerName);
public record CustomerNotified(Guid OrderId, string NotificationMessage);

// 2. PlaceOrderConsumer
public class PlaceOrderConsumer : IConsumer<PlaceOrder>
{
    public async Task Consume(ConsumeContext<PlaceOrder> context)
    {
        await context.Publish(new OrderPlaced(context.Message.OrderId, context.Message.CustomerName));
    }
}

// 3. NotifyCustomerConsumer
public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var message = $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}";
        await context.Publish(new CustomerNotified(context.Message.OrderId, message));
    }
}

// 4. xUnit v3 tests
public class OrderPipelineTests
{
    [Fact]
    public async Task PlaceOrder_ShouldPublishOrderPlaced()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var placeOrder = new PlaceOrder(orderId, "Alice");

        // Act
        await harness.Bus.Publish(placeOrder);

        // Assert
        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipeline_ShouldReachCustomerNotified()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var placeOrder = new PlaceOrder(orderId, "Bob");

        // Act
        await harness.Bus.Publish(placeOrder);

        // Assert
        Assert.Multiple(
            () => Assert.True(await harness.Consumed.Any<PlaceOrder>()),
            () => Assert.True(await harness.Consumed.Any<OrderPlaced>()),
            () => Assert.True(await harness.Published.Any<CustomerNotified>())
        );
    }
}