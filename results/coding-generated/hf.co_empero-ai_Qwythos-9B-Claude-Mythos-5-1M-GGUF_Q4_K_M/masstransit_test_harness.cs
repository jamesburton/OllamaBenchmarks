using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_WhenMessageReceived_ShouldPublishEvent()
    {
        var harness = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true);

        using (var scope = harness.CreateScope())
        {
            var harnessInstance = scope.ServiceProvider.GetRequiredService<ITestHarness>();
            await harnessInstance.Start();

            bool consumed = await harness.Consumed.Any<ProcessPayment>();
            bool published = await harness.Published.Any<PaymentProcessed>();

            Assert.Multiple(
                () => Assert.True(consumed, "Consumer did not receive the message"),
                () => Assert.True(published, "Event was not published")
            );
        }
    }
}