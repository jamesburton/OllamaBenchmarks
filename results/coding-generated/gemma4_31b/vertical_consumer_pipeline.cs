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
        var msg = context.Message;
        await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
    }
}

public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var msg = context.Message;
        var notification = $"Order {msg.OrderId} confirmed for {msg.CustomerName}";
        await context.Publish(new CustomerNotified(msg.OrderId, notification));
    }
}

public class OrderPipelineTests
{
    private IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task PlaceOrder_ShouldPublishOrderPlaced()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "Alice";

        // Act
        await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        // Assert
        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipeline_ShouldPublishCustomerNotified()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "Bob";

        // Act
        await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        // Assert
        // Verify the intermediate step was consumed
        Assert.True(await harness.Consumed.Any<OrderPlaced>());
        // Verify the final step was published
        Assert.True(await harness.Published.Any<CustomerNotified>());
    }
}