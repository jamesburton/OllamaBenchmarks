global using MassTransit;
global using MassTransit.Testing;
global using Microsoft.Extensions.DependencyInjection;

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

public class OrderConsumerPipelineTests : IAsyncLifetime
{
    private readonly IServiceProvider _serviceProvider;

    public OrderConsumerPipelineTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task InitializeAsync()
    {
        await _serviceProvider.GetRequiredService<ITestHarness>().Start();
    }

    public async Task DisposeAsync()
    {
        await _serviceProvider.GetRequiredService<ITestHarness>().Stop();
    }

    [Fact]
    public async Task PlaceOrder_consumer_publishes_OrderPlaced()
    {
        var harness = _serviceProvider.GetRequiredService<ITestHarness>();
        var orderId = Guid.NewGuid();
        var customerName = "Alice";

        await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        await Task.Delay(100); // Allow time for consumer to process

        Assert.True(harness.Consumed.Any<PlaceOrder>());
        Assert.True(harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task Full_pipeline_publishes_CustomerNotified()
    {
        var harness = _serviceProvider.GetRequiredService<ITestHarness>();
        var orderId = Guid.NewGuid();
        var customerName = "Bob";

        await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        await Task.Delay(100); // Allow time for both consumers to process

        Assert.True(harness.Consumed.Any<PlaceOrder>());
        Assert.True(harness.Consumed.Any<OrderPlaced>());
        Assert.True(harness.Consumed.Any<CustomerNotified>());
        Assert.True(harness.Published.Any<CustomerNotified>());
    }
}