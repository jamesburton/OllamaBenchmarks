using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

public class ApiKey
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public int RequestCount { get; set; }
    public bool IsActive { get; set; }
}

public class ApiKeyContext : DbContext
{
    public DbSet<ApiKey> ApiKeys { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseInMemoryDatabase("ApiKeyDb");
        }
    }
}

public static class ApiKeyRepository
{
    public static ApiKey? IncrementAndGet(ApiKeyContext context, string key)
    {
        // Step 1: Increment RequestCount for active keys matching the provided key.
        // We filter by Key and IsActive to ensure we only increment valid, active keys.
        int affectedRows = context.ApiKeys
            .Where(k => k.Key == key && k.IsActive)
            .ExecuteUpdate(setters => setters.SetProperty(k => k.RequestCount, k => k.RequestCount + 1));

        // Step 2: If no rows were affected, the key was not found or was inactive.
        if (affectedRows == 0)
        {
            return null;
        }

        // Step 3: Retrieve and return the updated active ApiKey.
        // Since ExecuteUpdate bypasses the ChangeTracker, we query the database directly to get the latest state.
        return context.ApiKeys.FirstOrDefault(k => k.Key == key && k.IsActive);
    }
}