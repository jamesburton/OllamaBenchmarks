global using Contracts;

namespace Contracts {
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    public class PlaceOrderConsumer : IConsumer<PlaceOrder> {
        public async Task Consume(ConsumeContext<PlaceOrder> context) {
            var msg = context.Message;
            await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced> {
        public async Task Consume(ConsumeContext<OrderPlaced> context) {
            var msg = context.Message;
            await context.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }

    // xUnit v3 tests
}

global using MassTransit.Testing;
global using Microsoft.Extensions.DependencyInjection;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ConsumerTests {
    [Fact]
    public void PlaceOrderConsumerPublishesOrderPlaced() {
        var harness = new TestHarness();
        harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Alice"));
        Assert.True(harness.Consumed.Any<OrderPlaced>());
    }

    [Theory, MemberData(nameof(Data))]
    public void FullPipeline(OrderPlaced order) {
        var harness = new TestHarness();
        harness.Bus.Publish(order);
        Assert.True(harness.Published.Any<CustomerNotified>());
    }

    public static MatrixTheoryData<OrderPlaced> Data() => 
        new(new[] { new OrderPlaced(Guid.NewGuid(), "Alice") });
}


<|end|>
global using Contracts;

namespace Contracts {
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    public class PlaceOrderConsumer : IConsumer<PlaceOrder> {
        public async Task Consume(ConsumeContext<PlaceOrder> context) {
            var msg = context.Message;
            await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced> {
        public async Task Consume(ConsumeContext<OrderPlaced> context) {
            var msg = context.Message;
            await context.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }

    // xUnit v3 tests
}

global using MassTransit.Testing;
global using Microsoft.Extensions.DependencyInjection;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ConsumerTests {
    [Fact]
    public void PlaceOrderConsumerPublishesOrderPlaced() {
        var harness = new TestHarness();
        harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Alice"));
        Assert.True(harness.Consumed.Any<OrderPlaced>());
    }

    [Theory, MemberData(nameof(Data))]
    public void FullPipeline(OrderPlaced order) {
        var harness = new TestHarness();
        harness.Bus.Publish(order);
        Assert.True(harness.Published.Any<CustomerNotified>());
    }

    public static MatrixTheoryData<OrderPlaced> Data() => 
        new(new[] { new OrderPlaced(Guid.NewGuid(), "Alice") });
}


<|end|>
global using Contracts;

namespace Contracts {
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    public class PlaceOrderConsumer : IConsumer<PlaceOrder> {
        public async Task Consume(ConsumeContext<PlaceOrder> context) {
            var msg = context.Message;
            await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced> {
        public async Task Consume(ConsumeContext<OrderPlaced> context) {
            var msg = context.Message;
            await context.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }

    // xUnit v3 tests
}

global using MassTransit.Testing;
global using Microsoft.Extensions.DependencyInjection;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ConsumerTests {
    [Fact]
    public void PlaceOrderConsumerPublishesOrderPlaced() {
        var harness = new TestHarness();
        harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Alice"));
        Assert.True(harness.Consumed.Any<OrderPlaced>());
    }

    [Theory, MemberData(nameof(Data))]
    public void FullPipeline(OrderPlaced order) {
        var harness = new TestHarness();
        harness.Bus.Publish(order);
        Assert.True(harness.Published.Any<CustomerNotified>());
    }

    public static MatrixTheoryData<OrderPlaced> Data() => 
        new(new[] { new OrderPlaced(Guid.NewGuid(), "Alice") });
}


<|end|>
global using Contracts;

namespace Contracts {
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    public class PlaceOrderConsumer : IConsumer<PlaceOrder> {
        public async Task Consume(ConsumeContext<PlaceOrder> context) {
            var msg = context.Message;
            await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced> {
        public async Task Consume(ConsumeContext<OrderPlaced> context) {
            var msg = context.Message;
            await context.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }

    // xUnit v3 tests
}

global using MassTransit.Testing;
global using Microsoft.Extensions.DependencyInjection;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ConsumerTests {
    [Fact]
    public void PlaceOrderConsumerPublishesOrderPlaced() {
        var harness = new TestHarness();
        harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Alice"));
        Assert.True(harness.Consumed.Any<OrderPlaced>());
    }

    [Theory, MemberData(nameof(Data))]
    public void FullPipeline(OrderPlaced order) {
        var harness = new TestHarness();
        harness.Bus.Publish(order);
        Assert.True(harness.Published.Any<CustomerNotified>());
    }

    public static MatrixTheoryData<OrderPlaced> Data() => 
        new(new[] { new OrderPlaced(Guid.NewGuid(), "Alice") });
}


<|end|>
global using Contracts;

namespace Contracts {
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    public class PlaceOrderConsumer : IConsumer<PlaceOrder> {
        public async Task Consume(ConsumeContext<PlaceOrder> context) {
            var msg = context.Message;
            await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced> {
        public async Task Consume(ConsumeContext<OrderPlaced> context) {
            var msg = context.Message;
            await context.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }

    // xUnit v3 tests
}

global using MassTransit.Testing;
global using Microsoft.Extensions.DependencyInjection;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ConsumerTests {
    [Fact]
    public void PlaceOrderConsumerPublishesOrderPlaced() {
        var harness = new TestHarness();
        harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Alice"));
        Assert.True(harness.Consumed.Any<OrderPlaced>());
    }

    [Theory, MemberData(nameof(Data))]
    public void FullPipeline(OrderPlaced order) {
        var harness = new TestHarness();
        harness.Bus.Publish(order);
        Assert.True(harness.Published.Any<CustomerNotified>());
    }

    public static MatrixTheoryData<OrderPlaced> Data() => 
        new(new[] { new OrderPlaced(Guid.NewGuid(), "Alice") });
}


<|end|>
global using Contracts;

namespace Contracts {
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    public class PlaceOrderConsumer : IConsumer<PlaceOrder> {
        public async Task Consume(ConsumeContext<PlaceOrder> context) {
            var msg = context.Message;
            await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced> {
        public async Task Consume(ConsumeContext<OrderPlaced> context) {
            var msg = context.Message;
            await context.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }

    // xUnit v3 tests
}

global using MassTransit.Testing;
global using Microsoft.Extensions.DependencyInjection;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ConsumerTests {
    [Fact]
    public void PlaceOrderConsumerPublishesOrderPlaced() {
        var harness = new TestHarness();
        harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Alice"));
        Assert.True(harness.Consumed.Any<OrderPlaced>());
    }

    [Theory, MemberData(nameof(Data))]
    public void FullPipeline(OrderPlaced order) {
        var harness = new TestHarness();
        harness.Bus.Publish(order);
        Assert.True(harness.Published.Any<CustomerNotified>());
    }

    public static MatrixTheoryData<OrderPlaced> Data() => 
        new(new[] { new OrderPlaced(Guid.NewGuid(), "Alice") });
}


<|end|>
global using Contracts;

namespace Contracts {
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    public class PlaceOrderConsumer : IConsumer<PlaceOrder> {
        public async Task Consume(ConsumeContext<PlaceOrder> context) {
            var msg = context.Message;
            await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced> {
        public async Task Consume(ConsumeContext<OrderPlaced> context) {
            var msg = context.Message;
            await context.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }

    // xUnit v3 tests
}

global using MassTransit.Testing;
global using Microsoft.Extensions.DependencyInjection;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ConsumerTests {
    [Fact]
    public void PlaceOrderConsumerPublishesOrderPlaced() {
        var harness = new TestHarness();
        harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Alice"));
        Assert.True(harness.Consumed.Any<OrderPlaced>());
    }

    [Theory, MemberData(nameof(Data))]
    public void FullPipeline(OrderPlaced order) {
        var harness = new TestHarness();
        harness.Bus.Publish(order);
        Assert.True(harness.Published.Any<CustomerNotified>());
    }

    public static MatrixTheoryData<OrderPlaced> Data() => 
        new(new[] { new OrderPlaced(Guid.NewGuid(), "Alice") });
}


<|end|>
global using Contracts;

namespace Contracts {
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    public class PlaceOrderConsumer : IConsumer<PlaceOrder> {
        public async Task Consume(ConsumeContext<PlaceOrder> context) {
            var msg = context.Message;
            await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced> {
        public async Task Consume(ConsumeContext<OrderPlaced> context) {
            var msg = context.Message;
            await context.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }

    // xUnit v3 tests
}

global using MassTransit.Testing;
global using Microsoft.Extensions.DependencyInjection;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ConsumerTests {
    [Fact]
    public void PlaceOrderConsumerPublishesOrderPlaced() {
        var harness = new TestHarness();
        harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Alice"));
        Assert.True(harness.Consumed.Any<OrderPlaced>());
    }

    [Theory, MemberData(nameof(Data))]
    public void FullPipeline(OrderPlaced order) {
        var harness = new TestHarness();
        harness.Bus.Publish(order);
        Assert.True(harness.Published.Any<CustomerNotified>());
    }

    public static MatrixTheoryData<OrderPlaced> Data() => 
        new(new[] { new OrderPlaced(Guid.NewGuid(), "Alice") });
}


<|end|>
global using Contracts;

namespace Contracts {
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    public class PlaceOrderConsumer : IConsumer<PlaceOrder> {
        public async Task Consume(ConsumeContext<PlaceOrder> context) {
            var msg = context.Message;
            await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced> {
        public async Task Consume(ConsumeContext<OrderPlaced> context) {
            var msg = context.Message;
            await context.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }

    // xUnit v3 tests
}

global using MassTransit.Testing;
global using Microsoft.Extensions.DependencyInjection;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ConsumerTests {
    [Fact]
    public void PlaceOrderConsumerPublishesOrderPlaced() {
        var harness = new TestHarness();
        harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Alice"));
        Assert.True(harness.Consumed.Any<OrderPlaced>());
    }

    [Theory, MemberData(nameof(Data))]
    public void FullPipeline(OrderPlaced order) {
        var harness = new TestHarness();
        harness.Bus.Publish(order);
        Assert.True(harness.Published.Any<CustomerNotified>());
    }

    public static MatrixTheoryData<OrderPlaced> Data() => 
        new(new[] { new OrderPlaced(Guid.NewGuid(), "Alice") });
}


<|end|>
global using Contracts;

namespace Contracts {
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    public class PlaceOrderConsumer : IConsumer<PlaceOrder> {
        public async Task Consume(ConsumeContext<PlaceOrder> context) {
            var msg = context.Message;
            await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced> {
        public async Task Consume(ConsumeContext<OrderPlaced> context) {
            var msg = context.Message;
            await context.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }

    // xUnit v3 tests
}

global using MassTransit.Testing;
global using Microsoft.Extensions.DependencyInjection;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ConsumerTests {
    [Fact]
    public void PlaceOrderConsumerPublishesOrderPlaced() {
        var harness = new TestHarness();
        harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Alice"));
        Assert.True(harness.Consumed.Any<OrderPlaced>());
    }

    [Theory, MemberData(nameof(Data))]
    public void FullPipeline(OrderPlaced order) {
        var harness = new TestHarness();
        harness.Bus.Publish(order);
        Assert.True(harness.Published.Any<CustomerNotified>());
    }

    public static MatrixTheoryData<OrderPlaced> Data() => 
        new(new[] { new OrderPlaced(Guid.NewGuid(), "Alice") });
}


<|end|>
global using Contracts;

namespace Contracts {
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    public class PlaceOrderConsumer : IConsumer<PlaceOrder> {
        public async Task Consume(ConsumeContext<PlaceOrder> context) {
            var msg = context.Message;
            await context.Publish(new OrderPlaced(msg.OrderId, msg.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced> {
        public async Task Consume(ConsumeContext<OrderPlaced> context) {
            var msg = context.Message;
            await context.Publish(new CustomerNotified(msg.OrderId, $"Order {msg.OrderId} confirmed for {msg.CustomerName}"));
        }
    }

    // xUnit v3 tests
}

global using MassTransit.Testing;
global using Microsoft.Extensions.DependencyInjection;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ConsumerTests {
    [Fact]
    public void PlaceOrderConsumerPublishesOrderPlaced() {
        var harness = new TestHarness();
        harness.Bus.Publish(new PlaceOrder(Guid.NewGuid(), "Alice"));
        Assert.True(harness.Consumed.Any<OrderPlaced>());
    }

    [Theory, MemberData(nameof(Data))]
    public void FullPipeline(OrderPlaced order) {
        var harness = new TestHarness();
        harness.Bus.Publish(order);
        Assert.True(harness.Published.Any<CustomerNotified>());
    }

    public static MatrixTheoryData<OrderPlaced> Data() => 
        new(new[] { new OrderPlaced(Guid.NewGuid(), "Alice") });
}