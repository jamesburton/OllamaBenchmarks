using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

// Global using for file-scoped namespace access
global using Contracts;

namespace Contracts;

// 1. SubmitOrder message record
public record SubmitOrder(Guid OrderId, string CustomerName, decimal Amount);

// 2. OrderSubmitted event record
public record OrderSubmitted(Guid OrderId, DateTime SubmittedAt);

// 3. SubmitOrderConsumer class
public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        var message = context.Message;
        await context.Publish(new OrderSubmitted(message.OrderId, DateTime.UtcNow));
    }
}

// 4. SubmitOrderConsumerDefinition class
public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
{
    public SubmitOrderConsumerDefinition()
    {
        // Set ConcurrentMessageLimit = 4
        this.SetConcurrentMessageLimit(4);
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator ep,
        IConsumerConfigurator<SubmitOrderConsumer> consumer,
        IRegistrationContext ctx)
    {
        // Call ep.UseMessageRetry(r => r.Interval(3, 1000))
        ep.UseMessageRetry(r => r.Interval(3, 1000));
    }
}

// Example usage structure (not required for the output, but useful for context)
/*
public static class Program
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<SubmitOrderConsumer>();
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });
    }
}
*/