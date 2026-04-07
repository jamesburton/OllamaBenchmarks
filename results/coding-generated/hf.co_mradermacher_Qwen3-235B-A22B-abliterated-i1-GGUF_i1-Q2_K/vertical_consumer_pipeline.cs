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
            var message = $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}";
            await context.Publish(new CustomerNotified(context.Message.OrderId, message));
        }
    }
}

public class ConsumerPipelineTests : IAsyncLifetime
{
    private readonly ServiceProvider _provider;

    public ConsumerPipelineTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });
        _provider = services.BuildServiceProvider();
    }

    public async ValueTask InitializeAsync() => await _provider.StartAsync();

    public async ValueTask DisposeAsync() => await _provider.StopAsync();

    [Fact]
    public async Task ShouldConsumePlaceOrderAndPublishOrderPlaced()
    {
        var harness = _provider.GetRequiredService<ITestHarness>();
        var orderId = Guid.NewGuid();
        await harness.Start();
        await harness.Bus.Publish(new PlaceOrder(orderId, "Test Customer"));
        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task ShouldCompleteFullPipeline()
    {
        var harness = _provider.GetRequiredService<ITestHarness>();
        var orderId = Guid.NewGuid();
        await harness.Start();
        await harness.Bus.Publish(new PlaceOrder(orderId, "Another Customer"));
        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());
    }
}