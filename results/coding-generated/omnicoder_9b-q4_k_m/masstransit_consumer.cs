public class ConsumerDefinition<T> : IConsumerDefinition<T> where T : class
            {
                public ConsumerDefinition() { }
                public int ConcurrentMessageLimit { get; set; }
                // ...
            }

protected override void ConfigureConsumer(
                IReceiveEndpointConfigurator ep,
                IConsumerConfigurator<T> consumer,
                IRegistrationContext ctx)
            {
                ep.UseMessageRetry(r => r.Interval(3, 1000));
            }

using MassTransit;
        global using Contracts;
        namespace Contracts;

public SubmitOrderConsumerDefinition()
        {
            ConcurrentMessageLimit = 4;
        }

using MassTransit;
        global using Contracts;
        namespace Contracts;

using System;
        using MassTransit;
        global using Contracts;
        namespace Contracts;

using MassTransit;
        global using Contracts;
        namespace Contracts;

using System;
        using MassTransit;
        global using Contracts;
        namespace Contracts;