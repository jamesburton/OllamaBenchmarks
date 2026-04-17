namespace Contracts
{
    public record CustomerDeleted(Guid CustomerId, string Reason);
}

public class CustomerDeletedConsumer : IConsumer<Contracts.CustomerDeleted>
{
    public HashSet<Guid> DeletedIds = new();

    public void Consume(Contracts.CustomerDeleted message)
    {
        DeletedIds.Add(message.CustomerId);
    }
}

// Interface definition required for the code to be valid
public interface IConsumer<in T>
{
    void Consume(T message);
}