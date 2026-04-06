using MassTransit;
    using MassTransit.Testing;
    using Microsoft.Extensions.DependencyInjection;

    public class ProcessPaymentTests
    {
        [Fact]
        public async Task ProcessPaymentConsumer_PublishesPaymentProcessed()
        {
            var provider = new ServiceCollection()
                .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var orderId = Guid.NewGuid();
            var amount = 100m;
            await harness.Bus.Publish(new ProcessPayment(orderId, amount));

            var consumed = await harness.Consumed.Any<ProcessPayment>();
            var published = await harness.Published.Any<PaymentProcessed>();

            Assert.Multiple(
                () => Assert.True(consumed),
                () => Assert.True(published)
            );
        }
    }

var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();
        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );