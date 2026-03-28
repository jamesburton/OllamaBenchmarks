using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace MassTransitTests
{
    public class ProcessPaymentConsumerTest : IAsyncLifetime
    {
        private readonly TestContext _context = new TestContext();
        private readonly ServiceCollection _services = new ServiceCollection();

        public async Task InitializeAsync()
        {
            var serviceProvider = _services.BuildServiceProvider(true);
            await _context.Start(serviceProvider);
        }

        public async Task DisposeAsync()
        {
            await _context.Stop(_services);
        }

        [Fact]
        public async Task ProcessPayment()
        {
            var context = new TestContext();
            var serviceProvider = _services.BuildServiceProvider(true);

            var consumer = serviceProvider.GetRequiredService<ProcessPaymentConsumer>();
            var message = new ProcessPayment(Guid.NewGuid(), 10.99m);
            await context.Publish(message);

            var result = await context.Consumed.Any<PaymentProcessed>();

            Assert.True(result);
        }
    }

    public class TestContext : IAsyncLifetime
    {
        private readonly TestContext _context;
        private readonly ServiceCollection _services;

        public TestContext()
        {
            _context = new TestContext();
            _services = new ServiceCollection();

            var serviceProvider = _services.BuildServiceProvider(true);
            await _context.Start(serviceProvider);
        }

        public async Task DisposeAsync()
        {
            await _context.Stop(_services);
        }
    }
}