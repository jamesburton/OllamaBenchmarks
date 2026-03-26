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
        private readonly IBus _bus;

        public PlaceOrderConsumer(IBus bus)
        {
            _bus = bus;
        }

        public async Task Consume(ConsumeContext<PlaceOrder> context)
        {
            var orderPlacedEvent = new OrderPlaced(context.Message.OrderId, context.Message.CustomerName);
            await _bus.Publish(orderPlacedEvent);
        }
    }

    public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
    {
        private readonly IBus _bus;

        public NotifyCustomerConsumer(IBus bus)
        {
            _bus = bus;
        }

        public async Task Consume(ConsumeContext<OrderPlaced> context)
        {
            var customerNotifiedEvent = new CustomerNotified(context.Message.OrderId, $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}");
            await _bus.Publish(customerNotifiedEvent);
        }
    }

    [assembly: AssemblyFixture(typeof(DatabaseFixture))]
}

public class PlaceOrderConsumerTests
{
    private readonly ITestHarness _harness;

    public PlaceOrderConsumerTests(ITestHarness harness)
    {
        _harness = harness;
    }

    [Fact]
    public async Task Test_Consume_OrderPlaced()
    {
        var consumer = new PlaceOrderConsumer(_harness.Bus);
        await _harness.Start();
        Guid orderId = Guid.NewGuid();

        using (var scope = _harness.CreateScope())
        {
            await scope.Send(new PlaceOrder(orderId, "Customer A"));
        }

        Assert.True(await _harness.Consumed.Any<OrderPlaced>());
    }
}

public class FullPipelineTests
{
    private readonly ITestHarness _harness;

    public FullPipelineTests(ITestHarness harness)
    {
        _harness = harness;
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void Test_Pipeline_Consuming_OrderPlaced_CustomerNotified(int orderId, string customerName)
    {
        var consumer1 = new PlaceOrderConsumer(_harness.Bus);
        var consumer2 = new NotifyCustomerConsumer(_harness.Bus);

        await _harness.Start();

        using (var scope = _harness.CreateScope())
        {
            Guid id;
            if (!await Task.FromResult(id = int.TryParse(orderId, out id)))
                throw new Exception("Invalid order ID");

            var placeOrderMessage = new PlaceOrder(Guid.NewGuid(), "Customer A");
            await scope.Send(placeOrderMessage);

            Assert.True(await _harness.Consumed.Any<OrderPlaced>());
            Assert.True(_harness.Published.Any<NotifyCustomerConsumer>(c => c.Message.OrderId == id));
        }
    }

    public static IEnumerable<object[]> Data =>
        new[]
        {
            (1, "A"),
            (2, "B")
        };
}