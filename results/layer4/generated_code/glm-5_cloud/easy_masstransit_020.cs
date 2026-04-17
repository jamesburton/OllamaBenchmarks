namespace Contracts
{
    public record CacheInvalidated(string CacheKey, string Reason);
}

using MassTransit;

public class CacheInvalidatedConsumer : IConsumer<Contracts.CacheInvalidated>
{
    public HashSet<string> InvalidatedKeys = new();

    public Task Consume(ConsumeContext<Contracts.CacheInvalidated> context)
    {
        InvalidatedKeys.Add(context.Message.CacheKey);
        return Task.CompletedTask;
    }
}

public class CacheInvalidatedConsumerDefinition : ConsumerDefinition<CacheInvalidatedConsumer>
{
    public CacheInvalidatedConsumerDefinition()
    {
        ConcurrentMessageLimit = 5;
    }
}