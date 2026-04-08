global using Contracts;
using MassTransit;
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
        public async Task Consume(ConsumeContext<PlaceOrder> context)
        {
            await context.Publish(new OrderPlaced(context.Message.OrderId, context.Message.CustomerName));
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
}

public class OrderPipelineTests
{
    [Fact]
    public async Task PlaceOrderConsumer_ShouldConsumePlaceOrderAndPublishOrderPlaced()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<PlaceOrderConsumer>();
                cfg.AddConsumer<NotifyCustomerConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Alice"));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());

        await harness.Stop();
    }

    [Fact]
    public async Task FullPipeline_ShouldConsumeAndPublishAllEvents()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<PlaceOrderConsumer>();
                cfg.AddConsumer<NotifyCustomerConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Alice"));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Consumed.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());

        await harness.Stop();
    }
}