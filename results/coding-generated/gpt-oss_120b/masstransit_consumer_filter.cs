global using Contracts;

using System;
using MassTransit;

namespace Contracts;

record ProcessPayment(Guid OrderId, decimal Amount, string Currency);
record PaymentProcessed(Guid OrderId, DateTime ProcessedAt);

class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        var msg = context.Message;
        if (msg.Amount <= 0)
            return;

        await context.Publish(new PaymentProcessed(msg.OrderId, DateTime.UtcNow));
    }
}

class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
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