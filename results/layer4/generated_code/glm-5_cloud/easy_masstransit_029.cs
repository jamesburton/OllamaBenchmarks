using System.Collections.Generic;
using MassTransit;

namespace Contracts
{
    public record ConfigurationChanged(string Key, string OldValue, string NewValue, string ChangedBy);
}

public class ConfigurationChangedConsumer : IConsumer<Contracts.ConfigurationChanged>
{
    public List<string> ChangedKeys = new();

    public Task Consume(ConsumeContext<Contracts.ConfigurationChanged> context)
    {
        if (context.Message.OldValue != context.Message.NewValue)
        {
            ChangedKeys.Add(context.Message.Key);
        }

        return Task.CompletedTask;
    }
}