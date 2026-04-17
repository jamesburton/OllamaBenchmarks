using System;
using System.Collections.Generic;

namespace Contracts
{
    public record OrderShipped(Guid OrderId, string TrackingNumber, DateTime ShippedAt);
}

public interface IConsumer<T>
{
    Task Consume(T message);
}

public class OrderShippedConsumer : IConsumer<Contracts.OrderShipped>
{
    public Dictionary<Guid, string> TrackingNumbers = new();

    public async Task Consume(Contracts.OrderShipped message)
    {
        TrackingNumbers[message.OrderId] = message.TrackingNumber;
        await Task.CompletedTask;
    }
}