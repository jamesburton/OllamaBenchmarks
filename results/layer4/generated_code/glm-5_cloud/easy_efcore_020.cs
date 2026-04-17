using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class Session
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}

public class SessionContext : DbContext
{
    public DbSet<Session> Sessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("SessionDatabase");
    }
}

public static class SessionRepository
{
    public static List<Session> GetActiveSessions(SessionContext context)
    {
        return context.Sessions
            .Where(s => s.EndedAt == null)
            .ToList();
    }
}