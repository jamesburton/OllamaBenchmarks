using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

global using Contracts;

namespace Tests
{
    public class OrderPipelineTests
    {
        [Fact]
        public async Task PlaceOrderConsumerConsumesAndPublishesOrderPlaced()
        {
            var harness = await TestHarness.Start<PlaceOrderConsumer>(cfg => cfg.UsingInMemory((ctx, c) => c.ConfigureEndpoints(ctx)));

            try
            {
                await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Alice"));

                Assert.True(await harness.Consumed.Any<PlaceOrder>());
                Assert.True(await harness.Published.Any<OrderPlaced>());
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Fact]
        public async Task FullPipelineConsumesBothEvents()
        {
            var harness = await TestHarness.Start<PlaceOrderConsumer, NotifyCustomerConsumer>(cfg => cfg.UsingInMemory((ctx, c) => c.ConfigureEndpoints(ctx)));

            try
            {
                var orderId = Guid.NewGuid();
                var customerName = "Bob";
                await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

                Assert.True(await harness.Consumed.Any<PlaceOrder>());
                Assert.True(await harness.Published.Any<OrderPlaced>());
                Assert.True(await harness.Published.Any<CustomerNotified>());
            }
            finally
            {
                await harness.Stop();
            }
        }
    }
}