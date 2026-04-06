using MassTransit;
        using MassTransit.Testing;
        using Microsoft.Extensions.DependencyInjection;
        // xUnit v3 implicit usings handle Fact, Assert, etc.

public class ProcessPaymentTests
        {
            // ...
        }

[Fact]
        public async Task ProcessPaymentConsumer_ProcessesMessageAndPublishesEvent()
        {
            // Build harness
            var services = new ServiceCollection();
            var harness = new ServiceCollection().AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            }).BuildServiceProvider(true);

            // Start harness
            var bus = harness.GetRequiredService<IBusControl>();
            // Wait, the prompt says:
            // var harness = provider.GetRequiredService<ITestHarness>();
            // But the constraint says:
            // Build test harness with: new ServiceCollection().AddMassTransitTestHarness(...).BuildServiceProvider(true);
            // Call await harness.Start();
            // So I need to capture the harness instance.
            // The pattern usually is:
            // var harness = new ServiceCollection().AddMassTransitTestHarness(...).BuildServiceProvider(true);
            // var bus = harness.GetRequiredService<IBusControl>();
            // await harness.Start();
            // ...
            // await harness.Consumed.Any<T>();
            // await harness.Published.Any<T>();
            // Assert.Multiple(...)
        }

var serviceProvider = new ServiceCollection().AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        }).BuildServiceProvider(true);

        var harness = serviceProvider.GetRequiredService<ITestHarness>();

var message = new ProcessPayment(Guid.NewGuid(), 100m);
        await harness.Bus.Publish(message);

var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed, "Consumer should have consumed the message"),
            () => Assert.True(published, "Event should have been published")
        );

var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );

Assert.Multiple(
            () => Assert.Equal(1, a),
            () => Assert.Equal(2, b)
        );

Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );

var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );

Assert.Multiple(
            () => Assert.Equal(1, a),
            () => Assert.Equal(2, b)
        );