using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentTests
{
    [Fact]
    public async Task ProcessPayment_ShouldProcessAndPublishCorrectEvent()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });

        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var message = new ProcessPayment(orderId, 100.0m);

        // Act
        await harness.Bus.Publish(message);

        // Assert
        bool wasConsumed = await harness.Consumed.Any<ProcessPayment>();
        bool wasPublished = await harness.Published.Any<PaymentProcessed>();

        Assert.Multiple(
            () => Assert.True(wasConsumed,