using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

public class ProcessPaymentConsumerTests : IAsyncLifetime
{
    private readonly ServiceProvider _provider;
    private readonly ITestHarness _harness;

    public ProcessPaymentConsumerTests()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
    }

    public async ValueTask InitializeAsync()
    {
        await _harness.Start();
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task Should_Consume_ProcessPayment_And_Publish_PaymentProcessed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var amount = 99.99m;

        // Act
        await _harness.Bus.Publish(new ProcessPayment(orderId, amount));

        // Assert — await each harness check individually first because
        // Assert.Multiple does not support async lambdas.
        var wasConsumed = await _harness.Consumed.Any<ProcessPayment>();
        var wasPublished = await _harness.Published.Any<PaymentProcessed>(
            x => x.Message.OrderId == orderId);

        Assert.Multiple(
            () => Assert.True(wasConsumed, "Consumer should have received the ProcessPayment message"),
            () => Assert.True(wasPublished, "PaymentProcessed should be published with the matching OrderId")
        );
    }

    [Theory]
    [InlineData(10.50)]
    [InlineData(99.99)]
    [InlineData(1500.00)]
    public async Task Should_Process_Multiple_Amounts(decimal amount)
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        await _harness.Bus.Publish(new ProcessPayment(orderId, amount));

        // Assert