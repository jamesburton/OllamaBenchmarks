using System;
using System.Collections.Generic;
using MassTransit;

namespace Contracts
{
    public record ItemExpired(string ItemId, string ItemType, DateTime ExpiredAt);
}

public class ItemExpiredConsumer : IConsumer<Contracts.ItemExpired>
{
    public Dictionary<string, List<string>> ExpiredByType = new();

    public Task Consume(ConsumeContext<Contracts.ItemExpired> context)
    {
        var message = context.Message;

        if (!ExpiredByType.ContainsKey(message.ItemType))
        {
            ExpiredByType[message.ItemType] = new List<string>();
        }

        ExpiredByType[message.ItemType].Add(message.ItemId);

        return Task.CompletedTask;
    }
}