using Microsoft.EntityFrameworkCore;

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsSent { get; set; }
}

public class NotifContext : DbContext
{
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("NotifDatabase");
    }
}

public static class NotifRepository
{
    public static int DeleteSent(NotifContext context)
    {
        return context.Notifications
            .Where(n => n.IsSent)
            .ExecuteDelete();
    }
}