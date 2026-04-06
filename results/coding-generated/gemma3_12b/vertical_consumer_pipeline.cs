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

    public class MassTransitFixture : IAsyncLifetime
    {
        private ITestHarness _harness;

        public ITestHarness Harness => _harness;

        public async Task InitializeAsync()
        {
            var services = new ServiceCollection();
            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<PlaceOrderConsumer>();
                cfg.AddConsumer<NotifyCustomerConsumer>();
            });

            _harness = services.BuildServiceProvider()
                .GetRequiredService<ITestHarness>();

            await _harness.Start();
        }

        public async Task DisposeAsync()
        {
            await _harness.Stop();
        }
    }

    [CollectionDefinition("MassTransitFixture")]
    public class MassTransitCollection : ICollectionFixture<MassTransitFixture> { }

    [MassTransitCollection]
    public class MassTransitTests : IDisposable
    {
        private readonly MassTransitFixture _fixture;

        public MassTransitTests(MassTransitFixture fixture)
        {
            _fixture = fixture;
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }

        [Fact]
        public async Task PlaceOrderConsumer_Publishes_OrderPlaced()
        {
            Guid orderId = Guid.NewGuid();
            string customerName = "Test Customer";
            var message = new PlaceOrder(orderId, customerName);

            await _fixture.Harness.Bus.Publish(message);

            Assert.True(await _fixture.Harness.Consumed.Any<OrderPlaced>());
        }

        [Fact]
        public async Task FullPipeline_Publishes_CustomerNotified()
        {
            Guid orderId = Guid.NewGuid();
            string customerName = "Test Customer";
            var message = new PlaceOrder(orderId, customerName);

            await _fixture.Harness.Bus.Publish(message);

            Assert.True(await _fixture.Harness.Consumed.Any<OrderPlaced>());
            Assert.True(await _fixture.Harness.Published.Any<CustomerNotified>());

            var publishedCustomerNotified = await _fixture.Harness.Published.SingleOrDefaultAsync<CustomerNotified>();
            Assert.NotNull(publishedCustomerNotified);
            Assert.Equal(orderId, publishedCustomerNotified.OrderId);
            Assert.Equal($"Order {orderId} confirmed for {customerName}", publishedCustomerNotified.NotificationMessage);
        }
    }
}