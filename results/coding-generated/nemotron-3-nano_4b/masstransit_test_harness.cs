using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class TestClass
{
    private ServiceProvider provider;
    private ITestHarness harness;

    public ValueTask InitializeAsync()
    {
        provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true);
        harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        return default;
    }

    public ValueTask DisposeAsync()
    {
        await harness.DisposeAsync();
        provider.Dispose();
        return default;
    }

    [Fact]
    public async ValueTask Test()
    {
        await harness.Publish(new ProcessPayment(Guid.NewGuid(), 1m));

        bool consumed = await harness.Consumed.Any<ProcessPayment>();
        bool published = await harness.Published.Any<PaymentProcessed>();

        var ok1 = consumed;
        var ok2 = published;

        Assert.Multiple(() => Assert.True(ok1),
                        () => Assert.True(ok2));
    }
}