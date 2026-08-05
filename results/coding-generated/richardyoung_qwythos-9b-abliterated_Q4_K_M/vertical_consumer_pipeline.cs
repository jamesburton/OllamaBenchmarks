using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

global using Contracts;

namespace TestApp
{
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
            var message = $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}";
            await context.Publish(new CustomerNotified(context.Message.OrderId, message));
        }
    }

    [CollectionDefinition("MassTransit")]
    public class MassTransitCollection : ICollectionFixture<ITestHarness>
    {
        // no need to declare anything here; the fixture is injected automatically
    }

    public class PlaceOrderTests : IClassFixture<ITestHarness>, IDisposable
    {
        private readonly ITestHarness _harness;

        public PlaceOrderTests(ITestHarness harness) => _harness = harness;

        [Fact]
        public async Task PublishPlaceOrder_ConsumedByPlaceOrderConsumer_PublishesOrderPlaced()
        {
            await _harness.Start();

            var orderId = Guid.NewGuid();
            var customerName = "Alice";

            await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

            Assert.True(await _harness.Consumed.Any<PlaceOrder>());
            Assert.True(await _harness.Published.Any<OrderPlaced>());

            // Verify the published OrderPlaced has correct data
            var orderPlaced = await _harness.Published.GetFirstOrDefault<OrderPlaced>();
            Assert.NotNull(orderPlaced);
            Assert.Equal(orderId, orderPlaced.OrderId);
            Assert.Equal(customerName, orderPlaced.CustomerName);

            await _harness.Stop();
        }

        [Fact]
        public async Task FullPipeline_PublishPlaceOrder_ConsumedByBothConsumers()
        {
            await _harness.Start();

            var orderId = Guid.NewGuid();
            var customerName = "Bob";

            await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

            // Ensure both consumers ran
            Assert.True(await _harness.Consumed.Any<PlaceOrder>());
            Assert.True(await _harness.Consumed.Any<OrderPlaced>());

            // Verify final published message
            var notified = await _harness.Published.GetFirstOrDefault<CustomerNotified>();
            Assert.NotNull(notified);
            Assert.Equal(orderId, notified.OrderId);
            Assert.Equal($"Order {orderId} confirmed for {customerName}", notified.NotificationMessage);

            await _harness.Stop();
        }

        public void Dispose() => _harness.DisposeAsync().AsTask().Wait();
    }
}