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
    public async Task TestFirstStage()
    {
        var harness = new TestHarness(
            services =>
            {
                services.AddMassTransitTestHarness(cfg => cfg
                    .AddConsumer<PlaceOrderConsumer>()
                    .AddConsumer<NotifyCustomerConsumer>());
            });

        await harness.Start();

        await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "John Doe"));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());

        await harness.Stop();
    }

    [Fact]
    public async Task TestFullPipeline()
    {
        var harness = new TestHarness(
            services =>
            {
                services.AddMassTransitTestHarness(cfg => cfg
                    .AddConsumer<PlaceOrderConsumer>()
                    .AddConsumer<NotifyCustomerConsumer>());
            });

        await harness.Start();

        await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Jane Smith"));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());

        await harness.Stop();
    }
}