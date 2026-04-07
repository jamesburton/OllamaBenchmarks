global using Contracts;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts;

public record PlaceOrder(Guid OrderId, string CustomerName);
public record OrderPlaced(Guid OrderId, string CustomerName);
public record CustomerNotified(Guid OrderId, string NotificationMessage);

public class PlaceOrderConsumer : IConsumer<PlaceOrder>
{
    public async Task Consume(ConsumeContext<PlaceOrder> context)
    {
        await context.Publish(new OrderPlaced(
            context.Message.OrderId, 
            context.Message.CustomerName));
    }
}

public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        await context.Publish(new CustomerNotified(
            context.Message.OrderId,
            $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"));
    }
}

public class OrderPipelineTests
{
    [Fact]
    public async Task PlaceOrder_should_be_consumed_and_publish_OrderPlaced()
    {
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<PlaceOrderConsumer>();
                cfg.AddConsumer<NotifyCustomerConsumer>();
            })
            .BuildServiceProvider();

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            var orderId = Guid.NewGuid();
            await harness.Bus.Publish(new PlaceOrder(orderId, "Test Customer"));

            Assert.True(await harness.Consumed.Any<PlaceOrder>(x => x.Message.OrderId == orderId));
            Assert.True(await harness.Published.Any<OrderPlaced>(x => x.Message.OrderId == orderId));
        }
        finally
        {
            await harness.Stop();
            provider.Dispose();
        }
    }

    [Fact]
    public async Task Full_pipeline_should_produce_CustomerNotified()
    {
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<PlaceOrderConsumer>();
                cfg.AddConsumer<NotifyCustomerConsumer>();
            })
            .BuildServiceProvider();

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            var orderId = Guid.NewGuid();
            await harness.Bus.Publish(new PlaceOrder(orderId, "Test Customer"));

            Assert.True(await harness.Published.Any<OrderPlaced>(x => x.Message.OrderId == orderId));
            Assert.True(await harness.Consumed.Any<OrderPlaced>(x => x.Message.OrderId == orderId));
            Assert.True(await harness.Published.Any<CustomerNotified>(x => x.Message.OrderId == orderId));
        }
        finally
        {
            await harness.Stop();
            provider.Dispose();
        }
    }
}