global using Contracts;
    using MassTransit;
    using MassTransit.Testing;
    using Microsoft.Extensions.DependencyInjection;

    namespace Contracts;

    // Records and Consumers

    // Tests outside namespace
    public class OrderPipelineTests { ... }

global using Contracts;
    using MassTransit;

    namespace Contracts;
    // ... types ...

    using MassTransit.Testing;
    using Microsoft.Extensions.DependencyInjection;

    public class OrderPipelineTests { ... }

public class OrderPipelineTests
    {
        private readonly ITestHarness _harness;

        public OrderPipelineTests(ITestHarness harness)
        {
            _harness = harness;
        }

        [Fact]
        public async Task Test1()
        {
            await _harness.Start();
            // ...
        }
    }

services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<OrderConsumer>());
    // In test:
    var harness = provider.GetRequiredService<ITestHarness>();

public class OrderPipelineTests
    {
        private readonly ITestHarness _harness;

        public OrderPipelineTests(ITestHarness harness)
        {
            _harness = harness;
        }
    }

services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<OrderConsumer>());
    // In test:
    var harness = provider.GetRequiredService<ITestHarness>();

public class OrderPipelineTests
    {
        private readonly ITestHarness _harness;

        public OrderPipelineTests(ITestHarness harness)
        {
            _harness = harness;
        }
    }

services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<OrderConsumer>());
    // In test:
    var harness = provider.GetRequiredService<ITestHarness>();

public class OrderPipelineTests
    {
        private readonly ITestHarness _harness;

        public OrderPipelineTests(ITestHarness harness)
        {
            _harness = harness;
        }
    }

var services = new ServiceCollection();
    services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<PlaceOrderConsumer>()
        .AddConsumer<NotifyCustomerConsumer>());
    var harness = services.BuildServiceProvider().GetRequiredService<ITestHarness>();

public class OrderPipelineTests
    {
        private readonly ITestHarness _harness;

        public OrderPipelineTests()
        {
            var services = new ServiceCollection();
            services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<PlaceOrderConsumer>()
                .AddConsumer<NotifyCustomerConsumer>());
            _harness = services.BuildServiceProvider().GetRequiredService<ITestHarness>();
        }
    }

global using Contracts;
    using MassTransit;
    namespace Contracts;

global using Contracts;
    using MassTransit;

    namespace Contracts;
    // ... types ...

    using MassTransit.Testing;
    using Microsoft.Extensions.DependencyInjection;

    public class OrderPipelineTests { ... }