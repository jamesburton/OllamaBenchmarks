using System;
using System.Collections.Generic;

namespace Contracts
{
    public record AlertTriggered(string AlertName, string Severity, string Message);
}

public interface IConsumer<T>
{
    void Consume(T message);
}

public class AlertTriggeredConsumer : IConsumer<Contracts.AlertTriggered>
{
    public List<string> CriticalAlerts = new();

    public void Consume(Contracts.AlertTriggered message)
    {
        if (message.Severity.Equals("critical", StringComparison.OrdinalIgnoreCase))
        {
            CriticalAlerts.Add(message.AlertName);
        }
    }
}