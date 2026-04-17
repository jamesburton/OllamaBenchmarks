using System;
using System.Threading.Tasks;

namespace Contracts
{
    public record UserRegistered(Guid UserId, string Email, DateTime RegisteredAt);
}

public interface IConsumer<T>
{
    Task Consume(T message);
}

public class UserRegisteredConsumer : IConsumer<Contracts.UserRegistered>
{
    public int ConsumedCount = 0;

    public Task Consume(Contracts.UserRegistered message)
    {
        ConsumedCount++;
        return Task.CompletedTask;
    }
}