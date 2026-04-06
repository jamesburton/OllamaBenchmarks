global using MassTransit;

namespace Contracts;

public record ProcessPayment(
    Guid OrderId,
    decimal Amount,
    string Currency);

public record PaymentProcessed(
    Guid OrderId,
    DateTime ProcessedAt);

public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        var msg = context.Message;
        if (msg.Amount <= 0)
        {
            return; // No further action needed
        }

        await context.Publish(new PaymentProcessed(msg.OrderId, DateTime.UtcNow));
    }
}

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