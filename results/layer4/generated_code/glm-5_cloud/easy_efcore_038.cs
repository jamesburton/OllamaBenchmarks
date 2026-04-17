using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

public class PageView
{
    public int Id { get; set; }
    public string Path { get; set; }
    public string Country { get; set; }
    public DateTime ViewedAt { get; set; }
}

public class AnalyticsContext : DbContext
{
    public DbSet<PageView> PageViews { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("AnalyticsDatabase");
    }
}

public static class AnalyticsRepository
{
    public static List<(string Path, int Count)> GetTopPages(AnalyticsContext context, int limit)
    {
        return context.PageViews
            .GroupBy(pv => pv.Path)
            .Select(g => new { Path = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(limit)
            .AsEnumerable()
            .Select(x => (x.Path, x.Count))
            .ToList();
    }
}