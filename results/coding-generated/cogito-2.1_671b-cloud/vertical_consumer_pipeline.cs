global using MassTransit;

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
            $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"
        ));
    }
}

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class OrderPipelineTests : IAsyncLifetime
{
    private ServiceProvider _provider;
    private ITestHarness _harness;

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

    [Fact]
    public async Task PlaceOrderConsumer_Should_Publish_OrderPlaced()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerName = "Test Customer";

        // Act
        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        // Assert
        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipeline_Should_Notify_Customer()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerName = "Test Customer";

        // Act
        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        // Assert
        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());

        var customerNotified = _harness.Published.Select<CustomerNotified>().FirstOrDefault()?.Context.Message;
        Assert.NotNull(customerNotified);
        Assert.Equal(orderId, customerNotified.OrderId);
        Assert.Equal($"Order {orderId} confirmed for {customerName}", customerNotified.NotificationMessage);
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }
}