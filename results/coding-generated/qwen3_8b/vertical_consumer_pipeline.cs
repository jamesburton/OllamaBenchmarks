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
            await context.Publish(new CustomerNotified(context.Message.OrderId, $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"));
        }
    }
}

public class OrderTests : IAsyncLifetime
{
    private readonly ITestHarness _harness;

    public OrderTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });
        _harness = services.BuildServiceProvider().GetRequiredService<ITestHarness>();
    }

    public async ValueTask InitializeAsync() => await _harness.Start();
    public async ValueTask DisposeAsync() => await _harness.Stop();

    [Fact]
    public async Task PlaceOrder_ConsumedAndOrderPlacedPublished()
    {
        await _harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Alice"));

        await _harness.Consumed.Any<PlaceOrder>();
        await _harness.Published.Any<OrderPlaced>();
    }

    [Fact]
    public async Task FullPipeline_OrderPlacedAndCustomerNotifiedPublished()
    {
        await _harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Bob"));

        await _harness.Published.Any<OrderPlaced>();
        await _harness.Published.Any<CustomerNotified>();
    }
}