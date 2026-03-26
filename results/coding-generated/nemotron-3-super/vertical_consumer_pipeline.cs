global using Contracts;
using MassTransit;
using System;
using System.Threading.Tasks;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Contracts
{
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    public class PlaceOrderConsumer : IConsumer<PlaceOrder>
    {
        public Task Consume(ConsumeContext<PlaceOrder> context)
        {
            var msg = context.Message;
            return context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
    {
        public Task Consume(ConsumeContext<OrderPlaced> context)
        {
            var msg = context.Message;
            var notification = $"Order {msg.OrderId} confirmed for {msg.CustomerName}";
            return context.Publish(new CustomerNotified(msg.OrderId, notification));
        }
    }
}

public class PipelineTests
{
    [Fact]
    public async Task PlaceOrderConsumer_publishes_OrderPlaced()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new PlaceOrder(orderId, "Alice"));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());

        await harness.Stop();
    }

    [Fact]
    public async Task FullPipeline()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new PlaceOrder(orderId, "Bob"));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Consumed.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());

        await harness.Stop();
    }
}