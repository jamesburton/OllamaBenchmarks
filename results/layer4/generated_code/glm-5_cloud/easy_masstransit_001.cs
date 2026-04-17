using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts
{
    public record OrderCreated(Guid OrderId, string CustomerName, decimal Total);
}

public interface IConsumer<T>
{
    Task Consume(T message);
}

public class OrderCreatedConsumer : IConsumer<Contracts.OrderCreated>
{
    public List<Contracts.OrderCreated> Received = new();

    public Task Consume(Contracts.OrderCreated message)
    {
        Received.Add(message);
        return Task.CompletedTask;
    }
}