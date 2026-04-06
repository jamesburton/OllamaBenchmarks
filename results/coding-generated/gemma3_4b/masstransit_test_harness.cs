using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransitTests
{
    public class ProcessPaymentConsumerTests
    {
        private ServiceProvider _provider;

        public ProcessPaymentConsumerTests()
        {
            _provider = new ServiceCollection()
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<ProcessPaymentConsumer>();
                })
                .BuildServiceProvider(true);
        }

        [Fact]
        public async Task ProcessPayment_ReceivesAndProcessesMessage()
        {
            var harness = _provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var payment = new ProcessPayment(Guid.NewGuid(), 100.0m);
            await harness.Bus.Publish(payment);

            await harness.Consumed.Any<ProcessPayment>();
        }

        [Fact]
        public async Task ProcessPayment_PublishesPaymentProcessedEvent()
        {
            var harness = _provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var payment = new ProcessPayment(Guid.NewGuid(), 100.0m);
            await harness.Bus.Publish(payment);

            var publishedEvent = await harness.Published.Any<PaymentProcessed>();
            Assert.True(publishedEvent, "PaymentProcessed event was not published.");
        }

        [Fact]
        public async Task ProcessPayment_AssertMultiple()
        {
            var harness = _provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var payment = new ProcessPayment(Guid.NewGuid(), 100.0m);
            await harness.Bus.Publish(payment);

            var consumedEvent = await harness.Consumed.Any<ProcessPayment>();
            Assert.True(consumedEvent, "ProcessPayment message was not consumed.");

            var publishedEvent = await harness.Published.Any<PaymentProcessed>();
            Assert.True(publishedEvent, "PaymentProcessed event was not published.");
        }
    }
}