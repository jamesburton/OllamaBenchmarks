using System;

namespace Contracts
{
    public record BackupCompleted(Guid BackupId, string BackupType, long SizeBytes, bool Success);
}

public interface IConsumer<T>
{
    void Consume(T message);
}

public class BackupCompletedConsumer : IConsumer<Contracts.BackupCompleted>
{
    public int SuccessCount = 0;
    public int FailureCount = 0;

    public void Consume(Contracts.BackupCompleted message)
    {
        if (message.Success)
        {
            SuccessCount++;
        }
        else
        {
            FailureCount++;
        }
    }
}