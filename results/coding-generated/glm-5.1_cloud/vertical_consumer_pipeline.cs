global using Contracts;

using MassTransit;

namespace Contracts;

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
        await context.Publish(new CustomerNotified(
            msg.OrderId, 
            $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
    }
}

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class OrderPipelineTests
{
    [Fact]
    public async Task PlaceOrderConsumer_ShouldConsumeAndPublishOrderPlaced()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
        });
        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new PlaceOrder(orderId, "Test Customer"));

        // Assert
        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());

        await harness.Stop();
    }

    [Fact]
    public async Task FullPipeline_ShouldConsumePlaceOrderAndPublishOrderPlacedAndCustomerNotified()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<PlaceOrderConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
        });
        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new PlaceOrder(orderId, "Test Customer"));

        // Assert
        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Consumed.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());

        await harness.Stop();
    }
}

Assert.True(await harness.Consumed.Any<OrderPlaced>());

global using Contracts;

using MassTransit;

namespace Contracts;

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
        await context.Publish(new CustomerNotified(
            msg.OrderId, 
            $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
    }
}

using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class OrderPipelineTests
{
    [Fact]
    public async Task PlaceOrderConsumer_ShouldConsumeAndPublishOrderPlaced()
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
        await harness.Bus.Publish(new PlaceOrder(orderId, "Test Customer"));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());

        await harness.Stop();
    }

    [Fact]
    public async Task FullPipeline_ShouldConsumePlaceOrderAndPublishOrderPlacedAndCustomerNotified()
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
        await harness.Bus.Publish(new PlaceOrder(orderId, "Test Customer"));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Consumed.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());

        await harness.Stop();
    }
}

global using Contracts;

using MassTransit;

namespace Contracts
{
    public record PlaceOrder(Guid OrderId, string CustomerName);
    // ...
}

using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class OrderPipelineTests
{
    // ...
}

global using Contracts;

using MassTransit;

namespace Contracts;

public record PlaceOrder(Guid OrderId, string CustomerName);
// ... all Contracts stuff

namespace // This would be the global namespace
{
    // But this doesn't work for top-level classes
}

global using Contracts;

using MassTransit;

namespace Contracts
{
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
            await context.Publish(new CustomerNotified(
                msg.OrderId, 
                $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }
}

using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class OrderPipelineTests
{
    // tests
}

global using Contracts;

using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class OrderPipelineTests
{
    // tests
}

using MassTransit;

namespace Contracts;

public record PlaceOrder(Guid OrderId, string CustomerName);
// ...

global using Contracts;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class OrderPipelineTests
{
    [Fact]
    public async Task PlaceOrderConsumer_ShouldConsumeAndPublishOrderPlaced()
    {
        // ...
    }

    [Fact]
    public async Task FullPipeline_ShouldConsumePlaceOrderAndPublishOrderPlacedAndCustomerNotified()
    {
        // ...
    }
}

namespace Contracts;

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
        await context.Publish(new CustomerNotified(
            msg.OrderId, 
            $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
    }
}

global using Contracts;

using MassTransit;

namespace Contracts
{
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
            await context.Publish(new CustomerNotified(
                msg.OrderId, 
                $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }
}

using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class OrderPipelineTests
{
    [Fact]
    public async Task PlaceOrderConsumer_ShouldConsumeAndPublishOrderPlaced()
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
        await harness.Bus.Publish(new PlaceOrder(orderId, "Test Customer"));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());

        await harness.Stop();
    }

    [Fact]
    public async Task FullPipeline_ShouldConsumePlaceOrderAndPublishOrderPlacedAndCustomerNotified()
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
        await harness.Bus.Publish(new PlaceOrder(orderId, "Test Customer"));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Consumed.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());

        await harness.Stop();
    }
}

namespace Contracts;
// stuff in Contracts

namespace; // This is invalid syntax

namespace Contracts
{
    // Contracts stuff
}

// This is in the global namespace
public class TestClass { }

global using Contracts;

using MassTransit;

namespace Contracts
{
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
            await context.Publish(new CustomerNotified(
                msg.OrderId, 
                $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }
}

using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class OrderPipelineTests
{
    [Fact]
    public async Task PlaceOrderConsumer_ShouldConsumeAndPublishOrderPlaced()
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
        await harness.Bus.Publish(new PlaceOrder(orderId, "Test Customer"));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());

        await harness.Stop();
    }

    [Fact]
    public async Task FullPipeline_ShouldConsumePlaceOrderAndPublishOrderPlacedAndCustomerNotified()
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
        await harness.Bus.Publish(new PlaceOrder(orderId, "Test Customer"));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Consumed.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());

        await harness.Stop();
    }
}

global using Contracts;

using MassTransit;

namespace Contracts
{
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
            await context.Publish(new CustomerNotified(
                msg.OrderId, 
                $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }
}

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class OrderPipelineTests
{
    [Fact]
    public async Task PlaceOrderConsumer_ShouldConsumePlaceOrderAndPublishOrderPlaced()
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
        await harness.Bus.Publish(new PlaceOrder(orderId, "Test Customer"));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());

        await harness.Stop();
    }

    [Fact]
    public async Task FullPipeline_ShouldPublishOrderPlacedAndCustomerNotified()
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
        await harness.Bus.Publish(new PlaceOrder(orderId, "Test Customer"));

        Assert.True(await harness.Consumed.Any<PlaceOrder>());
        Assert.True(await harness.Published.Any<OrderPlaced>());
        Assert.True(await harness.Consumed.Any<OrderPlaced>());
        Assert.True(await harness.Published.Any<CustomerNotified>());

        await harness.Stop();
    }
}

global using Contracts;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class OrderPipelineTests
{
    // tests
}

using MassTransit;

namespace Contracts;

public record PlaceOrder(Guid OrderId, string CustomerName);
// ...

public class TestClass { }

using MassTransit; // This is invalid - using must come before any type declarations

namespace Contracts;