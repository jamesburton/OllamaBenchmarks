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
        // Publish OrderPlaced event
        await context.Publish(new OrderPlaced(context.Message.OrderId, context.Message.CustomerName));
    }
}

// 3. NotifyCustomerConsumer
public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        // Construct the notification message
        var message = $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}";

        // Publish CustomerNotified event
        await context.Publish(new CustomerNotified(context.Message.OrderId, message));
    }
}

// --- Test Class (Outside Contracts Namespace) ---

public class ConsumerPipelineTests
{
    private ITestHarness SetupHarness()
    {
        var services = new ServiceCollection();

        // Register the consumers for the test harness
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        return harness;
    }

    [Fact]
    public async Task PlaceOrder_ShouldPublishOrderPlaced()
    {
        // Arrange
        var harness = SetupHarness();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var initialMessage = new PlaceOrder(orderId, "Alice");

        // Act
        await harness.Bus.Publish(initialMessage);

        // Assert
        // 1. Verify the initial message was consumed
        Assert.True(await harness.Consumed.Any<PlaceOrder>());

        // 2. Verify the first event was published
        var orderPlacedMessages = await harness.Published.OfType<OrderPlaced>().ToListAsync();
        Assert.Single(orderPlacedMessages);
        Assert.Equal(orderId, orderPlacedMessages.First().OrderId);
    }

    [Fact]
    public async Task FullPipeline_ShouldProcessPlaceOrderToCustomerNotified()
    {
        // Arrange
        var harness = SetupHarness();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var initialMessage = new PlaceOrder(orderId, "Bob");

        // Act
        // Publish the initial message
        await harness.Bus.Publish(initialMessage);

        // Wait for the asynchronous pipeline to complete (PlaceOrder -> OrderPlaced -> CustomerNotified)
        // We wait for the final expected message type to ensure the chain completes.
        await Task.Delay(100); 

        // Assert

        // 1. Verify the initial message was consumed
        Assert.True(await harness.Consumed.Any<PlaceOrder>());

        // 2. Verify the intermediate event was published
        var orderPlacedMessages = await harness.Published.OfType<OrderPlaced>().ToListAsync();
        Assert.Single(orderPlacedMessages);
        Assert.Equal(orderId, orderPlacedMessages.First().OrderId);

        // 3. Verify the final event was published
        var notifiedMessages = await harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.Single(notifiedMessages);

        var finalMessage = notifiedMessages.First();
        Assert.Equal(orderId, finalMessage.OrderId);
        Assert.Contains("confirmed for Bob", finalMessage.NotificationMessage);
    }
}