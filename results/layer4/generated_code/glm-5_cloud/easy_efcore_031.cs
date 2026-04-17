using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

public class Report
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReportContext : DbContext
{
    public DbSet<Report> Reports { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("ReportDatabase");
    }
}

public static class ReportRepository
{
    public static List<Report> GetByUser(ReportContext context, string userId)
    {
        return context.Reports
            .Where(r => r.CreatedBy == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
    }
}