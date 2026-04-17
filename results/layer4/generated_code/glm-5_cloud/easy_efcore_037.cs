using Microsoft.EntityFrameworkCore;

public class Feature
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string? Description { get; set; }
}

public class FeatureContext : DbContext
{
    public DbSet<Feature> Features { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("FeatureDatabase");
    }
}

public static class FeatureRepository
{
    public static Feature Toggle(FeatureContext context, string name)
    {
        var feature = context.Features.FirstOrDefault(f => f.Name == name);

        if (feature == null)
        {
            throw new KeyNotFoundException($"Feature with name '{name}' was not found.");
        }

        feature.IsEnabled = !feature.IsEnabled;
        context.SaveChanges();

        return feature;
    }
}