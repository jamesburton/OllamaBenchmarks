public class OrderConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var msg = context.Message;
        await context.Publish(new OrderProcessed(msg.OrderId));
    }
}

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Processing { get; private set; }
    public Event<OrderPlaced> OrderPlaced { get; private set; }

    public OrderStateMachine()
    {
        Initially(
            When(OrderPlaced)
                .TransitionTo(Processing));
        During(Processing,
            When(OrderShipped)
                .Finalize());
    }
}

services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<OrderConsumer>());

// In test:
var harness = provider.GetRequiredService<ITestHarness>();
await harness.Start();
await harness.Bus.Publish(new OrderPlaced(Guid.NewGuid()));
Assert.True(await harness.Consumed.Any<OrderPlaced>());

Assert.Equal(expected, actual);
Assert.True(condition);
Assert.Throws<Exception>(() => ...);

// Collects ALL failures (don't short-circuit)
Assert.Multiple(
    () => Assert.Equal(1, a),
    () => Assert.Equal(2, b)
);

public class MyFixture : IAsyncLifetime
{
    public ValueTask InitializeAsync() { ... return default; }
    public ValueTask DisposeAsync() { ... return default; }
}

[assembly: AssemblyFixture(typeof(DatabaseFixture))]

public class MyTests(DatabaseFixture db) { }

TestContext.Current.SendDiagnosticMessage("debug info");
CancellationToken ct = TestContext.Current.CancellationToken;
var fixture = TestContext.Current.GetFixture<DatabaseFixture>();

