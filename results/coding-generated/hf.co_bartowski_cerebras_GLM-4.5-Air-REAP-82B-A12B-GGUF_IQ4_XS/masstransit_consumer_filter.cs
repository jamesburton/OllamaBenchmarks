global using Contracts;

using MassTransit;

namespace Contracts;

public record ProcessPayment(Guid OrderId, decimal Amount, string Currency);
public record PaymentProcessed(Guid OrderId, DateTime ProcessedAt);

public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        if (context.Message.Amount <= 0)
            return;

        await context.Publish(new PaymentProcessed(context.Message.OrderId, DateTime.UtcNow));
    }
}

public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
{
    public ProcessPaymentConsumerDefinition()
    {
        ConcurrentMessageLimit = 2;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpoint,
        IConsumerConfigurator<ProcessPaymentConsumer> consumer,
        IRegistrationContext context)
    {
        endpoint.UseMessageRetry(r => r.Interval(2, 500));
    }
}