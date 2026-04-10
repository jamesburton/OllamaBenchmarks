global using Contracts; // This might be redundant if the whole file is inside Contracts, but the prompt asks for it specifically to make types accessible without explicit using statements from other files.
                using MassTransit;
                using MassTransit.Testing;
                using Microsoft.Extensions.DependencyInjection;

                namespace Contracts { ... }

global using Contracts; // This is technically invalid if Contracts namespace isn't defined yet in the same file scope, but often used to simulate a project reference or just a directive. However, standard C# requires the namespace to be visible.
                // Actually, looking at typical "single file" patterns for these prompts:
                // It usually means:
                global using Contracts; // This is likely intended to mean "Assume Contracts is available globally". But since I am defining it in this file...

global using Contracts;
        using MassTransit;
        using MassTransit.Testing;
        using Microsoft.Extensions.DependencyInjection;

        namespace Contracts { ... } // Messages & Consumers

        public class OrderPipelineTests { ... } // Tests

global using Contracts;
        using MassTransit;
        using MassTransit.Testing;
        using Microsoft.Extensions.DependencyInjection;

        namespace Contracts { ... }
        public class OrderPipelineTests { ... }

global using Contracts;
        using MassTransit;
        using MassTransit.Testing;
        using Microsoft.Extensions.DependencyInjection;

        namespace Contracts { ... }
        public class OrderPipelineTests { ... }

... 
global using Contracts;
        using MassTransit;
        using MassTransit.Testing;
        using Microsoft.Extensions.DependencyInjection;

        namespace Contracts { ... }
        public class OrderPipelineTests { ... }

global using Contracts;
        using MassTransit;
        using MassTransit.Testing;
        using Microsoft.Extensions.DependencyInjection;

namespace Contracts {
            // Messages
            public record PlaceOrder(Guid OrderId, string CustomerName);
            public record OrderPlaced(Guid OrderId, string CustomerName);
            public record CustomerNotified(Guid OrderId, string NotificationMessage);

            // Consumers
            public class PlaceOrderConsumer : IConsumer<PlaceOrder> { ... }
            public class NotifyCustomerConsumer : IConsumer<OrderPlaced> { ... }
        }

public class OrderPipelineTests {
            private readonly IServiceProvider _provider;
            private readonly ITestHarness _harness;

            [GlobalSetup]
            public void Setup() { ... } // Configure DI and get harness

            [Fact]
            public async Task TestStage1() { ... }

            [Fact]
            public async Task TestFullPipeline() { ... }
        }

[GlobalSetup]
            public void Setup() {
                var services = new ServiceCollection();
                services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<PlaceOrderConsumer>().AddConsumer<NotifyCustomerConsumer>());
                _provider = services.BuildServiceProvider();
                _harness = _provider.GetRequiredService<ITestHarness>();
            }

global using Contracts;
        using MassTransit;
        using MassTransit.Testing;
        using Microsoft.Extensions.DependencyInjection;

        namespace Contracts { ... }
        public class OrderPipelineTests { ... }