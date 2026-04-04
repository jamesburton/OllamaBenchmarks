using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace
{
    public class ProcessPaymentConsumer_ShouldProcessAndPublish
    {
        [Fact]
        public async Task ProcessPaymentConsumer_ShouldProcessAndPublish()
        {
            var provider = new ServiceCollection()
                .AddMassTransitTestHarness(() => cfg => cfg.AddConsumer<ProcessPaymentConsumer>())
                .BuildServiceProvider(true);

            ITestHarness harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();
            await harness.Publish(new ProcessPayment(Guid.NewGuid(), 1m));

            var consumedOk = await harness.Consumed.Any<ProcessPayment>();
            var publishedOk = await harness.Published.Any<PaymentProcessed>();
            Assert.Multiple(() => Assert.True(consumedOk), () => Assert.True(publishedOk));
        }
    }
}