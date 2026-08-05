using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

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
        var msg = $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}";
        await context.Publish(new CustomerNotified(context.Message.OrderId, msg));
    }
}

public class PipelineTests
{
    private readonly IServiceProvider _provider;

    public PipelineTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });
        _provider = services.BuildServiceProvider(true);
    }

    [Fact]
    public async Task PlaceOrder_IsConsumed_And_OrderPlacedPublished()
    {
        var harness = _provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "John Doe";
        await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipeline_Executes_OrderPlacedAndCustomerNotifiedPublished()
    {
        var harness = _provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "Jane Smith";
        await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());
    }
}