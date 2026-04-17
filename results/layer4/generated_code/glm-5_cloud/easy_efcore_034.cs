using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

public class Metric
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Value { get; set; }
    public DateTime RecordedAt { get; set; }
}

public class MetricContext : DbContext
{
    public DbSet<Metric> Metrics { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("MetricDatabase");
    }
}

public static class MetricRepository
{
    public static (double Min, double Max, double Avg) GetStats(MetricContext context, string name)
    {
        var values = context.Metrics
            .Where(m => m.Name == name)
            .Select(m => m.Value)
            .ToList();

        if (values.Count == 0)
        {
            throw new InvalidOperationException($"No data found for metric named '{name}'.");
        }

        double min = values.Min();
        double max = values.Max();
        double avg = values.Average();

        return (min, max, avg);
    }
}