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
        public Task Consume(ConsumeContext<PlaceOrder> context)
        {
            return context.Publish(new OrderPlaced(
                context.Message.OrderId,
                context.Message.CustomerName
            ));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
    {
        public Task Consume(ConsumeContext<OrderPlaced> context)
        {
            var notification = $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}";
            return context.Publish(new CustomerNotified(
                context.Message.OrderId,
                notification
            ));
        }
    }
}

public class PipelineTests : IAsyncLifetime
{
    private ITestHarness _harness;

    public ValueTask InitializeAsync()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });
        var provider = services.BuildServiceProvider();
        _harness = provider.GetRequiredService<ITestHarness>();
        return _harness.Start();
    }

    public ValueTask DisposeAsync()
    {
        return _harness.Stop();
    }

    [Fact]
    public async Task PlaceOrderConsumer_PublishesOrderPlaced()
    {
        await _harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Test Customer"));

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipeline_PublishesBothEvents()
    {
        var orderId = Guid.NewGuid();
        await _harness.Bus.Publish(new PlaceOrder(orderId, "Test Customer"));

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Consumed.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }
}