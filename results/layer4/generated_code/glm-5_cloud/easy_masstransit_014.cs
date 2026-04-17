namespace Contracts
{
    public record TaskCompleted(Guid TaskId, string AssignedTo, TimeSpan Duration);
}

public interface IConsumer<T>
{
    void Consume(T message);
}

public class TaskCompletedConsumer : IConsumer<Contracts.TaskCompleted>
{
    public TimeSpan TotalDuration = TimeSpan.Zero;

    public void Consume(Contracts.TaskCompleted message)
    {
        TotalDuration += message.Duration;
    }
}