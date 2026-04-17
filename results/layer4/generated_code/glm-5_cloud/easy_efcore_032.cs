using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

public class Setting
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public string Category { get; set; }
}

public class SettingsContext : DbContext
{
    public DbSet<Setting> Settings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("SettingsDatabase");
    }
}

public static class SettingsRepository
{
    public static Dictionary<string, string> GetByCategory(SettingsContext context, string category)
    {
        return context.Settings
                      .Where(s => s.Category == category)
                      .ToDictionary(s => s.Key, s => s.Value);
    }
}