using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

global using Contracts;

namespace
{
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

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
            string notificationMessage = $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}";
            await context.Publish(new CustomerNotified(context.Message.OrderId, notificationMessage));
        }
    }

    public class MassTransitIntegrationTests
    {
        [Fact]
        public async Task PlaceOrderConsumer_Publishes_OrderPlaced()
        {
            var harness = CreateTestHarness();
            await harness.Start();

            var orderId = Guid.NewGuid();
            var customerName = "Test Customer";
            var message = new PlaceOrder(orderId, customerName);

            await harness.Bus.Publish(message);

            Assert.True(await harness.Consumed.Any<OrderPlaced>());
        }

        [Fact]
        public async Task FullPipeline_Publishes_OrderPlaced_And_CustomerNotified()
        {
            var harness = CreateTestHarness();
            await harness.Start();

            var orderId = Guid.NewGuid();
            var customerName = "Test Customer";
            var message = new PlaceOrder(orderId, customerName);

            await harness.Bus.Publish(message);

            Assert.True(await harness.Consumed.Any<OrderPlaced>());
            Assert.True(await harness.Consumed.Any<CustomerNotified>());

            var publishedCustomerNotified = await harness.Published.Any<CustomerNotified>();
            Assert.True(publishedCustomerNotified);

            var publishedCustomerNotifiedEvent = await harness.Published.First<CustomerNotified>();
            Assert.Equal(orderId, publishedCustomerNotifiedEvent.OrderId);
            Assert.Equal($"Order {orderId} confirmed for {customerName}", publishedCustomerNotifiedEvent.NotificationMessage);
        }

        private ITestHarness CreateTestHarness()
        {
            var services = new ServiceCollection();
            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<PlaceOrderConsumer>();
                cfg.AddConsumer<NotifyCustomerConsumer>();
            });

            return services.BuildServiceProvider()
                .GetRequiredService<ITestHarness>();
        }
    }
}