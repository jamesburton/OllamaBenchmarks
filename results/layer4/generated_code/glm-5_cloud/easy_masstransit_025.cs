namespace Contracts
{
    public record ContentPublished(Guid ContentId, string Title, string[] Tags);
}

public interface IConsumer<T>
{
    Task Consume(T message);
}

public class ContentPublishedConsumer : IConsumer<Contracts.ContentPublished>
{
    public HashSet<string> AllTags = new();

    public Task Consume(Contracts.ContentPublished message)
    {
        if (message.Tags != null)
        {
            foreach (var tag in message.Tags)
            {
                AllTags.Add(tag);
            }
        }
        return Task.CompletedTask;
    }
}