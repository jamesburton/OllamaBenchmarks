global using Contracts;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System;
using System.Threading.Tasks;

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
        var message = context.Message;
        // Publish OrderPlaced event
        await context.Publish(new OrderPlaced(message.OrderId, message.CustomerName));
    }
}

// 3. NotifyCustomerConsumer
public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var message = context.Message;
        var notificationMessage = $"Order {message.OrderId} confirmed for {message.CustomerName}";

        // Publish CustomerNotified event
        await context.Publish(new CustomerNotified(message.OrderId, notificationMessage));
    }
}

// Test Class (Outside the Contracts namespace)
public class PipelineTests
{
    private ServiceProvider SetupServices()
    {
        var services = new ServiceCollection();
        services.AddMassTransit(x =>
        {
            x.AddConsumer<PlaceOrderConsumer>();
            x.AddConsumer<NotifyCustomerConsumer>();
            // Configure endpoints if necessary, but for testing, just adding consumers is enough
        });
        services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<PlaceOrderConsumer>()
                                                                .AddConsumer<NotifyCustomerConsumer>());
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task PlaceOrderConsumer_ShouldPublishOrderPlaced()
    {
        // Arrange
        var services = SetupServices();
        var harness = services.GetRequiredService<ITestHarness>();
        var testOrderId = Guid.NewGuid();
        var testCustomerName = "Alice";

        await harness.Start();

        // Act: Publish the initial message
        await harness.Bus.Publish(new PlaceOrder(testOrderId, testCustomerName));

        // Assert 1: Verify PlaceOrderConsumer consumed the message
        await Assert.True(await harness.Consumed.Any<PlaceOrder>());

        // Assert 2: Verify PlaceOrderConsumer published OrderPlaced
        await Assert.True(await harness.Published.Any<OrderPlaced>(p => 
            p.OrderId == testOrderId && p.CustomerName == testCustomerName));
    }

    [Fact]
    public async Task FullPipeline_ShouldProcessOrderAndNotifyCustomer()
    {
        // Arrange
        var services = SetupServices();
        var harness = services.GetRequiredService<ITestHarness>();
        var testOrderId = Guid.NewGuid();
        var testCustomerName = "Bob";

        await harness.Start();

        // Act: Publish the initial message
        await harness.Bus.Publish(new PlaceOrder(testOrderId, testCustomerName));

        // Wait for the pipeline to complete (MassTransit test harness handles this implicitly, 
        // but we wait briefly to ensure async processing completes before assertions)
        await Task.Delay(100); 

        // Assert 1: Verify PlaceOrderConsumer consumed PlaceOrder
        await Assert.True(await harness.Consumed.Any<PlaceOrder>());

        // Assert 2: Verify PlaceOrderConsumer published OrderPlaced
        var publishedOrderPlaced = await harness.Published.FirstOrDefaultAsync<OrderPlaced>(p => 
            p.OrderId == testOrderId && p.CustomerName == testCustomerName);
        await Assert.NotNull(publishedOrderPlaced);

        // Assert 3: Verify NotifyCustomerConsumer consumed OrderPlaced
        await Assert.True(await harness.Consumed.Any<OrderPlaced>());

        // Assert 4: Verify NotifyCustomerConsumer published CustomerNotified
        var publishedCustomerNotified = await harness.Published.FirstOrDefaultAsync<CustomerNotified>(p => 
            p.OrderId == testOrderId && p.NotificationMessage == $"Order {testOrderId} confirmed for {testCustomerName}");
        await Assert.NotNull(publishedCustomerNotified);
    }
}