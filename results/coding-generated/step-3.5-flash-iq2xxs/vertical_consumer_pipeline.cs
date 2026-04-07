global using Contracts;

namespace Contracts {
    // ... definitions
}

// Then the test class (without a namespace) can use `PlaceOrder` directly because the global using brings in the `Contracts` namespace? Actually, the global using is like having `using Contracts;` at the top of the file. So in the test class, we can write `PlaceOrder` and it will be resolved to `Contracts.PlaceOrder` because of the global using. But note: the test class is in the global namespace, and the `PlaceOrder` type is in the `Contracts` namespace. So we need to have a `using Contracts;` or the global using will cover it? Actually, the global using is a feature that allows you to import namespaces for the entire file. So if we write:

But wait, the requirement says: "Add `global using Contracts;` at the very top of the file so the types are accessible without explicit using statements from other files." This implies that if we split into multiple files, the test file would have `global using Contracts;` to access the types defined in the Contracts file. But in our single file, we are defining everything. So we can structure:

We'll have:


var harness = new TestHarnessBuilder()
    .AddTestHandler<PlaceOrderConsumer>()
    .AddTestHandler<NotifyCustomerConsumer>()
    .Build();

services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<OrderConsumer>());

var harness = new TestHarnessBuilder()
    .AddTestHandler<PlaceOrderConsumer>()
    .AddTestHandler<NotifyCustomerConsumer>()
    .Build();

var harness = new TestHarnessBuilder()
    .AddTestHandler<PlaceOrderConsumer>()
    .AddTestHandler<NotifyCustomerConsumer>()
    .Build();