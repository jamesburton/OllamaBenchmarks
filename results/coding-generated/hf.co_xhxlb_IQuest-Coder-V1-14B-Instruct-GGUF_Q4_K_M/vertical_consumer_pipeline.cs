using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// Global using for Contracts namespace
global using Contracts;

namespace Contracts;

// Message Records
public record PlaceOrder(Guid OrderId, string CustomerName);
public record OrderPlaced(Guid OrderId, string CustomerName);
public record CustomerNotified(Guid OrderId, string NotificationMessage);

// Consumer 1: PlaceOrderConsumer
public class PlaceOrderConsumer : IConsumer<PlaceOrder>
{
    public async Task Consume(ConsumeContext<PlaceOrder> context)
    {
        var message = context.Message;
        await context.Publish(new OrderPlaced(message.OrderId, message.CustomerName));
    }
}

// Consumer 2: NotifyCustomerConsumer
public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var message = context.Message;
        var notificationMessage = $"Order {message.OrderId} confirmed for {message.CustomerName}";
        await context.Publish(new CustomerNotified(message.OrderId, notificationMessage));
    }
}

// xUnit v3 Tests
public class ConsumerPipelineTests
{
    [Fact]
    public async Task PlaceOrder_Consumer_Consumes_And_Publishes_OrderPlaced()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });
        services.AddLogging();
        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();

        // Act
        await harness.Start();
        var orderId = Guid.NewGuid();
        var customerName = "John Doe";
        await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        // Assert
        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task Full_Pipeline_Executes_Correctly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });
        services.AddLogging();
        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();

        // Act
        await harness.Start();
        var orderId = Guid.NewGuid();
        var customerName = "Jane Smith";
        await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        // Assert
        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Consumed.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());
    }
}