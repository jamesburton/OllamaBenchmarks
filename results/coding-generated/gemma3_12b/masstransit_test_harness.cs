using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
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

public class ProcessPaymentConsumerTests : IDisposable
{
    private ITestHarness? _harness;

    public ProcessPaymentConsumerTests(ITestOutputHelper output)
    {
        _harness = new InMemoryTestHarness(cfg =>
        {
            cfg.ConfigureEndpoints(endpointConfig =>
            {
                endpointConfig.UseInMemory(endpoint => endpoint.Consumer<ProcessPaymentConsumer>());
            });
        });
        _harness.TestCleanup += Output.WriteLine;
    }

    public void Dispose()
    {
        _harness?.Stop();
    }

    [Fact]
    public async Task ProcessPayment_PublishesPaymentProcessed()
    {
        await _harness.Start();

        var orderId = Guid.NewGuid();
        var amount = 100.00m;
        var message = new ProcessPayment(orderId, amount);

        await _harness.Bus.Publish(message);

        var paymentProcessed = await _harness.Consumed.Any<PaymentProcessed>();
        Assert.True(paymentProcessed);

        var paymentProcessedEvent = await _harness.Consumed.Received<PaymentProcessed>();
        Assert.Single(paymentProcessedEvent);

        Assert.Multiple(() =>
        {
            Assert.Equal(orderId, paymentProcessedEvent.First().OrderId);
            Assert.NotEmpty(paymentProcessedEvent.First().TransactionId);
        });

        await _harness.Stop();
    }
}