using MassTransit;

global using Contracts;

namespace Contracts;

// ProcessPayment message record
public record ProcessPayment(Guid OrderId, decimal Amount, string Currency);

// PaymentProcessed event record
public record PaymentProcessed(Guid OrderId, DateTime ProcessedAt);

// ProcessPaymentConsumer class
public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        if (context.Message.Amount <= 0)
        {
            return;
        }

        await context.Publish(new PaymentProcessed(
            context.Message.OrderId,
            DateTime.UtcNow));
    }
}

// ProcessPaymentConsumerDefinition class
public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
{
    public ProcessPaymentConsumerDefinition()
    {
        ConcurrentMessageLimit = 2;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator ep,
        IConsumerConfigurator<ProcessPaymentConsumer> consumer,
        IRegistrationContext ctx)
    {
        ep.UseMessageRetry(r => r.Interval(2, 500));
    }
}

ep.UseMessageRetry(r => r.Intervals(500, 1000, 2000)););

But the user specifically asked for `r.Interval(2, 500)` which suggests。

Actually)