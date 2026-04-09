using MassTransit;
    using MassTransit.Testing;
    using Microsoft.Extensions.DependencyInjection;

    public class ProcessPaymentTests
    {
        [Fact]
        public async Task VerifyConsumerProcessesMessageAndPublishesEvent()
        {
            // Setup
            var provider = new ServiceCollection()
                .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            // Publish
            var message = new ProcessPayment(Guid.NewGuid(), 100m);
            await harness.Bus.Publish(message);

            // Check Consumed and Published
            bool consumed = await harness.Consumed.Any<ProcessPayment>();
            bool published = await harness.Published.Any<PaymentProcessed>();

            // Assert
            Assert.Multiple(
                () => Assert.True(consumed),
                () => Assert.True(published)
            );
        }
    }

public record ProcessPayment(Guid OrderId, decimal Amount);
        public record PaymentProcessed(Guid OrderId, string TransactionId);
        public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
        { ... }

public record ProcessPayment(Guid OrderId, decimal Amount);
        public record PaymentProcessed(Guid OrderId, string TransactionId);
        public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
        {
            public async Task Consume(ConsumeContext<ProcessPayment> context)
            {
                await context.Publish(new PaymentProcessed(context.Message.OrderId, Guid.NewGuid().ToString()));
            }
        }

... 
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public async Task VerifyConsumerProcessesMessageAndPublishesEvent()
    {
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var message = new ProcessPayment(Guid.NewGuid(), 100m);
        await harness.Bus.Publish(message);

        bool consumed = await harness.Consumed.Any<ProcessPayment>();
        bool published = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published)
        );
    }
}