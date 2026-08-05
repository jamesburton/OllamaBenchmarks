global using Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using xunit;

namespace Contracts
{
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    public class PlaceOrderConsumer : IConsumer<PlaceOrder>
    {
        public async Task Consume(ConsumeContext<PlaceOrder> context)
        {
            var order = context.Message;
            await context.Publish(new OrderPlaced(order.OrderId, order.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
    {
        public async Task Consume(ConsumeContext<OrderPlaced> context)
        {
            var order = context.Message;
            await context.Publish(new CustomerNotified(order.OrderId, $"Order {order.OrderId} confirmed for {order.CustomerName}"));
        }
    }
}

public class OrderConsumerTests
{
    [Fact]
    public async Task PlaceOrderConsumer_PublishesOrderPlaced()
    {
        var services = new ServiceCollection();
        services.AddMassTransit(x =>
        {
            x.AddConsumer<PlaceOrderConsumer>();
            x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
        });
        var harness = services.BuildServiceProvider().GetRequiredService<ITestHarness>();
        await harness.Start();

        await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "John Doe"));
        Assert.True(await harness.Consumed.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipeline_PublishesCustomerNotified()
    {
        var services = new ServiceCollection();
        services.AddMassTransit(x =>
        {
            x.AddConsumer<PlaceOrderConsumer>();
            x.AddConsumer<NotifyCustomerConsumer>();
            x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
        });
        var harness = services.BuildServiceProvider().GetRequiredService<ITestHarness>();
        await harness.Start();

        await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Jane Smith"));
        Assert.True(await harness.Consumed.Any<OrderPlaced>());
        Assert.True(await harness.Consumed.Any<CustomerNotified>());
    }
}