using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MassTransit;
using MassTransit.Saga;

namespace MassTransitTestHarness
{
    public class Program
    {
        private static readonly ILogger<Program> _logger = new Logger<Program>();

        public static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);

            builder.Services.AddMassTransit(x =>
            {
                x.UseInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
            });

            // Use IBusRegistrationConfigurator (NOT IServiceCollectionBusConfigurator)
            builder.Services.AddTransient<ITestHarness>();

            _logger.LogInformation("Starting MassTransit test harness...");

            var harness = builder.Build();
            await harness.Start();

            _logger.LogInformation("Press any key to stop the test harness...");
            Console.ReadKey();
        }
    }

    public class TestHarness : ITestHarness
    {
        private readonly IServiceProvider _serviceProvider;

        public TestHarness(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Start()
        {
            var context = await _serviceProvider.GetRequiredService<ITestHarnessContext>();
            await context.Start();
        }
    }
}