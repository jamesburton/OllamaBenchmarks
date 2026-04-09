using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Consumer_Should_Process_Payment_And_Publish_Event()
    {
        // Arrange: build the test harness with the consumer
        var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            var orderId = Guid.NewGuid();
            var payment = new ProcessPayment(orderId, 99.95m);

            // Act: publish the command message
            await harness.Bus.Publish(payment);

            // Assert: wait for consumption and publishing
            bool consumed = await harness.Consumed.Any<ProcessPayment>();
            var publishedMessage = harness.Published
                .Select<PaymentProcessed>()
                .FirstOrDefault(m => m.Context.Message.OrderId == orderId);

            bool publishedWithCorrectOrderId = publishedMessage != null;

            Assert.Multiple(
                () => Assert.True(consumed, "ProcessPayment message was not consumed by the consumer."),
                () => Assert.True(publishedWithCorrectOrderId, "PaymentProcessed event was not published with the correct OrderId.")
            );
        }
        finally
        {
            await harness.Stop();
        }
    }
}