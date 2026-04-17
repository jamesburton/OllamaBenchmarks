using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

public class Subscription
{
    public int Id { get; set; }
    public string PlanName { get; set; }
    public decimal Price { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class SubContext : DbContext
{
    public DbSet<Subscription> Subscriptions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("SubscriptionsDb");
    }
}

public static class SubscriptionRepository
{
    public static List<Subscription> GetExpiringSoon(SubContext context, int daysAhead)
    {
        var now = DateTime.UtcNow;
        var threshold = now.AddDays(daysAhead);

        return context.Subscriptions
            .Where(s => s.ExpiresAt >= now && s.ExpiresAt <= threshold)
            .OrderBy(s => s.ExpiresAt)
            .ToList();
    }
}