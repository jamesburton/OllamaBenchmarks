global using Contracts;
using System;
using System.Threading.Tasks;
using Xunit;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Contracts;          // ensure the namespace is open for the types below

namespace Contracts
{
    // -----------------------------------------------------------------
    //   Message types (positional record syntax)
    // -----------------------------------------------------------------
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record OrderShipped(Guid OrderId, string TrackingNumber);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);
    public record OrderProcessed(Guid OrderId);

    // -----------------------------------------------------------------
    //   Consumers
    // -----------------------------------------------------------------
    public class PlaceOrderConsumer : IConsumer<PlaceOrder>
    {
        public async Task Consume(ConsumeContext<PlaceOrder> context)
        {
            // Publish the OrderPlaced event
            context.Publish(new OrderPlaced(context.Message.OrderId, context.Message.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
    {
        public async Task Consume(ConsumeContext<OrderPlaced> context)
        {
            // Publish the CustomerNotified event
            context.Publish(new CustomerNotified(context.Message.OrderId,
                $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"));
        }
    }

    // -----------------------------------------------------------------
    //   Test harness registration (IServiceCollection)
    // -----------------------------------------------------------------
    static class TestProgram
    {
        public static void ConfigureServices(IServiceCollection cs)
        {
            cs.AddMassTransit(c => c.AddConsumer<PlaceOrderConsumer>());
            cs.AddMassTransit(c => c.AddConsumer<NotifyCustomerConsumer>());
            cs.AddMassTransitTestHarness(ctx => ctx.AddConsumer<PlaceOrderConsumer>());
            cs.AddMassTransitTestHarness(ctx => ctx.AddConsumer<NotifyCustomerConsumer>());
            cs.AddMassTransitTestHarness(ctx => 
                cfg => cfg.ConfigureEndpoints(ctx));
        }
    }

    // -----------------------------------------------------------------
    //   XUnit test collection
    // -----------------------------------------------------------------
    [CollectionDefinition("MassTransitPipelines")]
    public class MassTransitPipelineCollection { }

    [Collection("MassTransitPipelines")]
    public class PipeOrderTests
    {
        [Fact]
        public async Task Publish_Order_Placed_Event_Is_Consumed()
        {
            // Build the service provider with the consumer pipeline
            var provider = TestProgram.ConfigureServices(new ServiceCollection());

            // Get the ITTestHarness service from the provider
            var harness = provider.GetRequiredService<ITestHarness>();

            // Start the in‑memory bus
            await harness.Start();

            // Publish a PlaceOrder message
            var orderId = Guid.NewGuid();
            await harness.Bus.Publish(new PlaceOrder(orderId, "John Doe"));

            // Assert: the PlaceOrder message has been consumed
            bool orderConsumed = await harness.Consumed.AnyAsync(t => t is PlaceOrder);
            // Assert: an OrderPlaced event has been published and consumed by NotifyCustomerConsumer
            bool orderPlacedConsumed = await harness.Consumed.AnyAsync(t => t is OrderPlaced);
            bool orderPlacedPublished = await harness.Published.AnyAsync(t => t is OrderPlaced);

            Assert.True(orderConsumed);
            Assert.True(orderPlacedConsumed);
            Assert.True(orderPlacedPublished);
        }

        [Fact]
        public async Task Full_Pipeline_Afters_Publish_PlaceOrder()
        {
            var provider = TestProgram.ConfigureServices(new ServiceCollection());
            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var orderId = Guid.NewGuid();
            await harness.Bus.Publish(new PlaceOrder(orderId, "Jane Smith"));

            // The pipeline: PlaceOrder → OrderPlaced event → CustomerNotified event
            Assert.True(harness.Consumed.Any(t => t is OrderPlaced));
            Assert.True(harness.Published.Any(t => t is CustomerNotified));
        }
    }
}