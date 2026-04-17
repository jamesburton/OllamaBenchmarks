using System;
using System.Collections.Generic;

namespace Contracts
{
    public record HealthCheckFailed(string ServiceName, string Endpoint, int StatusCode);
}

public interface IConsumer<T>
{
    void Consume(T message);
}

public class HealthCheckFailedConsumer : IConsumer<Contracts.HealthCheckFailed>
{
    public List<string> FailedServices = new();

    public void Consume(Contracts.HealthCheckFailed message)
    {
        if (message.StatusCode >= 500)
        {
            FailedServices.Add(message.ServiceName);
        }
    }
}