public record ProcessPayment(Guid OrderId, decimal Amount);
public record PaymentProcessed(Guid OrderId, string TransactionId);
public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        await context.Publish(new PaymentProcessed(context.Message.OrderId, Guid.NewGuid().ToString()));
    }
}

public record ProcessPayment(Guid OrderId, decimal Amount);
public record PaymentProcessed(Guid OrderId, string TransactionId);
public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        await context.Publish(new PaymentProcessed(context.Message.OrderId, Guid.NewGuid().ToString()));
    }
}

public record ProcessPayment(Guid OrderId, decimal Amount);
public record PaymentProcessed(Guid OrderId, string TransactionId);
public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        await context.Publish(new PaymentProcessed(context.Message.OrderId, Guid.NewGuid().ToString()));
    }
}

public record ProcessPayment(Guid OrderId, decimal Amount);
public record PaymentProcessed(Guid OrderId, string TransactionId);
public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        await context.Publish(new PaymentProcessed(context.Message.OrderId, Guid.NewGuid().ToString()));
    }
}

public record ProcessPayment(Guid OrderId, decimal Amount);
public record PaymentProcessed(Guid OrderId, string TransactionId);
public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        await context.Publish(new PaymentProcessed(context.Message.OrderId, Guid.NewGuid().ToString()));
    }
}

public record ProcessPayment(Guid OrderId, decimal Amount);
public record PaymentProcessed(Guid OrderId, string TransactionId);
public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        await context.Publish(new PaymentProcessed(context.Message.OrderId, Guid.NewGuid().ToString()));
    }
}

public record ProcessPayment(Guid OrderId, decimal Amount);
public record PaymentProcessed(Guid OrderId, string TransactionId);
public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        await context.Publish(new PaymentProcessed(context.Message.OrderId, Guid.NewGuid().ToString()));
    }
}

public record ProcessPayment(Guid OrderId, decimal Amount);
public record PaymentProcessed(Guid OrderId, string TransactionId);
public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        await context.Publish(new PaymentProcessed(context.Message.OrderId, Guid.NewGuid().ToString()));
    }
}

public record ProcessPayment(Guid OrderId, decimal Amount);
public record PaymentProcessed(Guid OrderId, string TransactionId);
public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        await context.Publish(new PaymentProcessed(context.Message.OrderId, Guid.NewGuid().ToString()));
    }
}

public record ProcessPayment(Guid OrderId, decimal Amount);
public record PaymentProcessed(Guid OrderId, string TransactionId);
public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        await context.Publish(new PaymentProcessed(context.Message.OrderId, Guid.NewGuid().ToString()));
    }
}

public record ProcessPayment(Guid OrderId, decimal Amount);
public record PaymentProcessed(Guid OrderId, string TransactionId);
public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        await context.Publish(new PaymentProcessed(context.Message.OrderId, Guid.NewGuid().ToString()));
    }
}