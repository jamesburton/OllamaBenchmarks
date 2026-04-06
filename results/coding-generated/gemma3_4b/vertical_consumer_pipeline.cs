using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Contracts;

global using Contracts;

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
        await context.Publish(new CustomerNotified(context.Message.OrderId, $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"));
    }
}

public class OrderPipelineTests
{
    [Fact]
    public async Task TestPlaceOrderAndNotifyCustomer()
    {
        var harness = TestHelpers.CreateHarness();
        await harness.Start();

        // Publish PlaceOrder
        await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "John Doe"));

        // Assert OrderPlaced consumed and published
        Assert.True(await harness.Consumed.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<OrderPlaced>());

        // Assert CustomerNotified consumed and published
        Assert.True(await harness.Consumed.Any<CustomerNotified>());
        Assert.True(await harness.Published.Any<CustomerNotified>());

        await harness.Stop();
    }
}