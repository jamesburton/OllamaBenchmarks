global using Contracts;

using MassTransit;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using MassTransit.Testing;

public record PlaceOrder(Guid OrderId, string CustomerName);
public record OrderPlaced(Guid OrderId, string CustomerName);
public record CustomerNotified(Guid OrderId, string NotificationMessage);

namespace Contracts
{
    public class PlaceOrderConsumer : IConsumer<PlaceOrder>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public PlaceOrderConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<PlaceOrder> context)
        {
            var placeOrder = context.Message;
            await _publishEndpoint.Publish(new OrderPlaced(placeOrder.OrderId, placeOrder.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public NotifyCustomerConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OrderPlaced> context)
        {
            var orderPlaced = context.Message;
            await _publishEndpoint.Publish(new CustomerNotified(orderPlaced.OrderId, $"Order {orderPlaced.OrderId} confirmed for {orderPlaced.CustomerName}"));
        }
    }
}

public class ConsumerPipelineTests
{
    [Fact]
    public async Task PlaceOrderConsumer_ShouldPublish_OrderPlaced()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        using var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "John Doe";

        await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>(x => x.OrderId == orderId && x.CustomerName == customerName));
    }

    [Fact]
    public async Task FullPipeline_ShouldPublish_OrderPlaced_And_CustomerNotified()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });

        using var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "Jane Smith";

        await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>(x => x.OrderId == orderId && x.CustomerName == customerName));
        Assert.True(await harness.Published.Any<CustomerNotified>(x => x.OrderId == orderId && x.NotificationMessage.Contains(customerName)));
    }
}