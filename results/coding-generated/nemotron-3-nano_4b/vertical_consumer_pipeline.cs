using MassTransit;
global using Contracts;

namespace Contracts
{
    // Message types
    public record PlaceOrder(Guid OrderId, string CustomerName);
    public record OrderPlaced(Guid OrderId, string CustomerName);
    public record CustomerNotified(Guid OrderId, string NotificationMessage);

    // Consumers
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
            var msg = new CustomerNotified(context.Message.OrderId, $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}");
            await context.Publish(msg);
        }
    }

    // Test harness (xUnit v3)
    public class MyTest : Test
    {
        [Fact]
        public async Task Publish_PlaceOrder_Publishes_OrderPlaced()
        {
            using var handler = new TestHarness();
            var harness = handler.GetHarness();

            // arrange
            await harness.Publish(new PlaceOrder(Guid.NewGuid(), "John Doe"));

            // assertions
            Assert.True(handler.Consumed.Any(msg => msg is OrderPlaced));
            Assert.False(handler.Consumed.Any(msg => msg is CustomerNotified));
        }

        [Theory, InlineData(typeof(Table).GetType().GetMember(nameof(TableData))[0].GetValue(null)!.GetProperty(nameof(TableData.Value))!.GetValue(null)!)]
        public async Task Verify_Full_Pipeline_TotalEvents()
        {
            ServiceProvider provider = new ServiceCollection()
                .AddMassTransit(x => x.AddMassTransitBusBusConfig()
                    .AddMassTransitHostingProvider()
                    .ConfigureBus()
                    .ConfigureConsumer<PlaceOrderConsumer>().ConfigureConsumer<NotifyCustomerConsumer>())
                .AddTransient<PlaceOrderConsumer>()
                .AddTransient<NotifyCustomerConsumer>()
                .BuildServiceProvider();

            using TestHarness hs = new TestHarness(provider);
            await hs.Start();
            await hs.Publish(new PlaceOrder(Guid.NewGuid(), "Jane Smith"));

            var consumed = hs.Consumed.Any(msg => msg is OrderPlaced && msg is CustomerNotified);
            var published = hs.Published.Any(msg => msg is OrderPlaced || msg is CustomerNotified);
            Assert.True(consumed);
            Assert.True(published);
        }
    }

    // Helper table data class to generate InlineData (simplified inline data constant)
    public static class Table
    {
        public static int[] Value { get; set; } = new int[2] { 1, 2 };
    }

    public class TableData
    {
        public int[] Value { get; set; }
    }
}

// Test harness for MassTransit v8
public class TestHarness : ITestHarness
{
    public IMessageHandler<PlaceOrder> Consumer<PlaceOrder> { get; }
    public IMessageHandler<OrderPlaced> Consumer<OrderPlaced> { get; }
    public IMessageHandler<CustomerNotified> Consumer<CustomerNotified> { get; }

    public MyTest TestInstance { get; }

    public async Task Start()
    {
        TestInstance = new MyTest();
        Consumer<PlaceOrder> = TestInstance;
        Consumer<OrderPlaced> = TestInstance;
        Consumer<CustomerNotified> = TestInstance;
        await TestInstance.InitializeAsync();
    }

    public async Task Dispose()
    {
        await TestInstance.DisposeAsync();
    }
}

// Extension for easy provider creation inside IAsyncLifetime
public class ServiceProviderHelper
{
    public static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddMassTransit(x => x.AddMassTransitBusBusConfig()
            .AddMassTransitHostingProvider()
            .ConfigureBus()
            .ConfigureConsumer<PlaceOrderConsumer>()
            .ConfigureConsumer<NotifyCustomerConsumer>());
        return services.BuildServiceProvider();
    }
}

// Extension methods used in tests
internal static class TestHelper
{
    public static TestHarness GetHarness(this ServiceProvider provider)
    {
        var helper = ServiceProviderHelper.BuildProvider();
        var harness = new TestHarness(helper);
        return harness;
    }
}