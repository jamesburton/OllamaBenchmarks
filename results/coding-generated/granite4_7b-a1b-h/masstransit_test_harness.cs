using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Xunit;
using Xunit.Abstractions;

public class ProcessPaymentConsumerTests
{
    private readonly ITestOutputHelper _output;

    public ProcessPaymentConsumerTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Consume_ProcessPayment_PublishesPaymentProcessed()
    {
        // Arrange
        var harness = await TestHarness.CreateAsync(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });

        var message = new ProcessPayment(Guid.NewGuid(), 100.00m);

        // Act
        await harness.Bus.Publish(message);

        // Assert
        await harness.Consumed.Any<PaymentProcessed>(msg =>
            msg.OrderId == message.OrderId);

        _output.WriteLine($"Processed message: {message.OrderId}");
    }

    [Fact]
    public async Task Consume_ProcessPayment_PublishesCorrectTransactionId()
    {
        // Arrange
        var harness = await TestHarness.CreateAsync(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });

        var message = new ProcessPayment(Guid.NewGuid(), 100.00m);

        // Act
        await harness.Bus.Publish(message);

        // Assert
        var transactionId = await harness.Consumed.Single<PaymentProcessed>(msg =>
            msg.OrderId == message.OrderId);
        Assert.NotEqual(Guid.Empty.ToString(), transactionId.TransactionId);
    }

    [Fact]
    public async Task Consume_ProcessPayment_PublishesCorrectTransactionIdUsingMultiple()
    {
        // Arrange
        var harness = await TestHarness.CreateAsync(cfg =>
        {
            cfg.AddConsumer<ProcessPaymentConsumer>();
        });

        var message = new ProcessPayment(Guid.NewGuid(), 100.00m);

        // Act
        await harness.Bus.Publish(message);

        // Assert
        Assert.Multiple(() =>
        {
            var transactionId = await harness.Consumed.Single<PaymentProcessed>(msg =>
                msg.OrderId == message.OrderId);
            Assert.NotEqual(Guid.Empty.ToString(), transactionId.TransactionId);

            _output.WriteLine($"Processed message: {message.OrderId}");
        });
    }
}