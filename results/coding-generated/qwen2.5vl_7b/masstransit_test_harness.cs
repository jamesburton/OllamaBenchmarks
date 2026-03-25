using MassTransit;
using MassTransit.Testing;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

public record ProcessPayment(Guid OrderId, decimal Amount);
public record PaymentProcessed(Guid OrderId, string TransactionId);
public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        await context.Publish(new PaymentProcessed(context.Message.OrderId, Guid.NewGuid().ToString()));
    }
}

public class ProcessPaymentConsumerTests
{
    private readonly ITestHarness _harness;

    public ProcessPaymentConsumerTests(ITestHarness harness)
    {
        _harness = harness;
    }

    [Fact]
    public async Task ProcessPayment_Consumer_Publishes_PaymentProcessed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var amount = 100m;

        // Act
        await _harness.Bus.Publish(new ProcessPayment(orderId, amount));

        // Assert
        Assert.Multiple(
            () => Assert.True(await _harness.Consumed.Any<ProcessPayment>()),
            () => Assert.True(await _harness.Consumed.Any<PaymentProcessed>())
        );
    }
}

sh
   dotnet add package MassTransit.Testing
   dotnet add package xunit
   dotnet add package xunit.runner.visualstudio

using MassTransit;
using MassTransit.Testing;
using System;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = new ContainerBuilder();
        var container = builder.Build();

        var harness = new TestHarness(container);

        await harness.Start();

        var processPaymentConsumer = new ProcessPaymentConsumer();
        var processPayment = new ProcessPayment(Guid.NewGuid(), 100m);

        await harness.Bus.Publish(processPayment);

        await harness.Stop();
    }
}