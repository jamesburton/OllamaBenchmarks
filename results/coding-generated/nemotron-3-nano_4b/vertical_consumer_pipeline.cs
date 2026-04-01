global using Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts
{
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

    [GlobalSetup]
    public class TestSetup : IAsyncLifetime
    {
        public ValueTask InitializeAsync() =>
            new ValueTask(AddMassTransitTestHarness(cfg => cfg.AddConsumer<PlaceOrderConsumer>()
                .AddConsumer<NotifyCustomerConsumer>()),
                (ctx, cfg) => cfg.ConfigureEndpoints(ctx)));

        public async ValueTask DisposeAsync() => base.DisposeAsync();
    }
}