using System;
using System.Collections.Generic;
using System.Linq;

public record Message(string From, string To, string Body, DateTime SentAt);

public class Inbox
{
    private readonly List<Message> _messages = new();

    public void Receive(Message message)
    {
        _messages.Add(message);
    }

    public List<Message> GetFrom(string sender)
    {
        return _messages.Where(m => m.From == sender).ToList();
    }

    public List<Message> GetUnread()
    {
        return _messages.OrderByDescending(m => m.SentAt).Take(5).ToList();
    }

    public int Count => _messages.Count;
}