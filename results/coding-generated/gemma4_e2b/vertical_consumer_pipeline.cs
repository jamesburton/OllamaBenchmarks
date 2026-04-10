using MassTransit;
using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

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
    public async Task FullPipelineTest()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMassTransit(x =>
        {
            // Register consumers
            x.AddConsumer<PlaceOrderConsumer>();
            x.AddConsumer<NotifyCustomerConsumer>();
        });

        // Setup the test harness
        services.AddMassTransitTestHarness(cfg =>
        {
            // Register all consumers to the harness
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        // Build the service provider
        var provider = services.BuildServiceProvider();

        // Act: Start the harness
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Define the message to be published
        var orderId = Guid.NewGuid();
        var customerName = "Alice Smith";
        var placeOrderMessage = new PlaceOrder(orderId, customerName);

        // Act: Publish the initial message
        await harness.Bus.Publish(placeOrderMessage);

        // Assert: Verify the pipeline execution

        // 1. Verify PlaceOrderConsumer consumed PlaceOrder and published OrderPlaced
        Assert.True(await harness.Consumed.Any<OrderPlaced>());

        // 2. Verify NotifyCustomerConsumer consumed OrderPlaced and published CustomerNotified
        Assert.True(await harness.Consumed.Any<CustomerNotified>());

        // Optional: Verify the exact content of the published events (for robustness)
        var consumedOrderPlaced = await harness.Consumed.FirstOrDefaultAsync<OrderPlaced>();
        Assert.NotNull(consumedOrderPlaced);
        Assert.Equal(orderId, consumedOrderPlaced.OrderId);
        Assert.Equal(customerName, consumedOrderPlaced.CustomerName);

        var consumedCustomerNotified = await harness.Consumed.FirstOrDefaultAsync<CustomerNotified>();
        Assert.NotNull(consumedCustomerNotified);
        Assert.Equal(orderId, consumedCustomerNotified.OrderId);
        Assert.Contains(customerName, consumedCustomerNotified.NotificationMessage);
    }
}