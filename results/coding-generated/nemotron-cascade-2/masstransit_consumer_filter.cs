using MassTransit;

global using Contracts;

namespace Contracts;

public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        var msg = context.Message;
        if (msg.Amount <= 0)
            return;

        await context.Publish(new PaymentProcessed(msg.OrderId, DateTime.UtcNow));
    }
}

public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
{
    public ProcessPaymentConsumerDefinition()
    {
        ConcurrentMessageLimit = 2;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ProcessPaymentConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Interval(2, TimeSpan.FromMilliseconds(500)));
    }
}

using MassTransit;

global using Contracts;

namespace Contracts;

using System;
using MassTransit;

global using Contracts;

namespace Contracts;

public record ProcessPayment(Guid OrderId, decimal Amount, string Currency);

public record PaymentProcessed(Guid OrderId, DateTime ProcessedAt);

public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        var msg = context.Message;
        if (msg.Amount <= 0)
            return;

        await context.Publish(new PaymentProcessed(msg.OrderId, DateTime.UtcNow));
    }
}

public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
{
    public ProcessPaymentConsumerDefinition()
    {
        ConcurrentMessageLimit = 2;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ProcessPaymentConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Interval(2, 500));
    }
}

endpointConfigurator.UseMessageRetry(r => r.Interval(2, 500));

using System;
using MassTransit;

global using Contracts;

namespace Contracts;
{
    public record ProcessPayment(Guid OrderId, decimal Amount, string Currency);

    public record PaymentProcessed(Guid OrderId, DateTime ProcessedAt);

    public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
    {
        public async Task Consume(ConsumeContext<ProcessPayment> context)
        {
            var msg = context.Message;
            if (msg.Amount <= 0)
                return;

            await context.Publish(new PaymentProcessed(msg.OrderId, DateTime.UtcNow));
        }
    }

    public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
    {
        public ProcessPaymentConsumerDefinition()
        {
            ConcurrentMessageLimit = 2;
        }

        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<ProcessPaymentConsumer> consumerConfigurator,
            IRegistrationContext context)
        {
            endpointConfigurator.UseMessageRetry(r => r.Interval(2, 500));
        }
    }
}

using System;
using MassTransit;

global using Contracts;

namespace Contracts;
public record ProcessPayment(Guid OrderId, decimal Amount, string Currency);

public record PaymentProcessed(Guid OrderId, DateTime ProcessedAt);

public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        var msg = context.Message;
        if (msg.Amount <= 0)
            return;

        await context.Publish(new PaymentProcessed(msg.OrderId, DateTime.UtcNow));
    }
}

public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>
{
    public ProcessPaymentConsumerDefinition()
    {
        ConcurrentMessageLimit = 2;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ProcessPaymentConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Interval(2, 500));
    }
}