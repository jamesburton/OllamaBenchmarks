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
        var message = new CustomerNotified(
            context.Message.OrderId,
            $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"
        );
        await context.Publish(message);
    }
}

public class OrderPlacedTests
{
    private readonly ITestHarness _harness;

    public OrderPlacedTests(ITestHarness harness)
    {
        _harness = harness;
    }

    [Fact]
    public async Task PublishPlaceOrder_ConsumedByPlaceOrderConsumer_PublishesOrderPlaced()
    {
        var harness = await _harness.Start();
        try
        {
            var orderId = Guid.NewGuid();
            var customerName = "John Doe";
            var message = new PlaceOrder(orderId, customerName);

            await harness.Bus.Publish(message);

            Assert.True(await harness.Consumed.Any<PlaceOrder>());
            Assert.True(await harness.Published.Any<OrderPlaced>());
        }
        finally
        {
            await _harness.Stop();
        }
    }

    [Fact]
    public async Task PublishPlaceOrder_FullPipeline_ConsumesOrderPlacedAndPublishesCustomerNotified()
    {
        var harness = await _harness.Start();
        try
        {
            var orderId = Guid.NewGuid();
            var customerName = "Jane Smith";
            var message = new PlaceOrder(orderId, customerName);

            await harness.Bus.Publish(message);

            Assert.True(await harness.Consumed.Any<PlaceOrder>());
            Assert.True(await harness.Consumed.Any<OrderPlaced>());
            Assert.True(await harness.Published.Any<CustomerNotified>());
        }
        finally
        {
            await _harness.Stop();
        }
    }
}