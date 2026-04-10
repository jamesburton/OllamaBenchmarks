global using Contracts;

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
        var message = context.Message;
        await context.Publish(new OrderPlaced(message.OrderId, message.CustomerName));
    }
}

public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var message = context.Message;
        await context.Publish(new CustomerNotified(
            message.OrderId,
            $"Order {message.OrderId} confirmed for {message.CustomerName}"
        ));
    }
}

public class ConsumerPipelineTests
{
    [Fact]
    public async Task PlaceOrderConsumer_PublishesOrderPlaced()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
        });

        var harness = services.BuildServiceProvider().GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "John Doe";
        await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await harness.Consumed.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<OrderPlaced>());

        await harness.Stop();
    }

    [Fact]
    public async Task FullPipeline_PublishesOrderPlacedAndCustomerNotified()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        var harness = services.BuildServiceProvider().GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "Jane Smith";
        await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await harness.Consumed.Any<OrderPlaced>());
        Assert.True(await harness.Consumed.Any<CustomerNotified>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());

        await harness.Stop();
    }
}