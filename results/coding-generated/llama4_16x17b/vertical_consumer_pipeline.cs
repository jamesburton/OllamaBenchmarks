using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

global using Contracts;

namespace Contracts
{
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);
}

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

public class Tests
{
    [Fact]
    public async Task PlaceOrder_Publishes_OrderPlaced()
    {
        // Arrange
        var harness = new TestHarness<PlaceOrderConsumer>(new TestHarnessOptions { });
        await harness.Start();

        // Act
        await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "John Doe"));

        // Assert
        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task Full_Pipeline()
    {
        // Arrange
        var harness = new TestHarness<PlaceOrderConsumer>(new TestHarnessOptions { });
        await harness.Start();

        // Act
        await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "John Doe"));

        // Assert
        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());
    }
}

public class TestHarness<T> where T : class, IConsumer
{
    public async Task Start()
    {
        // Implementation
    }

    public async Task Stop()
    {
        // Implementation
    }
}

public class TestHarnessOptions
{
    // Options
}

public static class TestHarnessExtensions
{
    public static async Task<ITestHarness> StartTestHarness(this IServiceCollection services)
    {
        // Implementation
    }
}