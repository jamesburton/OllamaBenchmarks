global using Contracts;


using Contracts;


using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;


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
        var notificationMessage = $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}";
        await context.Publish(new CustomerNotified(context.Message.OrderId, notificationMessage));
    }
}

public class ConsumerPipelineTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITestHarness _harness;

    public ConsumerPipelineTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<PlaceOrderConsumer>());
        services.AddMassTransit(cfg => cfg.AddConsumer<NotifyCustomerConsumer>());
        services.AddSingleton<ITestHarness>(provider => provider.GetRequiredService<ITestHarness>());
        _serviceProvider = services.BuildServiceProvider();
        _harness = _serviceProvider.GetRequiredService<ITestHarness>();
        _harness.ConfigureEndpoints(cfg => cfg.AddConsumer<PlaceOrderConsumer>());
    }

    [Fact]
    public async Task PlaceOrder_PublishesOrderPlacedAndNotifiesCustomer()
    {
        await _harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "Alice";

        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await _harness.Consumed.Any<OrderPlaced>());
        Assert.True(await _harness.Consumed.Any<CustomerNotified>());
    }
}