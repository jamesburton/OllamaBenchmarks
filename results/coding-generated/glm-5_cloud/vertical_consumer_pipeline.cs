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
        await context.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
    }
}

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class OrderPipelineTests
{
    [Fact]
    public async Task PlaceOrder_ShouldConsumeAndPublishOrderPlaced()
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

        try
        {
            // Act
            var orderId = Guid.NewGuid();
            await harness.Bus.Publish(new PlaceOrder(orderId, "John Doe"));

            // Assert
            Assert.True(await harness.Consumed.Any<PlaceOrder>());
            Assert.True(await harness.Published.Any<OrderPlaced>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task PlaceOrder_ShouldCompleteFullPipeline()
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

        try
        {
            // Act
            var orderId = Guid.NewGuid();
            await harness.Bus.Publish(new PlaceOrder(orderId, "John Doe"));

            // Assert
            Assert.True(await harness.Consumed.Any<PlaceOrder>());
            Assert.True(await harness.Published.Any<OrderPlaced>());
            Assert.True(await harness.Consumed.Any<OrderPlaced>());
            Assert.True(await harness.Published.Any<CustomerNotified>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}

global using Contracts;
using MassTransit;

namespace Contracts;

// messages and consumers

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

// test class

global using Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts;

// messages and consumers

// test class (outside namespace)

global using Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts
{
    // messages and consumers
}

// test class (outside namespace)

global using Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts
{
    // messages and consumers
}

public class OrderPipelineTests
{
    // tests
}

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
}

public class OrderPipelineTests
{
    // tests
}

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
}

public class OrderPipelineTests
{
    [Fact]
    public async Task PlaceOrder_ShouldConsumeAndPublishOrderPlaced()
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

        try
        {
            var orderId = Guid.NewGuid();
            await harness.Bus.Publish(new PlaceOrder(orderId, "John Doe"));

            Assert.True(await harness.Consumed.Any<PlaceOrder>());
            Assert.True(await harness.Published.Any<OrderPlaced>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task PlaceOrder_ShouldCompleteFullPipeline()
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

        try
        {
            var orderId = Guid.NewGuid();
            await harness.Bus.Publish(new PlaceOrder(orderId, "John Doe"));

            Assert.True(await harness.Consumed.Any<PlaceOrder>());
            Assert.True(await harness.Published.Any<OrderPlaced>());
            Assert.True(await harness.Consumed.Any<OrderPlaced>());
            Assert.True(await harness.Published.Any<CustomerNotified>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}

[Fact]
public async Task PlaceOrder_ShouldConsumeAndPublishOrderPlaced()
{
    // ... setup ...

    await harness.Bus.Publish(new PlaceOrder(orderId, "John Doe"));

    Assert.True(await harness.Consumed.Any<PlaceOrder>());
    Assert.True(await harness.Published.Any<OrderPlaced>());
}

[Fact]
public async Task PlaceOrder_ShouldCompleteFullPipeline()
{
    // ... setup ...

    await harness.Bus.Publish(new PlaceOrder(orderId, "John Doe"));

    // OrderPlaced should be consumed by NotifyCustomerConsumer
    Assert.True(await harness.Consumed.Any<OrderPlaced>());
    // CustomerNotified should be published
    Assert.True(await harness.Published.Any<CustomerNotified>());
}

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

public class OrderPipelineTests
{
    // tests
}

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
}

public class OrderPipelineTests
{
    [Fact]
    public async Task PlaceOrder_ShouldConsumeAndPublishOrderPlaced()
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

        try
        {
            var orderId = Guid.NewGuid();
            await harness.Bus.Publish(new PlaceOrder(orderId, "John Doe"));

            Assert.True(await harness.Consumed.Any<PlaceOrder>());
            Assert.True(await harness.Published.Any<OrderPlaced>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task PlaceOrder_ShouldCompleteFullPipeline()
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

        try
        {
            var orderId = Guid.NewGuid();
            await harness.Bus.Publish(new PlaceOrder(orderId, "John Doe"));

            Assert.True(await harness.Consumed.Any<OrderPlaced>());
            Assert.True(await harness.Published.Any<CustomerNotified>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}

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
}

public class OrderPipelineTests
{
    [Fact]
    public async Task PlaceOrder_ShouldConsumeAndPublishOrderPlaced()
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

        try
        {
            var orderId = Guid.NewGuid();
            await harness.Bus.Publish(new PlaceOrder(orderId, "John Doe"));

            Assert.True(await harness.Consumed.Any<PlaceOrder>());
            Assert.True(await harness.Published.Any<OrderPlaced>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task PlaceOrder_ShouldCompleteFullPipeline()
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

        try
        {
            var orderId = Guid.NewGuid();
            await harness.Bus.Publish(new PlaceOrder(orderId, "John Doe"));

            Assert.True(await harness.Consumed.Any<OrderPlaced>());
            Assert.True(await harness.Published.Any<CustomerNotified>());
        }
        finally
        {
            await harness.Stop();
        }
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
}

public class OrderPipelineTests
{
    [Fact]
    public async Task PlaceOrder_ShouldConsumeAndPublishOrderPlaced()
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

        try
        {
            var orderId = Guid.NewGuid();
            await harness.Bus.Publish(new PlaceOrder(orderId, "John Doe"));

            Assert.True(await harness.Consumed.Any<PlaceOrder>());
            Assert.True(await harness.Published.Any<OrderPlaced>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task PlaceOrder_ShouldCompleteFullPipeline()
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

        try
        {
            var orderId = Guid.NewGuid();
            await harness.Bus.Publish(new PlaceOrder(orderId, "John Doe"));

            Assert.True(await harness.Consumed.Any<OrderPlaced>());
            Assert.True(await harness.Published.Any<CustomerNotified>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}

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

public class OrderPipelineTests
{
    [Fact]
    public async Task PlaceOrder_ShouldConsumeAndPublishOrderPlaced()
    {
        // ...
    }

    [Fact]
    public async Task PlaceOrder_ShouldCompleteFullPipeline()
    {
        // ...
    }
}