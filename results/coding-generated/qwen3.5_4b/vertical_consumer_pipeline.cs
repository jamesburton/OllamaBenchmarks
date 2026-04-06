public record PlaceOrder(Guid OrderId, string CustomerName);
        public record OrderPlaced(Guid OrderId, string CustomerName);
        public record CustomerNotified(Guid OrderId, string NotificationMessage);

public class PlaceOrderConsumer : IConsumer<PlaceOrder>
        {
            public async Task Consume(ConsumeContext<PlaceOrder> context)
            {
                var msg = context.Message;
                await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
            }
        }
        public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
        {
            public async Task Consume(ConsumeContext<OrderPlaced> context)
            {
                var msg = context.Message;
                await context.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
            }
        }

services.AddMassTransit(x =>
        {
            x.AddConsumer<PlaceOrderConsumer>();
            x.AddConsumer<NotifyCustomerConsumer>();
            x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
        });

services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<PlaceOrderConsumer>()
            .AddConsumer<NotifyCustomerConsumer>());