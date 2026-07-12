global using Contracts;
using MassTransit;

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
        var message = $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}";
        await context.Publish(new CustomerNotified(context.Message.OrderId, message));
    }
}

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ConsumerPipelineTests : IAsyncLifetime
{
    private readonly IServiceProvider _provider;
    private ITestHarness _harness;

    public ConsumerPipelineTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });
        _provider = services.BuildServiceProvider();
        _harness = _provider.GetRequiredService<ITestHarness>();
    }

    public async ValueTask InitializeAsync()
    {
        await _harness.Start();
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.Stop();
    }

    [Fact]
    public async Task PlaceOrderConsumer_Publishes_OrderPlaced()
    {
        var orderId = Guid.NewGuid();
        await _harness.Bus.Publish(new PlaceOrder(orderId, "Test Customer"));

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipeline_Publishes_CustomerNotified()
    {
        var orderId = Guid.NewGuid();
        await _harness.Bus.Publish(new PlaceOrder(orderId, "Test Customer"));

        Assert.True(await _harness.Consumed.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }
}