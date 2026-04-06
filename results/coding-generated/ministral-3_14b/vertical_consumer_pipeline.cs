global using MassTransit;
global using Contracts;

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
        var orderPlaced = new OrderPlaced(context.Message.OrderId, context.Message.CustomerName);
        await context.Publish(orderPlaced);
    }
}

public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var notificationMessage = $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}";
        var customerNotified = new CustomerNotified(context.Message.OrderId, notificationMessage);
        await context.Publish(customerNotified);
    }
}

public class ConsumerPipelineTests
{
    private readonly IServiceProvider _provider;
    private readonly ITestHarness _harness;

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

    [Fact]
    public async Task PlaceOrderConsumer_Publishes_OrderPlaced_Event()
    {
        await _harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "John Doe";

        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task Full_Pipeline_Processes_PlaceOrder_To_CustomerNotified()
    {
        await _harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "Jane Smith";

        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }
}