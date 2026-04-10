using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

// Global using for easy access to Contracts types
global using Contracts;

// --- Contracts Namespace (Messages and Consumers) ---
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
        var msg = context.Message;
        // Publish OrderPlaced event
        await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
    }
}

// 3. NotifyCustomerConsumer
public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var msg = context.Message;
        var notificationMessage = $"Order {msg.OrderId} confirmed for {msg.CustomerName}";

        // Publish CustomerNotified event
        await context.Publish(new CustomerNotified(msg.OrderId, notificationMessage));
    }
}

// --- Test Class (No Namespace) ---

public class ConsumerPipelineTests
{
    private IServiceCollection SetupServices()
    {
        var services = new ServiceCollection();
        services.AddMassTransit(x =>
        {
            x.AddConsumer<PlaceOrderConsumer>();
            x.AddConsumer<NotifyCustomerConsumer>();
            // Use in-memory bus for testing
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });
        return services;
    }

    [Fact]
    public async Task PlaceOrder_ShouldPublishOrderPlaced()
    {
        // Arrange
        var services = SetupServices();
        var provider = services.BuildServiceProvider();

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var initialMessage = new PlaceOrder(orderId, "Alice");

        // Act
        await harness.Bus.Publish(initialMessage);

        // Assert
        // 1. Verify the initial message was consumed
        await Assert.True(harness.Consumed.Any<PlaceOrder>());

        // 2. Verify the subsequent event was published
        await Assert.True(harness.Published.Any<OrderPlaced>());

        // Clean up
        await harness.Stop();
    }

    [Fact]
    public async Task FullPipeline_ShouldProcessAndPublishCustomerNotified()
    {
        // Arrange
        var services = SetupServices();
        var provider = services.BuildServiceProvider();

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var initialMessage = new PlaceOrder(orderId, "Bob");

        // Act
        await harness.Bus.Publish(initialMessage);

        // Wait for the asynchronous pipeline to complete
        // We wait until the final expected message (CustomerNotified) is published.
        await Task.Delay(100); 

        // Assert
        // 1. Verify the initial message was consumed
        await Assert.True(harness.Consumed.Any<PlaceOrder>());

        // 2. Verify the intermediate event was consumed
        await Assert.True(harness.Consumed.Any<OrderPlaced>());

        // 3. Verify the final event was published
        await Assert.True(harness.Published.Any<CustomerNotified>());

        // Optional: Verify the content of the final message
        var publishedCustomerNotified = await harness.Published.OfType<CustomerNotified>().FirstOrDefaultAsync();
        Assert.NotNull(publishedCustomerNotified);
        Assert.Equal(orderId, publishedCustomerNotified.OrderId);
        Assert.Contains("confirmed for Bob", publishedCustomerNotified.NotificationMessage);

        // Clean up
        await harness.Stop();
    }
}