using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<ProcessPaymentConsumer>());
var harness = services.BuildServiceProvider(true).GetRequiredService<ITestHarness>();

await harness.Start();

var message = new ProcessPayment(Guid.NewGuid(), 100.0m);
await harness.Bus.Publish(message);

var received = await harness.Consumed.Any<ProcessPayment>();
var published = await harness.Published.Any<PaymentProcessed>();

Assert.Multiple(
    () => Assert.True(received),
    () => Assert.True(published)
);