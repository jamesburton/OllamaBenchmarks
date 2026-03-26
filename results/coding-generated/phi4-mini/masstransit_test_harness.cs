using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests : ITestHarnessBase
{
    private readonly TestHarness _harness;

    public void Setup()
    {
        var serviceProvider = new ServiceCollection().AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
            .BuildServiceProvider(true);

        _harness = GetRequiredService<TestHarness>();
        await harness.Start();
    }

    [Fact]
    public async Task ProcessPaymentIsConsumedAndPublished()
    {
        var orderId = Guid.NewGuid();

        // Arrange
        using (var scope = serviceProvider.CreateScope())
        {
            var processPaymentConsumer = scope.ServiceProvider.GetRequiredService<IConsumeFromTestHarness>().GetRequiredService<ProcessPaymentConsumer>();

            await _harness.Bus.Publish(new ProcessPayment(orderId, 100m));

            // Act & Assert
            bool isConsumed = await harness.Consumed.Any<ProcessPayment>(p => p.OrderId == orderId);
            bool wasPublished = false;

            if (isConsumed)
            {
                var publishedMessages = await _harness.Published.All<Message>();
                foreach (var message in publishedMessages.OfType<PaymentProcessed>())
                {
                    isPublished = true;
                    Assert.Equal(orderId, message.OrderId); // Check that the correct OrderId was used
                }
            }

            Assert.True(isConsumed);
            Assert.True(wasPublished);

            await _harness.DisposeAsync();
        }
    }
}