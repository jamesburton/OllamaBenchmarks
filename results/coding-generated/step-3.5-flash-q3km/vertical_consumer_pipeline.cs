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

public class OrderPipelineTests
{
    private readonly ITestHarness _harness;

    public OrderPipelineTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        var provider = services.BuildServiceProvider();
        _harness = provider.GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task PlaceOrder_Should_Publish_OrderPlaced()
    {
        await _harness.Start();
        try
        {
            await _harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "John Doe"));

            Assert.True(await _harness.Consumed.Any<PlaceOrder>());
            Assert.True(await _harness.Published.Any<OrderPlaced>());
        }
        finally
        {
            await _harness.Stop();
        }
    }

    [Fact]
    public async Task FullPipeline_Should_Publish_Both_Events()
    {
        await _harness.Start();
        try
        {
            await _harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Jane Doe"));

            Assert.True(await _harness.Published.Any<OrderPlaced>());
            Assert.True(await _harness.Published.Any<CustomerNotified>());
        }
        finally
        {
            await _harness.Stop();
        }
    }
}