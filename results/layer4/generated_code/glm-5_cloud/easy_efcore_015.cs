using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}

public class MessageContext : DbContext
{
    public DbSet<Message> Messages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("MessageDatabase");
    }
}

public static class MessageRepository
{
    public static int MarkAllAsRead(MessageContext context)
    {
        return context.Messages
            .Where(m => !m.IsRead)
            .ExecuteUpdate(setters => setters.SetProperty(m => m.IsRead, true));
    }
}