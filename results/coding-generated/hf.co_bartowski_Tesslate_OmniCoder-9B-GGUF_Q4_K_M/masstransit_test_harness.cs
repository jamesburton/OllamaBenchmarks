using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ShouldPublishPaymentProcessed()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });
        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100m;
        var message = new ProcessPayment(orderId, amount);

        await harness.Bus.Publish(message);

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );
    }
}

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ShouldPublishPaymentProcessed()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });
        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100m;
        var message = new ProcessPayment(orderId, amount);

        await harness.Bus.Publish(message);

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );
    }
}

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ShouldPublishPaymentProcessed()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });
        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100m;
        var message = new ProcessPayment(orderId, amount);

        await harness.Bus.Publish(message);

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );
    }
}

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ShouldPublishPaymentProcessed()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });
        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100m;
        var message = new ProcessPayment(orderId, amount);

        await harness.Bus.Publish(message);

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );
    }
}

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ShouldPublishPaymentProcessed()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });
        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100m;
        var message = new ProcessPayment(orderId, amount);

        await harness.Bus.Publish(message);

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );
    }
}

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ShouldPublishPaymentProcessed()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });
        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100m;
        var message = new ProcessPayment(orderId, amount);

        await harness.Bus.Publish(message);

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );
    }
}

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ShouldPublishPaymentProcessed()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });
        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100m;
        var message = new ProcessPayment(orderId, amount);

        await harness.Bus.Publish(message);

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );
    }
}

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_ShouldPublishPaymentProcessed()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });
        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100m;
        var message = new ProcessPayment(orderId, amount);

        await harness.Bus.Publish(message);

        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );
    }
}