using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

global using Contracts;

namespace MassTransitPipelineTest
{
    // 1. Message Records (Contracts Namespace)
    namespace Contracts
    {
        public record PlaceOrder(Guid OrderId, string CustomerName);
        public record OrderPlaced(Guid OrderId, string CustomerName);
        public record CustomerNotified(Guid OrderId, string NotificationMessage);
    }

    // 2. Consumers (Contracts Namespace)
    namespace Contracts
    {
        public class PlaceOrderConsumer : IConsumer<PlaceOrder>
        {
            public async Task Consume(ConsumeContext<PlaceOrder> context)
            {
                var msg = context.Message;
                // Publish OrderPlaced event
                await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
            }
        }

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

    // 4. xUnit v3 Test Class
    public class ConsumerPipelineTests
    {
        [Fact]
        public async Task FullPipeline_PlaceOrder_To_CustomerNotified()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddMassTransit(x =>
                {
                    // Register the consumers
                    x.AddConsumer<Contracts.PlaceOrderConsumer>();
                    x.AddConsumer<Contracts.NotifyCustomerConsumer>();
                    // Use in-memory transport for testing
                    x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
                })
                .BuildServiceProvider();

            // Setup Test Harness
            var harness = serviceProvider.GetRequiredService<ITestHarness>();
            await harness.Start();

            // Define input data
            var orderId = Guid.NewGuid();
            var customerName = "Alice Smith";
            var placeOrderMessage = new Contracts.PlaceOrder(orderId, customerName);

            // Act: Publish the initial message
            await harness.Bus.Publish(placeOrderMessage);

            // Assert: Verify the flow

            // 1. Verify that the initial message was consumed by PlaceOrderConsumer
            Assert.True(await harness.Consumed.Any<Contracts.PlaceOrder>());

            // 2. Verify that the intermediate event was published by PlaceOrderConsumer
            Assert.True(await harness.Published.Any<Contracts.OrderPlaced>());

            // 3. Verify that the final event was published by NotifyCustomerConsumer
            Assert.True(await harness.Published.Any<Contracts.CustomerNotified>());

            // Optional: Verify the specific content of the final notification
            var notifiedMessages = harness.Published.OfType<Contracts.CustomerNotified>().ToList();
            Assert.Single(notifiedMessages);
            Assert.Equal(orderId, notifiedMessages[0].OrderId);
            Assert.Contains(customerName, notifiedMessages[0].NotificationMessage);
        }
    }
}