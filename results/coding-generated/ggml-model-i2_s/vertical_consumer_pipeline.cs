using Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts
{
    public class PlaceOrder
    {
        public Guid OrderId { get; set; }
        public string CustomerName { get; set; }
    }

    public class OrderPlaced
    {
        public Guid OrderId { get; set; }
        public string CustomerName { get; set; }
    }

    public class CustomerNotified
    {
        public Guid OrderId { get; set; }
        public string NotificationMessage { get; set; }
    }

    public class PlaceOrderConsumer : IConsumer<PlaceOrder>
    {
        public async Task Consume(PlaceOrderContext<OrderPlaced> context)
        {
            var msg = context.Message;
            await context.Publish(new OrderPlaced(new PlaceOrder(msg.OrderId, msg.CustomerName)));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
    {
        public async Task Consume(OrderPlacedContext<OrderPlaced> context)
        {
            var msg = context.Message;
            await context.Publish(new CustomerNotified(new PlaceOrder(msg.OrderId, msg.CustomerName), $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"));
        }
    }

    public class PlaceOrderConsumerDefinition : ConsumerDefinition<PlaceOrderConsumer>
    {
        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator ep,
            IConsumerConfigurator<PlaceOrderConsumer> consumer,
            IRegistrationContext context)
        {
            ep.UseMessageRetry(r => r.Intervals(500, 1000, 2000));
            ep.UseInMemoryOutbox(context);
        }
    }

    public class NotifyCustomerConsumerDefinition : ConsumerDefinition<OrderPlaced>
    {
        protected override void ConfigureConsumer(
            IConsumerConfigurator<OrderPlaced> consumer,
            IRegistrationContext context)
        {
            ep.UseMessageRetry(r => r.Intervals(500, 1000, 2000));
            ep.UseInMemoryOutbox(context);
        }
    }

    public class OrderPlacedConsumer : IConsumer<OrderPlaced>
    {
        public async Task Consume(PlaceOrderContext<OrderPlaced> context)
        {
            var msg = context.Message;
            await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
        }
    }

    public class OrderPlacedConsumerDefinition : ConsumerDefinition<OrderPlacedConsumer>
    {
        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator ep,
            IConsumerConfigurator<OrderPlacedConsumer> consumer,
            IRegistrationContext context)
        {
            ep.UseMessageRetry(r => r.Intervals(500, 1000, 2000));
            ep.UseInMemoryOutbox(context);
        }
    }

    public class CustomerNotifiedConsumer : IConsumer<OrderPlaced>
    {
        public async Task Consume(OrderPlacedContext<OrderPlaced> context)
        {
            var msg = context.Message;
            await context.Publish(new CustomerNotified(new PlaceOrder(msg.OrderId, msg.CustomerName), $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"));
        }
    }

    public class CustomerNotifiedConsumerDefinition : ConsumerDefinition<OrderPlaced>
    {
        protected override void ConfigureConsumer(
            IConsumerConfigurator<OrderPlaced> consumer,
            IRegistrationContext context)
        {
            ep.UseMessageRetry(r => r.Intervals(500, 1000, 2000));
            ep.UseInMemoryOutbox(context);
        }
    }

    public class PlaceOrderConsumerTestHarness : ITestHarness<PlaceOrderConsumer>
    {
        public async Task RunAsync()
        {
            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();
            await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "John Doe"));
            await harness.Consumed.Any<OrderPlaced>();
        }
    }

    public class NotifyCustomerConsumerTestHarness : ITestHarness<OrderPlaced>
    {
        public async Task RunAsync()
        {
            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();
            await harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "John Doe"));
            await harness.Published.Any<OrderPlaced>();
        }
    }
}