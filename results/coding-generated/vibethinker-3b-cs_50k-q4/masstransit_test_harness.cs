using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.Logging;

public class TestPayment
{
    public void Test() =>
        Assert.Multiple(
            () => await harness.Consumed.Any<ProcessPaymentProcessed>().All().Any(),
            () => await harness.Published.Any<PaymentProcessed>().All().Any()
        );
}