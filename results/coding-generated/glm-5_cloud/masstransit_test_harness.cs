using MassTransit;
    using MassTransit.Testing;
    using Microsoft.Extensions.DependencyInjection;

    public class ProcessPaymentConsumerTests
    {
        [Fact]
        public async Task ProcessPayment_ShouldConsumeAndPublish()
        {
            var provider = new ServiceCollection()
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<ProcessPaymentConsumer>();
                })
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();

            await harness.Start();

            var orderId = Guid.NewGuid();
            var amount = 100m;

            await harness.Bus.Publish(new ProcessPayment(orderId, amount));

            // Wait for consumption and publication
            // The prompt says: "Await each harness check individually first"
            var consumed = await harness.Consumed.Any<ProcessPayment>();
            var published = await harness.Published.Any<PaymentProcessed>(x => x.Context.Message.OrderId == orderId);

            Assert.Multiple(
                () => Assert.True(consumed, "ProcessPayment message was not consumed"),
                () => Assert.True(published, "PaymentProcessed message was not published with correct OrderId")
            );
        }
    }