using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Contracts
{
    // Message Records
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    // Consumer Classes
    public class PlaceOrderConsumer : IConsumer<PlaceOrder>
    {
        public async Task Consume(ConsumeContext<PlaceOrder> context)
        {
            // Publish OrderPlaced event
            await context.Publish(new OrderPlaced(context.Message.OrderId, context.Message.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
    {
        public async Task Consume(ConsumeContext<OrderPlaced> context)
        {
            // Publish CustomerNotified event
            await context.Publish(new CustomerNotified(
                context.Message.OrderId, 
                $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"));
        }
    }

    // xUnit Tests
    public class OrderPipelineTests
    {
        [Fact]
        public async Task Should_Publish_OrderPlaced_When_PlaceOrder_Consumed()
        {
            // Arrange
            var harness = new TestHarness(new ServiceCollection().BuildServiceProvider());
            await harness.Start();

            var orderId = Guid.NewGuid();
            var customerName = "Test Customer";

            // Act
            await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

            // Assert
            Assert.True(await harness.Consumed.Any<OrderPlaced>(), 
                "OrderPlaced should be consumed");
        }

        [Fact]
        public async Task Should_Publish_CustomerNotified_When_OrderPlaced_Consumed()
        {
            // Arrange
            var harness = new TestHarness(new ServiceCollection().BuildServiceProvider());
            await harness.Start();

            var orderId = Guid.NewGuid();
            var customerName = "Test Customer";

            // Act
            await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

            // Assert
            Assert.True(await harness.Consumed.Any<CustomerNotified>(), 
                "CustomerNotified should be consumed");
        }

        [Fact]
        public async Task Should_Complete_Full_Pipeline()
        {
            // Arrange
            var harness = new TestHarness(new ServiceCollection().BuildServiceProvider());
            await harness.Start();

            var orderId = Guid.NewGuid();
            var customerName = "Test Customer";

            // Act
            await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

            // Assert
            Assert.True(await harness.Consumed.Any<OrderPlaced>(), 
                "OrderPlaced should be consumed");
            Assert.True(await harness.Consumed.Any<CustomerNotified>(), 
                "CustomerNotified should be consumed");
        }
    }
}

// In Program.cs or Startup.cs
services.AddMassTransitTestHarness(cfg => 
{
    cfg.AddConsumer<PlaceOrderConsumer>();
    cfg.AddConsumer<NotifyCustomerConsumer>();
});

using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Contracts
{
    // Message Records
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    // Consumer Classes
    public class PlaceOrderConsumer : IConsumer<PlaceOrder>
    {
        public async Task Consume(ConsumeContext<PlaceOrder> context)
        {
            // Publish OrderPlaced event
            await context.Publish(new OrderPlaced(context.Message.OrderId, context.Message.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
    {
        public async Task Consume(ConsumeContext<OrderPlaced> context)
        {
            // Publish CustomerNotified event
            await context.Publish(new CustomerNotified(
                context.Message.OrderId, 
                $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"));
        }
    }

    // xUnit Tests
    public class OrderPipelineTests
    {
        [Fact]
        public async Task Should_Publish_OrderPlaced_When_PlaceOrder_Consumed()
        {
            // Arrange
            var harness = new TestHarness(new ServiceCollection().BuildServiceProvider());
            await harness.Start();

            var orderId = Guid.NewGuid();
            var customerName = "Test Customer";

            // Act
            await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

            // Assert
            Assert.True(await harness.Consumed.Any<OrderPlaced>(), 
                "OrderPlaced should be consumed");
        }

        [Fact]
        public async Task Should_Publish_CustomerNotified_When_OrderPlaced_Consumed()
        {
            // Arrange
            var harness = new TestHarness(new ServiceCollection().BuildServiceProvider());
            await harness.Start();

            var orderId = Guid.NewGuid();
            var customerName = "Test Customer";

            // Act
            await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

            // Assert
            Assert.True(await harness.Consumed.Any<CustomerNotified>(), 
                "CustomerNotified should be consumed");
        }

        [Fact]
        public async Task Should_Complete_Full_Pipeline()
        {
            // Arrange
            var harness = new TestHarness(new ServiceCollection().BuildServiceProvider());
            await harness.Start();

            var orderId = Guid.NewGuid();
            var customerName = "Test Customer";

            // Act
            await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

            // Assert
            Assert.True(await harness.Consumed.Any<OrderPlaced>(), 
                "OrderPlaced should be consumed");
            Assert.True(await harness.Consumed.Any<CustomerNotified>(), 
                "CustomerNotified should be consumed");
        }
    }
}

// In Program.cs or Startup.cs
services.AddMassTransitTestHarness(cfg => 
{
    cfg.AddConsumer<PlaceOrderConsumer>();
    cfg.AddConsumer<NotifyCustomerConsumer>();
});

using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Contracts
{
    // Message Records
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    // Consumer Classes
    public class PlaceOrderConsumer : IConsumer<PlaceOrder>
    {
        public async Task Consume(ConsumeContext<PlaceOrder> context)
        {
            // Publish OrderPlaced event
            await context.Publish(new OrderPlaced(context.Message.OrderId, context.Message.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
    {
        public async Task Consume(ConsumeContext<OrderPlaced> context)
        {
            // Publish CustomerNotified event
            await context.Publish(new CustomerNotified(
                context.Message.OrderId, 
                $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"));
        }
    }

    // xUnit Tests
    public class OrderPipelineTests
    {
        [Fact]
        public async Task Should_Publish_OrderPlaced_When_PlaceOrder_Consumed()
        {
            // Arrange
            var harness = new TestHarness(new ServiceCollection().BuildServiceProvider());
            await harness.Start();

            var orderId = Guid.NewGuid();
            var customerName = "Test Customer";

            // Act
            await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

            // Assert
            Assert.True(await harness.Consumed.Any<OrderPlaced>(), 
                "OrderPlaced should be consumed");
        }

        [Fact]
        public async Task Should_Publish_CustomerNotified_When_OrderPlaced_Consumed()
        {
            // Arrange
            var harness = new TestHarness(new ServiceCollection().BuildServiceProvider());
            await harness.Start();

            var orderId = Guid.NewGuid();
            var customerName = "Test Customer";

            // Act
            await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

            // Assert
            Assert.True(await harness.Consumed.Any<CustomerNotified>(), 
                "CustomerNotified should be consumed");
        }

        [Fact]
        public async Task Should_Complete_Full_Pipeline()
        {
            // Arrange
            var harness = new TestHarness(new ServiceCollection().BuildServiceProvider());
            await harness.Start();

            var orderId = Guid.NewGuid();
            var customerName = "Test Customer";

            // Act
            await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

            // Assert
            Assert.True(await harness.Consumed.Any<OrderPlaced>(), 
                "OrderPlaced should be consumed");
            Assert.True(await harness.Consumed.Any<CustomerNotified>(), 
                "CustomerNotified should be consumed");
        }
    }
}

// In Program.cs or Startup.cs
services.AddMassTransitTestHarness(cfg => 
{
    cfg.AddConsumer<PlaceOrderConsumer>();
    cfg.AddConsumer<NotifyCustomerConsumer>();
});

using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Contracts
{
    // Message Records
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    // Consumer Classes
    public class PlaceOrderConsumer : IConsumer<PlaceOrder>
    {
        public async Task Consume(ConsumeContext<PlaceOrder> context)
        {
            // Publish OrderPlaced event
            await context.Publish(new OrderPlaced(context.Message.OrderId, context.Message.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
    {
        public async Task Consume(ConsumeContext<OrderPlaced> context)
        {
            // Publish CustomerNotified event
            await context.Publish(new CustomerNotified(
                context.Message.OrderId, 
                $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"));
        }
    }

    // xUnit Tests
    public class OrderPipelineTests
    {
        [Fact]
        public async Task Should_Publish_OrderPlaced_When_PlaceOrder_Consumed()
        {
            // Arrange
            var harness = new TestHarness(new ServiceCollection().BuildServiceProvider());
            await harness.Start();

            var orderId = Guid.NewGuid();
            var customerName = "Test Customer";

            // Act
            await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

            // Assert
            Assert.True(await harness.Consumed.Any<OrderPlaced>(), 
                "OrderPlaced should be consumed");
        }

        [Fact]
        public async Task Should_Publish_CustomerNotified_When_OrderPlaced_Consumed()
        {
            // Arrange
            var harness = new TestHarness(new ServiceCollection().BuildServiceProvider());
            await harness.Start();

            var orderId = Guid.NewGuid();
            var customerName = "Test Customer";

            // Act
            await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

            // Assert
            Assert.True(await harness.Consumed.Any<CustomerNotified>(), 
                "CustomerNotified should be consumed");
        }

        [Fact]
        public async Task Should_Complete_Full_Pipeline()
        {
            // Arrange
            var harness = new TestHarness(new ServiceCollection().BuildServiceProvider());
            await harness.Start();

            var orderId = Guid.NewGuid();
            var customerName = "Test Customer";

            // Act
            await harness.Bus.Publish(new PlaceOrder(orderId, customerName));

            // Assert
            Assert.True(await harness.Consumed.Any<OrderPlaced>(), 
                "OrderPlaced should be consumed");
            Assert.True(await harness.Consumed.Any<CustomerNotified>(), 
                "CustomerNotified should be consumed");
        }
    }
}

// In Program.cs or Startup.cs
services.AddMassTransitTestHarness(cfg => 
{
    cfg.AddConsumer<PlaceOrderConsumer>();
    cfg.AddConsumer<NotifyCustomerConsumer>();
});