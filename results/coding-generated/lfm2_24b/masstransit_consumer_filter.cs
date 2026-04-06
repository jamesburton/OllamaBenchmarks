using MassTransit;
global using Contracts;

namespace Contracts;

// 1. Message record for payment
public record ProcessPayment(Guid OrderId, decimal Amount, string Currency);

// 2. Event record for payment processed
public record PaymentProcessed(Guid OrderId, DateTime ProcessedAt);

// 3. Consumer for processing payment
public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        if (context.Message.Amount <= 0)
            return;

        await context.Publish(new PaymentProcessed(context.Message.OrderId, DateTime.UtcNow));
    }
}

// 4. Consumer definition with retry and outbox
public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator ep, IConsumerConfigurator<ProcessPaymentConsumer> consumer, IRegistrationContext ctx)
    {
        ep.UseMessageRetry(r => r.Interval(2, 500));
        ep.UseInMemoryOutbox(ctx);
    }

    public ProcessPaymentConsumerDefinition()
    {
        ConcurrentMessageLimit = 2;
    }
}