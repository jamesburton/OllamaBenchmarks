using MassTransit.Testing;

public class TestPlaceOrder
{
    [Fact]
    public async Task Test()
    {
        var config = new MassTransit.ConfigureTest()
            .AddMassTransitTestHarness(cfg => cfg
                .AddConsumer<PlaceOrderConsumer>()
                .AddConsumer<NotifyCustomerConsumer>()
                .AddInMemoryEndpoints()
            );

        var harness = config.GetRequiredService<ITestHarness>();
        await harness.Start();

        // publish a PlaceOrder
        await harness BusPublish(paste: new PlaceOrder(1, "Alice"));

        // verify that the consumer consumed it and published OrderPlaced
        Assert (> 0 == harness.Consumed.Any<PlaceOrder>(c => c.OrderId == 1));
        Assert (> 0 == harness.Published.Any<OrderPlaced>(p => p.OrderId == 1));

        await harness.Stop();
    }
}

services.AddMassTransitTestHarness(cfg => cfg
    .AddConsumer<OrderConsumer>()
);
// In test:
var harness = provider.GetRequiredService<ITestHarness>();
await harness.Start();
await harness Bus Publish(new OrderPlaced(1));

var config = new MassTransit.ConfigureTest()
    .AddMassTransitTestHarness(cfg => cfg
        .AddConsumer<YourConsumer1>()
        .AddConsumer<YourConsumer2>()
        .AddInMemoryEndpoints()
    );

var harness = config.GetRequiredService<ITestHarness>();
await harness.Start();

// publish a message
await harness BusPublish(paste: new OrderPlaced(1));

// assertions
Assert (> 0 == harness.Consumed.Any<OrderPlaced>(c => c.OrderId == 1));

await harness BusPublish(new OrderPlaced(1));

var config = new MassTransit.ConfigureTest()
    .AddMassTransitTestHarness(cfg => cfg
        .AddConsumer<YourConsumer>()
        .AddInMemoryEndpoints()
    );

var harness = config.GetRequiredService<ITestHarness>();
await harness.Start();

// publish a message
await harness BusPublish(paste: new OrderPlaced(1));

// assertions
Assert (> 0 == harness.Consumed.Any<OrderPlaced>(c => c.OrderId == 1));

using MassTransit.Testing;

[Fact]
public async Task TestPlaceOrder()
{
    var config = new MassTransit.ConfigureTest()
        .AddMassTransitTestHarness(cfg => cfg
            .AddConsumer<OrderConsumer>()
        );

    var harness = config.GetRequiredService<ITestHarness>();
    await harness.Start();

    // publish a message
    await harness BusPublish(paste: new OrderPlaced(1));

    // verify that the consumer consumed it and published an OrderProcessed event
    Assert (> 0 == harness.Consumed.Any<OrderPlaced>(c => c.OrderId == 1));
    Assert (> 0 == harness.Published.Any<OrderProcessed>(p => p.OrderId == 1));
}

var config = new MassTransit.ConfigureTest()
    .AddMassTransitTestHarness(cfg => cfg
        .AddConsumer<YourConsumer>()
    );

var harness = config.GetRequiredService<ITestHarness>();
await harness.Start();

// publish a message
await harness BusPublish(paste: new OrderPlaced(1));

// assertions...

using MassTransit.Testing;
using Microsoft.Extensions.Logging; // maybe not needed

[Fact]
public async Task TestPlaceOrderConsumer()
{
    var config = new MassTransit.ConfigureTest()
        .AddMassTransitTestHarness(cfg => cfg
            .AddConsumer<PlaceOrderConsumer>()
            .AddConsumer<NotifyCustomerConsumer>()
            .AddInMemoryEndpoints()
        );

    var harness = config.GetRequiredService<ITestHarness>();
    await harness.Start();

    // publish a PlaceOrder message
    var place = new PlaceOrder Guid(123, "Alice");
    await harness.BusPublish(paste: place);

    // Verify that the PlaceOrderConsumer consumed it and published OrderPlaced
    Assert (> 0 == harness.Consumed.Any<PlaceOrder>(c => c.OrderId == 123));
    Assert (> 0 == harness.Published.Any<OrderPlaced>(p => p.OrderId == 123));

    // Wait for the second consumer to process the OrderPlaced message.
    // Since both consumers are asynchronous, we need to wait until both have completed.
    await harness.WaitAllConsumersDone();
}

