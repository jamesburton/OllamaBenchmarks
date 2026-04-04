global using Contracts;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Contracts;

public record PlaceOrder(Guid OrderId, string CustomerName);
public record OrderPlaced(Guid OrderId, string CustomerName);
public record CustomerNotified(Guid OrderId, string NotificationMessage);

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
        await context.Publish(new CustomerNotified(
            msg.OrderId,
            $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
    }
}

public class OrderPipelineTests
{
    [Fact]
    public async Task PlaceOrder_ShouldBeConsumedAndPublishOrderPlaced()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        try
        {
            var orderId = Guid.NewGuid();
            await harness.Bus.Publish(new PlaceOrder(orderId, "John Doe"));

            Assert.True(await harness.Consumed.Any<PlaceOrder>());
            Assert.True(await harness.Published.Any<OrderPlaced>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task FullPipeline_ShouldProduceCustomerNotified()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        try
        {
            var orderId = Guid.NewGuid();
            await harness.Bus.Publish(new PlaceOrder(orderId, "Jane Doe"));

            Assert.True(await harness.Consumed.Any<PlaceOrder>());
            Assert.True(await harness.Published.Any<OrderPlaced>());
            Assert.True(await harness.Consumed.Any<OrderPlaced>());
            Assert.True(await harness.Published.Any<CustomerNotified>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}