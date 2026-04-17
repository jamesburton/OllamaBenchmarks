using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

public class Job
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class JobContext : DbContext
{
    public DbSet<Job> Jobs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("JobDatabase");
    }
}

public static class JobRepository
{
    public static Dictionary<string, int> GetCountByStatus(JobContext context)
    {
        return context.Jobs
            .GroupBy(j => j.Status)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}