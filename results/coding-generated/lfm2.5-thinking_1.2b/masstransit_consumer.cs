using MassTransit;
namespace Contracts;
global using Contracts;
using MassTransit;

public record SubmitOrder(Guid OrderId, string CustomerName, decimal Amount);
public record OrderSubmitted(Guid OrderId, DateTime SubmittedAt);
public class SubmitOrderConsumer : IConsumer<SubmitOrder> {
    public async Task Consume(ConsumeContext<SubmitOrder> context) {
        var msg = context.Message;
        var orderId = msg.OrderId;
        var customerName = msg.CustomerName;
        var amount = msg.Amount;
        await context.Publish(new OrderSubmitted(orderId, DateTime.UtcNow));
    }
}
public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer> {
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator ep,
        IConsumerConfigurator<SubmitOrderConsumer> consumer,
        IRegistrationContext ctx)
    {
        ep.UseMessageRetry(r => r.Intervals(500, 1000, 2000));
        ep.UseInMemoryOutbox(ctx);
    }
}