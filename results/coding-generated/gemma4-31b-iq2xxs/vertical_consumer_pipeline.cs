*   API Reference: MassTransit v8 and xUnit v3.
    *   Goal: Create a two-stage consumer pipeline.
    *   Messages: `PlaceOrder`, `OrderPlaced`, `CustomerNotified` (all positional records).
    *   Consumers: `PlaceOrderConsumer` (PlaceOrder -> OrderPlaced) and `NotifyCustomerConsumer` (OrderPlaced -> CustomerNotified).
    *   Tests: xUnit v3 tests verifying the pipeline.
    *   Constraints:
        *   `global using Contracts;` at the top.
        *   `using MassTransit;` before the namespace.
        *   `namespace Contracts;` for messages and consumers.
        *   `AddMassTransitTestHarness` for testing.
        *   `harness.Consumed.Any<T>()` and `harness.Published.Any<T>()`.
        *   No `Xunit.sAbstractions`.

    *   `PlaceOrder`: `Guid OrderId`, `string CustomerName`.
    *   `OrderPlaced`: `Guid OrderId`, `string CustomerName`.
    *   `CustomerNotified`: `Guid OrderId`, `string NotificationMessage`.

    *   `PlaceOrderConsumer`:
        *   `IConsumer<PlaceOrder>`
        *   `Consume` method: `await context.Publish(new OrdersPlaced(context.Message.OrderId, context.Message.CustomerName))` (Wait, the prompt says "publishes an `OrderPlaced` event").

    *   `NotifyCustomerConsumer`:
        *   `IConsumer<OrderPlaced>`
        *   `Consume` method: `await context.Publish(new CustomerNotified(context.Message.OrderId, $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"))`.

    *   `AddMassTransitTestHarness` configuration.
    *   Test 1: `PlaceOrder` -> `OrderPlaced`.
    *   Test 2: Full pipeline.

    *   `global using Contracts;`
    *   `using MassTransit;`
    *   `namespace Contracts;`
    *   `public record PlaceOrder(...)`
    *   `public record OrderPlaced(...)`
    *   `public record CustomerNotified(...)`
    *   `public class PlaceOrderConsumer : IConsumer<PlaceOrder> { ... }`
    *   `