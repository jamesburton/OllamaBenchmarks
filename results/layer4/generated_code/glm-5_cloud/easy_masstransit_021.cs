using System;
using System.Collections.Generic;

namespace Contracts
{
    public record ChatMessage(Guid MessageId, string RoomId, string SenderName, string Content);
}

public interface IConsumer<T>
{
    void Consume(T message);
}

public class ChatMessageConsumer : IConsumer<Contracts.ChatMessage>
{
    public Dictionary<string, int> MessageCountByRoom = new();

    public void Consume(Contracts.ChatMessage message)
    {
        if (message == null) return;

        if (MessageCountByRoom.ContainsKey(message.RoomId))
        {
            MessageCountByRoom[message.RoomId]++;
        }
        else
        {
            MessageCountByRoom[message.RoomId] = 1;
        }
    }
}