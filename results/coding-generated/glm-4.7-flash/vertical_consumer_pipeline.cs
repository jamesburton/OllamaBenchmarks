global using Contracts;
using MassTransit;

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
            var msg = context.Message;
            await context.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }
}

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class PipelineTests
{
    private readonly ITestHarness _harness;

    public PipelineTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });
        _harness = services.BuildServiceProvider().GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task FirstStage_Consumes_PlaceOrder_And_Publishes_OrderPlaced()
    {
        await _harness.Start();

        var orderId = Guid.NewGuid();
        await _harness.Bus.Publish(new PlaceOrder(orderId, "Alice"));

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipeline_Consumes_OrderPlaced_And_Publishes_CustomerNotified()
    {
        await _harness.Start();

        var orderId = Guid.NewGuid();
        await _harness.Bus.Publish(new PlaceOrder(orderId, "Alice"));

        Assert.True(await _harness.Consumed.Any<OrderPlaced>());
        Assert.True(await _harness.Consumed.Any<CustomerNotified>());
    }
}