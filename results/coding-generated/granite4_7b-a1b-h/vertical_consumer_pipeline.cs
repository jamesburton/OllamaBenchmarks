using System;
using MassTransit;
using MassTransit.Testing;
using MassTransit.v8;
using Xunit;

namespace Contracts
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
            await context.Publish(new CustomerNotified(context.Message.OrderId, $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"));
        }
    }

    public class TestPipeline
    {
        [Fact]
        public async Task TestPlaceOrderPipeline()
        {
            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var placeOrderMessage = new PlaceOrder(Guid.NewGuid(), "John Doe");
            harness.Bus.Publish(placeOrderMessage);

            Assert.True(await harness.Consumed.Any<PlaceOrder>());
            Assert.True(await harness.Published.Any<OrderPlaced>());

            var orderPlaced = await harness.Published.First<OrderPlaced>();
            Assert.Equal(placeOrderMessage.OrderId, orderPlaced.OrderId);
            Assert.Equal(placeOrderMessage.CustomerName, orderPlaced.CustomerName);

            await harness.Stop();
        }

        [Fact]
        public async Task TestFullPipeline()
        {
            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var placeOrderMessage = new PlaceOrder(Guid.NewGuid(), "Jane Doe");
            harness.Bus.Publish(placeOrderMessage);

            Assert.True(await harness.Consumed.Any<PlaceOrder>());
            Assert.True(await harness.Published.Any<OrderPlaced>());

            var orderPlaced = await harness.Published.First<OrderPlaced>();
            Assert.Equal(placeOrderMessage.OrderId, orderPlaced.OrderId);
            Assert.Equal(placeOrderMessage.CustomerName, orderPlaced.CustomerName);

            var orderPlacedMessage = new OrderPlaced(orderPlaced.OrderId, "Confirmed");
            harness.Bus.Publish(orderPlacedMessage);

            Assert.True(await harness.Consumed.Any<OrderPlaced>());
            Assert.True(await harness.Consumed.Any<CustomerNotified>());

            var customerNotified = await harness.Consumed.First<CustomerNotified>();
            Assert.Equal(orderPlaced.OrderId, customerNotified.OrderId);
            Assert.Equal($"Order {orderPlaced.OrderId} confirmed for {orderPlaced.CustomerName}", customerNotified.NotificationMessage);

            await harness.Stop();
        }
    }
}