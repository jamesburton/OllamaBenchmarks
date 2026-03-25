using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;

public class SubmitOrder
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; }
    public decimal Amount { get; set; }
}

public class OrderSubmitted
{
    public Guid OrderId { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        var msg = context.Message;
        await context.Publish(new OrderSubmitted
        {
            OrderId = msg.OrderId,
            SubmittedAt = DateTime.UtcNow
        });
    }
}

public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
{
    public SubmitOrderConsumerDefinition()
    {
        ConcurrentMessageLimit = 4;
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator ep, IConsumerConfigurator<SubmitOrderConsumer> consumer, IRegistrationContext ctx)
    {
        ep.UseMessageRetry(r => r.Interval(3, 1000));
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var busControl = Bus.Factory.CreateUsingInMemory((ctx, cfg) =>
        {
            cfg.UseInMemoryOutbox();
            cfg.ConfigureEndpoints(ctx);
        });

        var consumer = new SubmitOrderConsumerDefinition();
        consumer.ConfigureEndpoint(busControl);

        var harness = new TestHarness(busControl);
        await harness.Start();

        var submitOrder = new SubmitOrder
        {
            OrderId = Guid.NewGuid(),
            CustomerName = "John Doe",
            Amount = 100.00m
        };

        await harness.Bus.Publish(submitOrder);

        await Task.Delay(1000);

        Assert.True(await harness.Consumed.Any<OrderSubmitted>());
    }
}