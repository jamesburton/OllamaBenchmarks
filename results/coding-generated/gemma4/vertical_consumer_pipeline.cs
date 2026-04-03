using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// Global using for easy access to types defined in Contracts namespace
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

// --- Test Class (Outside Contracts Namespace) ---

public class PipelineTests
{
    [Fact]
    public async Task PlaceOrder_ShouldPublishOrderPlaced()
    {
        // Arrange
        var testOrderId = Guid.NewGuid();
        var initialMessage = new PlaceOrder(testOrderId, "Alice");

        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            // We only need the first consumer for this specific test
        });

        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();

        // Act
        await harness.Start();
        await harness.Bus.Publish(initialMessage);

        // Assert
        // 1. Verify PlaceOrderConsumer consumed the message
        Assert.True(await harness.Consumed.Any<PlaceOrder>());

        // 2. Verify OrderPlaced was published by PlaceOrderConsumer
        var publishedOrderPlaced = await harness.Published.OfType<OrderPlaced>().FirstOrDefaultAsync(p => p.OrderId == testOrderId);
        Assert.NotNull(publishedOrderPlaced);
    }

    [Fact]
    public async Task FullPipeline_ShouldProcessOrderAndNotifyCustomer()
    {
        // Arrange
        var testOrderId = Guid.NewGuid();
        var initialMessage = new PlaceOrder(testOrderId, "Bob");

        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            // Register all consumers in the pipeline
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();

        // Act
        await harness.Start();
        await harness.Bus.Publish(initialMessage);

        // Wait for the pipeline to complete (PlaceOrder -> OrderPlaced -> CustomerNotified)
        // We wait until the final expected message (CustomerNotified) has been published.
        await Task.Delay(100); 

        // Assert
        // 1. Verify PlaceOrderConsumer consumed the initial message
        Assert.True(await harness.Consumed.Any<PlaceOrder>());

        // 2. Verify OrderPlaced was published and consumed
        var consumedOrderPlaced = await harness.Consumed.OfType<OrderPlaced>().FirstOrDefaultAsync(p => p.OrderId == testOrderId);
        Assert.NotNull(consumedOrderPlaced);

        // 3. Verify CustomerNotified was published (the final step)
        var publishedCustomerNotified = await harness.Published.OfType<CustomerNotified>().FirstOrDefaultAsync(p => p.OrderId == testOrderId);
        Assert.NotNull(publishedCustomerNotified);

        // Optional: Verify the content of the final message
        Assert.Contains("confirmed for Bob", publishedCustomerNotified.NotificationMessage);
    }
}