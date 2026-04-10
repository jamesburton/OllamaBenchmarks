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
        var msg = context.Message;
        await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
    }
}

public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var msg = context.Message;
        var notificationMessage = $"Order {msg.OrderId} confirmed for {msg.CustomerName}";
        await context.Publish(new CustomerNotified(msg.OrderId, notificationMessage));
    }
}

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class ConsumerPipelineTests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private ITestHarness _harness = null!;

    public async ValueTask InitializeAsync()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<PlaceOrderConsumer>();
                cfg.AddConsumer<NotifyCustomerConsumer>();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task PlaceOrderConsumer_Consumes_And_Publishes_OrderPlaced()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var placeOrder = new PlaceOrder(orderId, "Test Customer");

        // Act
        await _harness.Bus.Publish(placeOrder);

        // Assert
        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());

        var published = _harness.Published.Select<OrderPlaced>().FirstOrDefault();
        Assert.NotNull(published);
        Assert.Equal(orderId, published.Context.Message.OrderId);
        Assert.Equal("Test Customer", published.Context.Message.CustomerName);
    }

    [Fact]
    public async Task FullPipeline_Publishes_Both_OrderPlaced_And_CustomerNotified()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var placeOrder = new PlaceOrder(orderId, "Pipeline Customer");

        // Act
        await _harness.Bus.Publish(placeOrder);

        // Assert
        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Consumed.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());

        var customerNotified = _harness.Published.Select<CustomerNotified>().FirstOrDefault();
        Assert.NotNull(customerNotified);
        Assert.Equal(orderId, customerNotified.Context.Message.OrderId);
        Assert.Equal($"Order {orderId} confirmed for Pipeline Customer", 
            customerNotified.Context.Message.NotificationMessage);
    }
}