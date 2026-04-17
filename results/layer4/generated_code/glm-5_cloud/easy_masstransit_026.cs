using System;
using System.Collections.Generic;

namespace Contracts
{
    public record AuditEvent(Guid EventId, string UserId, string Action, DateTime OccurredAt);
}

public interface IConsumer<T>
{
    void Consume(T message);
}

public class AuditEventConsumer : IConsumer<Contracts.AuditEvent>
{
    public Queue<Contracts.AuditEvent> EventQueue = new();

    public void Consume(Contracts.AuditEvent message)
    {
        EventQueue.Enqueue(message);

        if (EventQueue.Count > 100)
        {
            EventQueue.Dequeue();
        }
    }
}