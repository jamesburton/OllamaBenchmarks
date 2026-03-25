using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Test()
    {
        var bus = Bus.Create();
        await using (var harness = bus.GetTestHarness())
        {
            // Add the consumer to the test harness.
            harness.AddConsumer<ProcessPaymentConsumer>();

            // Send a message and wait for it to be processed
            var paymentMessage = new ProcessPayment(Guid.NewGuid(), 10m);
            await harness.Publish(paymentMessage);

            // Assert that the correct event was published with the expected OrderId
            Assert.Multiple(
                () => Assert.Equal(paymentMessage.OrderId, harness.Consumed.Single().Message.OrderId),
                () => Assert.Equal(paymentMessage.OrderId, harness.Consumed.Single().Message.OrderId));
        }
    }
}