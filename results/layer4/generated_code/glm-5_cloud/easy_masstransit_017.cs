using System;
using System.Collections.Generic;

namespace Contracts
{
    public record DeploymentStarted(Guid DeploymentId, string Environment, string Version);
}

public interface IConsumer<T>
{
    void Consume(T message);
}

public class DeploymentStartedConsumer : IConsumer<Contracts.DeploymentStarted>
{
    public List<string> ActiveEnvironments = new();

    public void Consume(Contracts.DeploymentStarted message)
    {
        if (!ActiveEnvironments.Contains(message.Environment))
        {
            ActiveEnvironments.Add(message.Environment);
        }
    }
}