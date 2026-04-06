public class ConsumerDefinition<T> : IConsumerDefinition<T> where T : class
            {
                protected ConsumerDefinition() { }
                public int ConcurrentMessageLimit { get; set; }
                // ...
            }

protected SubmitOrderConsumerDefinition()
            {
                ConcurrentMessageLimit = 4;
            }

protected override void ConfigureConsumer(
                IReceiveEndpointConfigurator ep,
                IConsumerConfigurator<T> consumer,
                IRegistrationContext ctx)
            {
                ep.UseMessageRetry(r => r.Interval(3, 1000));
            }

public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
            {
                protected SubmitOrderConsumerDefinition()
                {
                    ConcurrentMessageLimit = 4;
                }

                protected override void ConfigureConsumer(...)
                {
                    ep.UseMessageRetry(r => r.Interval(3, 1000));
                }
            }

public class ConsumerDefinition<T> : IConsumerDefinition<T>
            {
                protected ConsumerDefinition() { }
                // ...
            }

protected SubmitOrderConsumerDefinition()
            {
                ConcurrentMessageLimit = 4;
            }

public class OrderConsumerDefinition : ConsumerDefinition<OrderConsumer>
        {
            protected override void ConfigureConsumer(...)
            {
                // ...
            }
        }

using MassTransit;
        global using Contracts;
        namespace Contracts;

using MassTransit;
        global using Contracts;
        namespace Contracts;

using MassTransit;
        global using Contracts;
        namespace Contracts;

using MassTransit;
        global using Contracts;
        namespace Contracts;

using MassTransit;
        global using Contracts;
        namespace Contracts;