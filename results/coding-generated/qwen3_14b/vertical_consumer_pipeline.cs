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
    public Task Consume(ConsumeContext<PlaceOrder> context)
    {
        return context.Publish(new OrderPlaced(context.Message.OrderId, context.Message.CustomerName));
    }
}

public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var message = context.Message;
        return context.Publish(new CustomerNotified(message.OrderId, $"Order {message.OrderId} confirmed for {message.CustomerName}"));
    }
}

public class ConsumerTests : IAsyncLifetime
{
    private ITestHarness _harness;

    public async ValueTask InitializeAsync()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        _harness = services.BuildServiceProvider().GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.Stop();
    }

    [Fact]
    public async Task PlaceOrderConsumer_PublishesOrderPlaced()
    {
        var order = new PlaceOrder(Guid.NewGuid(), "John Doe");
        await _harness.Bus.Publish(order);

        await _harness.Consumed.Any<PlaceOrder>();
        await _harness.Published.Any<OrderPlaced>();
    }

    [Fact]
    public async Task FullPipeline_PublishesCustomerNotified()
    {
        var order = new PlaceOrder(Guid.NewGuid(), "Jane Smith");
        await _harness.Bus.Publish(order);

        await _harness.Consumed.Any<PlaceOrder>();
        await _harness.Published.Any<OrderPlaced>();
        await _harness.Published.Any<CustomerNotified>();
    }
}