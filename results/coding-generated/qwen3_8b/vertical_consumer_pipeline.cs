global using Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Contracts
{
    // Positional records for messages
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);
}

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

public class ConsumerTests : IAsyncLifetime
{
    private readonly ITestHarness _harness;

    public ConsumerTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        _harness = services.BuildServiceProvider().GetRequiredService<ITestHarness>();
    }

    public async Task InitializeAsync()
    {
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
    }

    [Fact]
    public async Task Test_PlaceOrder_Consumed_And_OrderPlaced_Published()
    {
        var orderId = Guid.NewGuid();
        var customerName = "Alice";

        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        // Verify PlaceOrder was consumed
        Assert.True(await _harness.Consumed.Any<PlaceOrder>());

        // Verify OrderPlaced was published
        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task Test_Full_Pipeline_PlaceOrder_To_CustomerNotified()
    {
        var orderId = Guid.NewGuid();
        var customerName = "Bob";

        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        // Verify PlaceOrder was consumed
        Assert.True(await _harness.Consumed.Any<PlaceOrder>());

        // Verify OrderPlaced was published
        Assert.True(await _harness.Published.Any<OrderPlaced>());

        // Verify CustomerNotified was published
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }
}