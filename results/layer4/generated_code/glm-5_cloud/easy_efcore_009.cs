using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public bool IsCancelled { get; set; }
}

public class EventContext : DbContext
{
    public DbSet<Event> Events { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("EventDatabase");
    }
}

public static class EventRepository
{
    public static List<Event> GetUpcoming(EventContext context, DateTime from)
    {
        return context.Events
            .Where(e => !e.IsCancelled && e.StartDate >= from)
            .OrderBy(e => e.StartDate)
            .ToList();
    }
}