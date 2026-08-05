using MassTransit;

namespace Contracts;

global using Contracts;

public record ProcessPayment(Guid OrderId, decimal Amount, string Currency) : ICommand;
public record PaymentProcessed(Guid OrderId, DateTime ProcessedAt) : IEvent;

public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        var msg = context.Message;
        if (msg.Amount <= 0)
            return;
        await context.Publish(new PaymentProcessed(msg.OrderId, DateTime.UtcNow));
    }
}

public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
                                              IConsumerConfigurator<ProcessPaymentConsumer> consumerConfigurator,
                                              IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Interval(2, 500));
    }

    public override int ConcurrentMessageLimit => 2;
}