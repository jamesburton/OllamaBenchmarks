using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

public class LogEntry
{
    public int Id { get; set; }
    public string Level { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
}

public class LogContext : DbContext
{
    public DbSet<LogEntry> LogEntries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("LogDatabase");
    }
}

public static class LogRepository
{
    public static List<LogEntry> GetByLevel(LogContext context, string level, int limit)
    {
        return context.LogEntries
            .Where(l => l.Level == level)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToList();
    }
}