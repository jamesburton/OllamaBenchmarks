using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Contracts;

global using Contracts;
using MassTransit;

// Messages
public record PlaceOrder(Guid OrderId, string CustomerName);
public record OrderPlaced(Guid OrderId, string CustomerName);
public record CustomerNotified(Guid OrderId, string NotificationMessage);

// Consumers
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
        var msg = context.Message;
        var notification = $"Order {msg.OrderId} confirmed for {msg.CustomerName}";
        await context.Publish(new CustomerNotified(msg.OrderId, notification));
    }
}

// Tests
public class OrderPipelineTests : IAsyncLifetime
{
    private ITestHarness _harness;

    public async Task InitializationAsync()
    {
        var services = new ServiceCollection();
        services.AddMassTransit(x =>
        {
            x.UsingInMemory(cfg =>
            {
                cfg.AddConsumer<PlaceOrderConsumer>();
                cfg.AddConsumer<NotifyCustomerConsumer>();
                cfg.ConfigureEndpoints(_ => _);
            });
        });

        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        var provider = services.BuildServiceProvider();
        _harness = provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
    }

    [Fact]
    public async Task PlaceOrderConsumer_ConsumesAndPublishesOrderPlaced()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerName = "John Doe";
        var message = new PlaceOrder(orderId, customerName);

        // Act
        await _harness.Bus.Publish(message);

        // Assert
        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipeline_PublishesBothOrderPlacedAndCustomerNotified()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerName = "Jane Doe";
        var message = new PlaceOrder(orderId, customerName);

        // Act
        await _harness.Bus.Publish(message);

        // Assert
        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }
}