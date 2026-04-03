public class ProcessPaymentConsumerTests
        {
            [Fact]
            public async Task Consume_ShouldPublishPaymentProcessed()
            {
                var provider = new ServiceCollection()
                    .AddMassTransitTestHarness(cfg =>
                    {
                        cfg.AddConsumer<ProcessPaymentConsumer>();
                    })
                    .BuildServiceProvider(true);

                var harness = provider.GetRequiredService<ITestHarness>();
                await harness.Start();

                var orderId = Guid.NewGuid();
                var amount = 100m;

                await harness.Bus.Publish(new ProcessPayment(orderId, amount));

                // 1. The consumer receives and processes the ProcessPayment message
                bool consumed = await harness.Consumed.Any<ProcessPayment>();

                // 2. A PaymentProcessed event is published
                bool published = await harness.Published.Any<PaymentProcessed>();

                // 2. (continued) with the correct OrderId
                bool correctOrderId = await harness.Published.Select<PaymentProcessed>()
                    .Any(x => x.Context.Message.OrderId == orderId);

                // 3. Use Assert.Multiple to check both assertions
                Assert.Multiple(
                    () => Assert.True(consumed, "Message was not consumed"),
                    () => Assert.True(published, "Message was not published"),
                    () => Assert.True(correctOrderId, "Published message had incorrect OrderId")
                );
            }
        }

bool wasConsumed = await harness.Consumed.Any<ProcessPayment>();
    bool wasPublishedWithCorrectId = await harness.Published.Select<PaymentProcessed>()
        .Any(x => x.Context.Message.OrderId == orderId);

    Assert.Multiple(
        () => Assert.True(wasConsumed, "The ProcessPayment message was not consumed."),
        () => Assert.True(wasPublishedWithCorrectId, "The PaymentProcessed event was not published with the correct OrderId.")
    );

bool wasConsumed = await harness.Consumed.Any<ProcessPayment>();
    bool wasPublished = await harness.Published.Any<PaymentProcessed>();
    bool wasPublishedWithCorrectId = await harness.Published.Select<PaymentProcessed>()
        .Any(x => x.Context.Message.OrderId == orderId);

    Assert.Multiple(
        () => Assert.True(wasConsumed),
        () => Assert.True(wasPublished),
        () => Assert.True(wasPublishedWithCorrectId)
    );