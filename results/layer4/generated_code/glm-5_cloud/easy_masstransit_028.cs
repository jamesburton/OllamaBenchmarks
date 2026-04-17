using System;
using System.Collections.Generic;

namespace Contracts
{
    public record LoginAttempt(string UserId, bool Success, string IpAddress);
}

public interface IConsumer<T>
{
    void Consume(T message);
}

public class LoginAttemptConsumer : IConsumer<Contracts.LoginAttempt>
{
    public Dictionary<string, int> FailedAttemptsByIp = new();

    public void Consume(Contracts.LoginAttempt message)
    {
        if (!message.Success)
        {
            if (FailedAttemptsByIp.ContainsKey(message.IpAddress))
            {
                FailedAttemptsByIp[message.IpAddress]++;
            }
            else
            {
                FailedAttemptsByIp[message.IpAddress] = 1;
            }
        }
    }
}