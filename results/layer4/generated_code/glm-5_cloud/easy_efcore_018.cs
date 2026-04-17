using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public int Priority { get; set; }
}

public class TicketContext : DbContext
{
    public DbSet<Ticket> Tickets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("TicketDatabase");
    }
}

public static class TicketRepository
{
    public static List<Ticket> GetByStatus(TicketContext context, string status)
    {
        return context.Tickets
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.Priority)
            .ToList();
    }
}