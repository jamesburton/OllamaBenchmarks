using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

global using Contracts;

namespace Contracts
{
    // 1. Message Records
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);
}

namespace Contracts
{
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
            // Publish CustomerNotified event
            var notification = $"Order {msg.OrderId} confirmed for {msg.CustomerName}";
            await context.Publish(new CustomerNotified(msg.OrderId, notification));
        }
    }
}

namespace MassTransitPipelineTests
{
    using Contracts;

    public class ConsumerPipelineTests
    {
        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register consumers
            services.AddMassTransit(x =>
            {
                // Register the consumers
                x.AddConsumer<PlaceOrderConsumer>();
                x.AddConsumer<NotifyCustomerConsumer>();
            });

            // Add the test harness for tracking published/consumed messages
            services.AddMassTransitTestHarness(cfg =>
            {
                // Register the consumers again for the harness to track them
                cfg.AddConsumer<PlaceOrderConsumer>();
                cfg.AddConsumer<NotifyCustomerConsumer>();
            });

            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task Test_PlaceOrder_PublishesOrderPlacedEvent()
        {
            // Arrange
            var services = ConfigureServices();
            var harness = services.GetRequiredService<ITestHarness>();

            var orderId = Guid.NewGuid();
            var customerName = "Alice";
            var placeOrderMessage = new PlaceOrder(orderId, customerName);

            // Act: Publish the initial message
            await harness.Bus.Publish(placeOrderMessage);

            // Assert: Verify that the PlaceOrderConsumer consumed the message
            Assert.True(await harness.Consumed.Any<PlaceOrder>());

            // Assert: Verify that the PlaceOrderConsumer published the OrderPlaced event
            Assert.True(await harness.Published.Any<OrderPlaced>());
        }

        [Fact]
        public async Task Test_FullPipeline_PlaceOrder_To_CustomerNotified()
        {
            // Arrange
            var services = ConfigureServices();
            var harness = services.GetRequiredService<ITestHarness>();

            var orderId = Guid.NewGuid();
            var customerName = "Bob";
            var placeOrderMessage = new PlaceOrder(orderId, customerName);

            // Act: Publish the initial message
            await harness.Bus.Publish(placeOrderMessage);

            // Assert 1: Verify the first stage (PlaceOrderConsumer) completed its work
            Assert.True(await harness.Consumed.Any<PlaceOrder>());
            Assert.True(await harness.Published.Any<OrderPlaced>());

            // Assert 2: Verify the second stage (NotifyCustomerConsumer) completed its work
            // It should have consumed OrderPlaced and published CustomerNotified
            Assert.True(await harness.Consumed.Any<OrderPlaced>());
            Assert.True(await harness.Published.Any<CustomerNotified>());
        }
    }
}