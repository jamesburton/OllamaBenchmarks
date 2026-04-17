using MassTransit;
using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts;

// 1. Message Records
public record PlaceOrder(Guid OrderId, string CustomerName);
public record OrderPlaced(Guid OrderId, string CustomerName);
public record CustomerNotified(Guid OrderId, string NotificationMessage);

// 2. PlaceOrderConsumer
public class PlaceOrderConsumer : IConsumer<PlaceOrder>
{
    public async Task Consume(ConsumeContext<PlaceOrder> context)
    {
        // Publishes an OrderPlaced event
        await context.Publish(new OrderPlaced(context.Message.OrderId, context.Message.CustomerName));
    }
}

// 3. NotifyCustomerConsumer
public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        // Publishes a CustomerNotified event
        var notificationMessage = $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}";
        await context.Publish(new CustomerNotified(context.Message.OrderId, notificationMessage));
    }
}

public class ConsumerPipelineTests
{
    [Fact]
    public async Task Pipeline_PlaceOrder_Triggers_Notification()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMassTransit(x =>
        {
            // Register both consumers
            x.AddConsumer<PlaceOrderConsumer>();
            x.AddConsumer<NotifyCustomerConsumer>();
            // Use in-memory transport for testing
            x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
        });

        // Setup the test harness
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        using var serviceProvider = services.BuildServiceProvider();
        var harness = serviceProvider.GetRequiredService<ITestHarness>();

        var orderId = Guid.NewGuid();
        var customerName = "Alice Smith";
        var placeOrderMessage = new PlaceOrder(orderId, customerName);

        // Act: Publish the initial message
        await harness.Bus.Publish(placeOrderMessage);

        // Assert 1: Verify the first step (PlaceOrderConsumer) consumed the message and published OrderPlaced
        Assert.True(await harness.Consumed.Any<OrderPlaced>());

        // Assert 2: Verify the full pipeline (NotifyCustomerConsumer) consumed OrderPlaced and published CustomerNotified
        Assert.True(await harness.Consumed.Any<CustomerNotified>());
    }
}