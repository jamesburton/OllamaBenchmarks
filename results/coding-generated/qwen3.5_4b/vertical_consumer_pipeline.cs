using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

global using Contracts;

namespace Tests;

public class OrderPipelineTests
{
    private readonly ITestHarness _harness;

    public OrderPipelineTests(ITestHarness harness)
    {
        _harness = harness;
    }

    [Fact]
    public async Task PublishesOrderPlacedAfterPlaceOrder()
    {
        await _harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "John Doe";

        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        // Verify PlaceOrder was consumed
        Assert.True(await _harness.Consumed.Any<PlaceOrder>());

        // Verify OrderPlaced was published and consumed
        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Consumed.Any<OrderPlaced>());

        await _harness.Stop();
    }

    [Fact]
    public async Task FullPipelineCompletesEndToEnd()
    {
        await _harness.Start();

        var orderId = Guid.NewGuid();
        var customerName = "Jane Smith";

        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        // Wait for async operations to complete
        await Task.Delay(100);

        // Verify full chain: PlaceOrder -> OrderPlaced -> CustomerNotified
        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Consumed.Any<OrderPlaced>());
        Assert.True(await _harness.Consumed.Any<CustomerNotified>());

        await _harness.Stop();
    }
}