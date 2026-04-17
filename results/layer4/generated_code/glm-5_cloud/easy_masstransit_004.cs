using System.Collections.Generic;
using MassTransit;

namespace Contracts
{
    public record EmailNotification(string To, string Subject, string Body);
}

public class EmailNotificationConsumer : IConsumer<Contracts.EmailNotification>
{
    public List<string> SentTo = new();

    public Task Consume(ConsumeContext<Contracts.EmailNotification> context)
    {
        SentTo.Add(context.Message.To);
        return Task.CompletedTask;
    }
}