[Fact]
public async Task TestFullPipeline()
{
    var config = new MassTransit.ConfigureTest()
        .AddMassTransitTestHarness(cfg => cfg
            .AddConsumer<PlaceOrderConsumer>()
            .AddConsumer<NotifyCustomerConsumer>()
        );
    // maybe also AddInMemoryEndpoints() automatically added?
    var harness = config.GetRequiredService<ITestHarness>();
    await harness.Start();

    // publish PlaceOrder
    var place = new PlaceOrder Guid(123, "Alice");
    await harness.BusPublish(paste: place);

    // wait a little for processing
    await Task.Delay(0.5);

    // assert consumption of PlaceOrder
    Assert (> 0 == harness.Consumed.Any<PlaceOrder>(c => c.OrderId == 123));

    // assert published OrderPlaced
    Assert (> 0 == harness.Published.Any<OrderPlaced>(p => p.OrderId == 123));

    // assert published CustomerNotified
    Assert (> 0 == harness.Published.Any<CustomerNotified>(n => n.OrderId == 123));
}

using MassTransit;
using Microsoft.Extensions.Logging;

global using Contracts;

namespace Contracts {
    // define records and consumer classes
    public record PlaceOrder Guid OrderId, string CustomerName;
    public record OrderPlaced Guid OrderId, string CustomerName;
    public record CustomerNotified Guid OrderId, string NotificationMessage;

