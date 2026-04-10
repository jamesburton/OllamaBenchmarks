using MassTransit;
using System;
using System.Threading.Tasks;

// Global using statement for file-scoped namespace access
global using Contracts;

namespace Contracts;

// 1. Message Record
public record ProcessPayment(Guid OrderId, decimal Amount, string Currency);

// 2. Event Record
public record PaymentProcessed(Guid OrderId, DateTime ProcessedAt);

// 3. Consumer Implementation
public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        var message = context.Message;

        // Check if amount is zero or negative
        if (message.Amount <= 0)
        {
            // Return immediately if amount is invalid
            return;
        }

        // Publish the event
        await context.Publish(new PaymentProcessed(message.OrderId, DateTime.UtcNow));
    }
}

// 4. Consumer Definition
public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
{
    public ProcessPaymentConsumerDefinition()
    {
        // Set ConcurrentMessageLimit
        this.ConcurrentMessageLimit = 2;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator ep,
        IConsumerConfigurator<ProcessPaymentConsumer> consumer,
        IRegistrationContext ctx)
    {
        // Configure retry policy
        ep.UseMessageRetry(r => r.Interval(2, 500));
    }
}