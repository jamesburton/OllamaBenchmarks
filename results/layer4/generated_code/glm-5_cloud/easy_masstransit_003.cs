using System;
using System.Collections.Generic;

namespace Contracts
{
    public record PaymentProcessed(Guid PaymentId, decimal Amount, bool Success);
}

public interface IConsumer<in T>
{
    void Consume(T message);
}

public class PaymentProcessedConsumer : IConsumer<Contracts.PaymentProcessed>
{
    public List<string> ProcessedIds = new();

    public void Consume(Contracts.PaymentProcessed message)
    {
        ProcessedIds.Add(message.PaymentId.ToString());
    }
}