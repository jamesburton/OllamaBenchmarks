# MassTransit v8 — Quick Reference

**Default serializer:** System.Text.Json

## Registration

```csharp
services.AddMassTransit(x =>
{
    x.AddConsumer<OrderConsumer>();
    x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
});
// Use IBusRegistrationConfigurator (NOT IServiceCollectionBusConfigurator)
```

## Consumer

```csharp
public class OrderConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var msg = context.Message;
        await context.Publish(new OrderProcessed(msg.OrderId));
    }
}
```

## ConsumerDefinition — retry / outbox

```csharp
public class OrderConsumerDefinition : ConsumerDefinition<OrderConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator ep,
        IConsumerConfigurator<OrderConsumer> consumer,
        IRegistrationContext ctx)
    {
        ep.UseMessageRetry(r => r.Intervals(500, 1000, 2000));
        ep.UseInMemoryOutbox(ctx);
    }
}
```

## Saga State Machine

```csharp
public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Processing { get; private set; }
    public Event<OrderPlaced> OrderPlaced { get; private set; }

    public OrderStateMachine()
    {
        Initially(
            When(OrderPlaced)
                .TransitionTo(Processing));
        During(Processing,
            When(OrderShipped)
                .Finalize());
    }
}
```

## Test Harness

```csharp
services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<OrderConsumer>());

// In test:
var harness = provider.GetRequiredService<ITestHarness>();
await harness.Start();
await harness.Bus.Publish(new OrderPlaced(Guid.NewGuid()));
Assert.True(await harness.Consumed.Any<OrderPlaced>());
```

**Source:** https://masstransit.io/documentation/configuration
