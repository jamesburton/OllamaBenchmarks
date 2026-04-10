global using Contracts;
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
                await context.Publish(new CustomerNotified(
                    context.Message.OrderId,
                    $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"));
            }
        }

        public class PipelineTests
        {
            [Fact]
            public async Task PlaceOrder_Should_Publish_OrderPlaced()
            {
                var services = new ServiceCollection();
                services.AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<PlaceOrderConsumer>();
                });

                var provider = services.BuildServiceProvider();
                var harness = provider.GetRequiredService<ITestHarness>();
                await harness.Start();

                var orderId = Guid.NewGuid();
                var customerName = "John Doe";
                await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

                Assert.True(await harness.Consumed.Any<PlaceOrder>());
                Assert.True(await harness.Published.Any<OrderPlaced>());
            }

            [Fact]
            public async Task FullPipeline_Should_Complete_All_Stages()
            {
                var services = new ServiceCollection();
                services.AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<PlaceOrderConsumer>();
                    cfg.AddConsumer<NotifyCustomerConsumer>();
                });

                var provider = services.BuildServiceProvider();
                var harness = provider.GetRequiredService<ITestHarness>();
                await harness.Start();

                var orderId = Guid.NewGuid();
                var customerName = "Jane Doe";
                await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

                // Verify Stage 1
                Assert.True(await harness.Consumed.Any<PlaceOrder>());
                Assert.True(await harness.Published.Any<OrderPlaced>());

                // Verify Stage 2
                Assert.True(await harness.Consumed.Any<OrderPlaced>());
                Assert.True(await harness.Published.Any<CustomerNotified>());
            }
        }