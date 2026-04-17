namespace Contracts
{
    public record UserStatusChanged(Guid UserId, string OldStatus, string NewStatus, DateTime ChangedAt);
}

public interface IConsumer<in T>
{
    Task Consume(T message);
}

public class UserStatusChangedConsumer : IConsumer<Contracts.UserStatusChanged>
{
    public List<string> SuspendedUsers = new();

    public Task Consume(Contracts.UserStatusChanged message)
    {
        if (message.NewStatus == "suspended")
        {
            SuspendedUsers.Add(message.UserId.ToString());
        }

        return Task.CompletedTask;
    }
}

public abstract class ConsumerDefinition<TConsumer> where TConsumer : class { }

public class UserStatusChangedConsumerDefinition : ConsumerDefinition<UserStatusChangedConsumer>
{
    public UserStatusChangedConsumerDefinition()
    {
        ConcurrentMessageLimit = 10;
    }

    public int ConcurrentMessageLimit { get; set; }
}