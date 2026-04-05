using MassTransit;
        using MassTransit.Testing;
        using Microsoft.Extensions.DependencyInjection;

        public class ProcessPaymentConsumerTests
        {
            [Fact]
            public async Task Should_Consume_ProcessPayment_And_Publish_PaymentProcessed()
            {
                // Arrange
                var provider = new ServiceCollection()
                    .AddMassTransitTestHarness(cfg =>
                    {
                        cfg.AddConsumer<ProcessPaymentConsumer>();
                    })
                    .BuildServiceProvider(true);

                var harness = provider.GetRequiredService<ITestHarness>();
                await harness.Start();

                var orderId = Guid.NewGuid();
                var amount = 100.50m;

                // Act
                await harness.Bus.Publish(new ProcessPayment(orderId, amount));

                // Assert
                var consumed = await harness.Consumed.Any<ProcessPayment>();
                var published = await harness.Published.Any<PaymentProcessed>(x => x.Message.OrderId == orderId);

                Assert.Multiple(() =>
                {
                    Assert.True(consumed);
                    Assert.True(published);
                });
            }
        }

var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();
        // We need to verify the OrderId.
        // We can get the message context synchronously now.
        var publishedMessage = harness.Published.Select<PaymentProcessed>().FirstOrDefault()?.Context.Message;

        Assert.Multiple(
            () => Assert.True(consumed),
            () => Assert.True(published),
            () => Assert.NotNull(publishedMessage),
            () => Assert.Equal(orderId, publishedMessage.OrderId)
        );

var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();
        // We can't easily check OrderId with just Any() unless we use Select.
        // But the prompt says "Use ... Any()".
        // It doesn't say "Don't use Select".
        // But checking "correct OrderId" is a requirement.