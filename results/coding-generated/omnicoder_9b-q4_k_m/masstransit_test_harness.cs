using MassTransit;
        using MassTransit.Testing;
        using Microsoft.Extensions.DependencyInjection;
        // xUnit v3 implicit usings handle Fact, Theory, Assert, etc.

var services = new ServiceCollection();
        var harness = services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        }).BuildServiceProvider(true);

await harness.Start();
        await harness.Bus.Publish(new ProcessPayment(Guid.NewGuid(), 100m));

var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();
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

var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();
        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );

var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();
        var publishedMsg = await harness.Published.Get<PaymentProcessed>(); // Is this allowed?

var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();
        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );

var publishedMsg = await harness.Published.Get<PaymentProcessed>();
        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published),
            () => Assert.Equal(context.Message.OrderId, publishedMsg.OrderId) // Wait, context is not available here
        );

var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new ProcessPayment(orderId, 100m));
        var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();
        var publishedMsg = await harness.Published.Get<PaymentProcessed>();
        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published),
            () => Assert.Equal(orderId, publishedMsg.OrderId)
        );

var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();
        var publishedMsg = await harness.Published.Get<PaymentProcessed>();
        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published),
            () => Assert.Equal(orderId, publishedMsg.OrderId)
        );