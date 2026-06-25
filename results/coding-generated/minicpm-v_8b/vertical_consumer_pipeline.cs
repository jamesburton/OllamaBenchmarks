using Contracts;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace MyTests
{
    public class PlaceOrderConsumer : IConsumer<PlaceOrder>
    {
        public async Task Consume(ConsumeContext<PlaceOrder> context)
        {
            var msg = context.Message;
            await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
    {
        public async Task Consume(ConsumeContext<OrderPlaced> context)
        {
            var msg = context.Message;
            await context.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }

    [Fact]
    public async Task Test()
    {
        using (var bus = Bus.Create())
        {
            // Add consumers
            var harness = bus.GetRequiredService<ITestHarness>();
            harness.AddConsumer<PlaceOrderConsumer>();
            harness.AddConsumer<NotifyCustomerConsumer>();

            // Publish a PlaceOrder message and verify it's consumed by the consumer.
            await bus.Publish(new PlaceOrder(Guid.NewGuid(), "John Doe"));
            Assert.True(harness.Consumed.Any<PlaceOrder>());

            // Verify that both OrderPlaced and CustomerNotified are eventually published
            var orderPlaced = harness.Published.FirstOrDefault(p => p.MessageType == typeof(OrderPlaced));
            Assert.NotNull(orderPlaced);
            var customerNotified = harness.Published.FirstOrDefault(p => p.MessageType == typeof(CustomerNotified));
            Assert.NotNull(customerNotified);

            // Verify the contents of OrderPlaced and CustomerNotified events
            var orderPlacedEvent = (OrderPlaced)orderPlaced.Message;
            Assert.Equal(orderPlacedEvent.OrderId, Guid.NewGuid());
            Assert.Equal(orderPlacedEvent.CustomerName, "John Doe");

            var customerNotifiedEvent = (CustomerNotified)customerNotified.Message;
            Assert.Equal(customerNotifiedEvent.OrderId, orderPlacedEvent.OrderId);
            Assert.Equal(customerNotifiedEvent.NotificationMessage, $"Order {orderPlacedEvent.OrderId} confirmed for John Doe");
        }
    }
}