using System;
using MassTransit;
using MassTransit.Testing;

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
        public void PlaceOrder_Produces_OrderPlaced_And_Notification()
        {
            var harness = new TestHarnessBuilder()
                .AddConsumer<PlaceOrderConsumer>()
                .AddConsumer<NotifyCustomerConsumer>()
                .Build();

            harness.Start();

            harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "John Doe"));

            Assert.True(harness.Consumed.Any<OrderPlaced>());
            Assert.True(harness.Published.Any<CustomerNotified>());

            harness.Stop();
        }
    }
}