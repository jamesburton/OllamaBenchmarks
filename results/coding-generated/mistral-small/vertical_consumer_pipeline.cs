global using Contracts;

using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Threading.Tasks;
using MassTransit.Testing;

namespace Contracts
{
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    public class PlaceOrderConsumer : IConsumer<PlaceOrder>
    {
        private readonly IBus _bus;

        public PlaceOrderConsumer(IBus bus)
        {
            _bus = bus;
        }

        public async Task Consume(ConsumeContext<PlaceOrder> context)
        {
            var msg = context.Message;
            await _bus.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
    {
        private readonly IBus _bus;

        public NotifyCustomerConsumer(IBus bus)
        {
            _bus = bus;
        }

        public async Task Consume(ConsumeContext<OrderPlaced> context)
        {
            var msg = context.Message;
            await _bus.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }
}

public class PlaceOrderConsumerTests
{
    private readonly IServiceProvider _provider;

    public PlaceOrderConsumerTests()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });
        _provider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task PlaceOrderConsumer_ShouldPublishOrderPlaced()
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
    public async Task FullPipeline_ShouldPublishCustomerNotified()
    {
        var harness = _provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "John Doe";

        await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Consumed.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());
    }
}