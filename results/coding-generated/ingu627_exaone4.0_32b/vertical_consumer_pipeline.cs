using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

global using Contracts;

namespace Contracts
{
    // Message records
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    // Consumers
    public class PlaceOrderConsumer : IConsumer<PlaceOrder>
    {
        public async Task Consume(ConsumeContext<PlaceOrder> context)
        {
            var msg = context.Message;
            await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
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

// Test class (no namespace)
public class MassTransitPipelineTests : IAsyncLifetime
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

        var provider = services.BuildServiceProvider();
        _harness = provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public ValueTask DisposeAsync()
    {
        return new ValueTask(_harness?.StopAsync());
    }

    [Fact]
    public async Task PlaceOrderConsumer_consumes_PlaceOrder_and_publishes_OrderPlaced()
    {
        var placeOrder = new PlaceOrder(Guid.NewGuid(), "John Doe");
        await _harness.Bus.Publish(placeOrder);

        // Wait for the message to be processed (we don't have explicit waiting, but the test harness will process synchronously in-memory)
        // We can use Task.Delay(100) to yield, but the harness is synchronous so we can just assert immediately?
        // However, the test harness runs in a background thread, so we might need to wait a bit.

        // Since the test harness is in-memory and synchronous, we can wait for the consumed and published to appear.
        // We'll use a small delay to be safe.
        await Task.Delay(100);

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipeline_publishes_PlaceOrder_and_results_in_CustomerNotified()
    {
        var placeOrder = new PlaceOrder(Guid.NewGuid(), "Jane Smith");
        await _harness.Bus.Publish(placeOrder);

        await Task.Delay(100);

        // Check the first consumer consumed PlaceOrder and published OrderPlaced
        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());

        // Now check the second consumer consumed OrderPlaced and published CustomerNotified
        Assert.True(await _harness.Consumed.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }
}

[Fact]
    public async Task PlaceOrderConsumer_consumes_PlaceOrder_and_publishes_OrderPlaced()
    {
        var placeOrder = new PlaceOrder(Guid.NewGuid(), "John Doe");
        await _harness.Bus.Publish(placeOrder);

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipeline_publishes_PlaceOrder_and_results_in_CustomerNotified()
    {
        var placeOrder = new PlaceOrder(Guid.NewGuid(), "Jane Smith");
        await _harness.Bus.Publish(placeOrder);

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Consumed.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

global using Contracts;

namespace Contracts
{
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    public class PlaceOrderConsumer : IConsumer<PlaceOrder>
    {
        public async Task Consume(ConsumeContext<PlaceOrder> context)
        {
            var msg = context.Message;
            await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
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

public class MassTransitPipelineTests : IAsyncLifetime
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

        var provider = services.BuildServiceProvider();
        _harness = provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public ValueTask DisposeAsync()
    {
        return new ValueTask(_harness?.StopAsync());
    }

    [Fact]
    public async Task PlaceOrderConsumer_consumes_PlaceOrder_and_publishes_OrderPlaced()
    {
        var placeOrder = new PlaceOrder(Guid.NewGuid(), "John Doe");
        await _harness.Bus.Publish(placeOrder);

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipeline_publishes_PlaceOrder_and_results_in_CustomerNotified()
    {
        var placeOrder = new PlaceOrder(Guid.NewGuid(), "Jane Smith");
        await _harness.Bus.Publish(placeOrder);

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Consumed.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }
}