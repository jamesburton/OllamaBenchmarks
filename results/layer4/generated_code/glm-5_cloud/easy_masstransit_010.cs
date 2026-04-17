using System;
using System.Collections.Generic;

namespace Contracts
{
    public record PasswordResetRequested(string UserId, string Token, DateTime ExpiresAt);
}

public class PasswordResetConsumer : IConsumer<Contracts.PasswordResetRequested>
{
    public List<string> TokensSent = new();

    public void Consume(Contracts.PasswordResetRequested message)
    {
        if (message.ExpiresAt > DateTime.UtcNow)
        {
            TokensSent.Add(message.Token);
        }
    }
}

// Interface definition required for the code to be valid
public interface IConsumer<T>
{
    void Consume(T message);
}