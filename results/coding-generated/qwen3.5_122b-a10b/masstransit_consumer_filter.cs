public class ConsumerDefinition<TConsumer> : ConsumerDefinition
            {
                public ConsumerDefinition()
                {
                    // ...
                }
            }

using MassTransit;
    global using Contracts;

    namespace Contracts;

    public record ProcessPayment(...)
    public record PaymentProcessed(...)
    public class ProcessPaymentConsumer : IConsumer<ProcessPayment> { ... }
    public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer> { ... }

using MassTransit;
    global using Contracts;

    namespace Contracts;

public ProcessPaymentConsumerDefinition()
    {
        ConcurrentMessageLimit = 2;
    }

protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator ep,
        IConsumerConfigurator<OrderConsumer> consumer,
        IRegistrationContext ctx)

public async Task Consume(ConsumeContext<OrderPlaced> context)

using MassTransit;
    global using Contracts;

    namespace Contracts;

    // types

using MassTransit;
    global using Contracts;

    namespace Contracts;

public ProcessPaymentConsumerDefinition()
    {
        ConcurrentMessageLimit = 2;
    }

using MassTransit;
    global using Contracts;

    namespace Contracts;

public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
    {
        public async Task Consume(ConsumeContext<ProcessPayment> context)
        {
            if (context.Message.Amount <= 0)
                return;

            await context.Publish(new PaymentProcessed(context.Message.OrderId, DateTime.UtcNow));
        }
    }

public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
    {
        public ProcessPaymentConsumerDefinition()
        {
            ConcurrentMessageLimit = 2;
        }

        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator ep,
            IConsumerConfigurator<ProcessPaymentConsumer> consumer,
            IRegistrationContext ctx)
        {
            ep.UseMessageRetry(r => r.Interval(2, 500));
        }
    }

using MassTransit;
    global using Contracts;

    namespace Contracts;