    public class PlaceOrderConsumer : IConsumer<PlaceOrder> {
        public async Task Consume(ConsumeContext<PlaceOrder> context) {
            await context Publish(new OrderPlaced(context.OrderId, context.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced> {
        public async Task Consume(ConsumeContext<OrderPlaced> context) {
            var message = $"Order {context.OrderId} confirmed for {context.CustomerName}";
            await context Publish(new CustomerNotified(context.OrderId, message));
        }
    }
}

using xunit.v3;
[Fact]
public async Task TestPlaceOrderConsumer()
{
    // test code using harness and assertions
}

[Fact]
public async Task TestFullPipeline()
{
    // test code...
}

using MassTransit;
global using Contracts;

namespace Contracts {
    // define records
    public record PlaceOrder Guid OrderId, string CustomerName;
    public record OrderPlaced Guid OrderId, string CustomerName;
    public record CustomerNotified Guid OrderId, string NotificationMessage;

    // consumer implementations
    public class PlaceOrderConsumer : IConsumer<PlaceOrder> {
        public async Task Consume(ConsumeContext<PlaceOrder> context) {
            await context Publish(new OrderPlaced(context.OrderId, context.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced> {
        public async Task Consume(ConsumeContext<OrderPlaced> context) {
            var message = $"Order {context.OrderId} confirmed for {context.CustomerName}";
            await context Publish(new CustomerNotified(context.OrderId, message));
        }
    }
}

using xunit.v3;
// The test harness requires also MassTransit and Microsoft.Extensions.Logging per constraints
using MassTransit.Testing;
using Microsoft.Extensions.Logging;

[Fact]
public async Task TestPlaceOrderConsumer()
{
    var config = new MassTransit.ConfigureTest()
        .AddMassTransitTestHarness(cfg => cfg
            .AddConsumer<PlaceOrderConsumer>()
            .AddConsumer<NotifyCustomerConsumer>()
            .AddInMemoryEndpoints()
        );

    var harness = config.GetRequiredService<ITestHarness>();
    await harness.Start();

    // Create the PlaceOrder message
    var place = new PlaceOrder Guid(123, "Alice");

    // Publish it
    await harness.BusPublish(paste: place);

    // Verify that the consumer consumed it and published OrderPlaced
    Assert (> 0 == harness.Consumed.Any<PlaceOrder>(c => c.OrderId == 123));
    Assert (> 0 == harness.Published.Any<OrderPlaced>(p => p.OrderId == 123));

    await harness.Stop();
}

[Fact]
public async Task TestFullPipeline()
{
    var config = new MassTransit.ConfigureTest()
        .AddMassTransitTestHarness(cfg => cfg
            .AddConsumer<PlaceOrderConsumer>()
            .AddConsumer<NotifyCustomerConsumer>()
            .AddInMemoryEndpoints()
        );

    var harness = config.GetRequiredService<ITestHarness>();
    await harness.Start();

    var place = new PlaceOrder Guid(444, "Bob");

    await harness.BusPublish(paste: place);
    // small delay to allow first consumer to process
    await Task.Delay(0.5);

    // Verify consumption of the original message
    Assert (> 0 == harness.Consumed.Any<PlaceOrder>(c => c.OrderId == 444));
    // Verify that the first consumer published OrderPlaced
    Assert (> 0 == harness.Published.Any<OrderPlaced>(p => p.OrderId == 444));
    // Verify that the second consumer published CustomerNotified
    Assert (> 0 == harness.Published.Any<CustomerNotified>(n => n.OrderId == 444));

    await harness.Stop();
}

using MassTransit;
global using Contracts;

namespace Contracts {
    // definitions...
}

using xunit.v3;
using MassTransit.Testing;
using Microsoft.Extensions.Logging;

[Fact]
...

[Fact]
public async Task TestPlaceOrderConsumer()
{
    var config = new MassTransit.ConfigureTest()
        .AddMassTransitTestHarness(cfg => cfg
            .AddConsumer<PlaceOrderConsumer>()
            .AddConsumer<NotifyCustomerConsumer>()
            .AddInMemoryEndpoints()
        );

    var harness = config.GetRequiredService<ITestHarness>();
    await harness.Start();

    var place = new PlaceOrder Guid(123, "Alice");
    await harness.BusPublish(paste: place);

    // Ensure consumer consumed and published OrderPlaced
    Assert (> 0 == harness.Consumed.Any<PlaceOrder>(c => c.OrderId == 123));
    // Actually we can just use Any<PlaceOrder> but to be explicit maybe filter.
    // But they said use Any<T>, so:
    Assert (> 0 == harness.Published.Any<OrderPlaced>());

    await harness.Stop();
}

Assert(> 0 == harness.Consumed.Any<PlaceOrder>(c => c.OrderId == 123));

Assert(> 0 == harness.Consumed.Any<PlaceOrder>());
Assert(> 0 == harness.Published.Any<OrderPlaced>());

Assert(> 0 == something);

[Fact]
public async Task TestPlaceOrder()
{
    var config = new MassTransit.ConfigureTest()
        .AddMassTransitTestHarness(cfg => cfg
            .AddConsumer<YourConsumer>()
        );

    var harness = config.GetRequiredService<ITestHarness>();
    await harness.Start();

    // publish a message
    await harness.BusPublish(paste: new OrderPlaced(1));

    // verify that the consumer consumed it and published an OrderProcessed event
    Assert(> 0 == harness.Consumed.Any<OrderPlaced>(c => c.OrderId == 1));
    Assert(> 0 == harness.Published.Any<OrderProcessed>(p => p.OrderId == 1));
}

Assert(> 0 == harness.Consumed.Any<OrderPlaced>(c => c.OrderId == 1));

[Fact]
public async Task Test()
{
    var config = new MassTransit.ConfigureTest()
        .AddMassTransitTestHarness(cfg => cfg
            .AddConsumer<YourConsumer>()
        );

    var harness = config.GetRequiredService<ITestHarness>();
    await harness.Start();

    // publish a message
    await harness.BusPublish(paste: new OrderPlaced(1));

    // verify that the consumer consumed it and published an OrderProcessed event
    Assert(> 0 == harness.Consumed.Any<OrderPlaced>(c => c.OrderId == 1));
    Assert(> 0 == harness.Published.Any<OrderProcessed>(p => p.OrderId == 1));
}

[Fact]
public async Task TestPlaceOrderConsumer()
{
    var config = new MassTransit.ConfigureTest()
        .AddMassTransitTestHarness(cfg => cfg
            .AddConsumer<PlaceOrderConsumer>()
            .AddConsumer<NotifyCustomerConsumer>()
            .AddInMemoryEndpoints()
        );

    var harness = config.GetRequiredService<ITestHarness>();
    await harness.Start();

    var place = new PlaceOrder Guid(123, "Alice");
    await harness.BusPublish(paste: place);

    // Ensure consumer consumed the PlaceOrder and published OrderPlaced
    Assert(> 0 == harness.Consumed.Any<PlaceOrder>(c => c.OrderId == 123));
    Assert(> 0 == harness.Published.Any<OrderPlaced>(p => p.OrderId == 123));

    await harness.Stop();
}

[Fact]
public async Task TestFullPipeline()
{
    var config = new MassTransit.ConfigureTest()
        .AddMassTransitTestHarness(cfg => cfg
            .AddConsumer<PlaceOrderConsumer>()
            .AddConsumer<NotifyCustomerConsumer>()
            .AddInMemoryEndpoints()
        );

    var harness = config.GetRequiredService<ITestHarness>();
    await harness.Start();

    var place = new PlaceOrder Guid(444, "Bob");
    await harness.BusPublish(paste: place);

    // Wait a short while for the consumers to process
    await Task.Delay(0.5);

    // Verify consumption of the original message
    Assert(> 0 == harness.Consumed.Any<PlaceOrder>(c => c.OrderId == 444));
    // Verify that the first consumer published OrderPlaced
    Assert(> 0 == harness.Published.Any<OrderPlaced>(p => p.OrderId == 444));
    // Verify that the second consumer published CustomerNotified
    Assert(> 0 == harness.Published.Any<CustomerNotified>(n => n.OrderId == 444));

    await harness.Stop();
}

using MassTransit;
global using Contracts;

namespace Contracts {
    // definitions
}

using MassTransit;
global using Contracts;

namespace Contracts {
    // define records
    public record PlaceOrder Guid OrderId, string CustomerName;
    public record OrderPlaced Guid OrderId, string CustomerName;
    public record CustomerNotified Guid OrderId, string NotificationMessage;

    // consumer implementations
    public class PlaceOrderConsumer : IConsumer<PlaceOrder> {
        public async Task Consume(ConsumeContext<PlaceOrder> context) {
            await context Publish(new OrderPlaced(context.OrderId, context.CustomerName));
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced> {
        public async Task Consume(ConsumeContext<OrderPlaced> context) {
            var message = $"Order {context.OrderId} confirmed for {context.CustomerName}";
            await context Publish(new CustomerNotified(context.OrderId, message));
        }
    }
}

using xunit.v3;
using MassTransit.Testing;
using Microsoft.Extensions.Logging;

[Fact]
public async Task TestPlaceOrderConsumer()
{
    var config = new MassTransit.ConfigureTest()
        .AddMassTransitTestHarness(cfg => cfg
            .AddConsumer<PlaceOrderConsumer>()
            .AddConsumer<NotifyCustomerConsumer>()
            .AddInMemoryEndpoints()
        );

    var harness = config.GetRequiredService<ITestHarness>();
    await harness.Start();

    var place = new PlaceOrder Guid(123, "Alice");
    await harness.BusPublish(paste: place);

    // Verify that the PlaceOrderConsumer consumed it and published OrderPlaced
    Assert(> 0 == harness.Consumed.Any<PlaceOrder>(c => c.OrderId == 123));
    Assert(> 0 == harness.Published.Any<OrderPlaced>(p => p.OrderId == 123));

    await harness.Stop();
}

[Fact]
public async Task TestFullPipeline()
{
    var config = new MassTransit.ConfigureTest()
        .AddMassTransitTestHarness(cfg => cfg
            .AddConsumer<PlaceOrderConsumer>()
            .AddConsumer<NotifyCustomerConsumer>()
            .AddInMemoryEndpoints()
        );

    var harness = config.GetRequiredService<ITestHarness>();
    await harness.Start();

    var place = new PlaceOrder Guid(444, "Bob");
    await harness.BusPublish(paste: place);

    // small delay to allow first consumer to process
    await Task.Delay(0.5);

    // Verify consumption of the original message
    Assert(> 0 == harness.Consumed.Any<PlaceOrder>(c => c.OrderId == 444));
    // Verify that the first consumer published OrderPlaced
    Assert(> 0 == harness.Published.Any<OrderPlaced>(p => p.OrderId == 444));
    // Verify that the second consumer published CustomerNotified
    Assert(> 0 == harness.Published.Any<CustomerNotified>(n => n.OrderId == 444));

    await harness.Stop();
}