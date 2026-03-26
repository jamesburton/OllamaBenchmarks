global using Contracts;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Contracts;

// Message records
public record PlaceOrder(Guid OrderId, string CustomerName);
public record OrderPlaced(Guid OrderId, string CustomerName);
public record CustomerNotified(Guid OrderId, string NotificationMessage);

// Consumers
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
        var message = context.Message;
        var notification = $"Order {message.OrderId} confirmed for {message.CustomerName}";
        await context.Publish(new CustomerNotified(message.OrderId, notification));
    }
}

// Tests
public class MassTransitPipelineTests : IAsyncLifetime
{
    private readonly IServiceProvider _provider;
    private readonly ITestHarness _harness;

    public MassTransitPipelineTests()
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
    public async Task PlaceOrderConsumer_Should_Consume_And_Publish_OrderPlaced()
    {
        var orderId = Guid.NewGuid();
        await _harness.Bus.Publish(new PlaceOrder(orderId, "Alice"));

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task Full_Pipeline_Should_Process_And_Publish_All_Messages()
    {
        var orderId = Guid.NewGuid();
        await _harness.Bus.Publish(new PlaceOrder(orderId, "Bob"));

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }
}