using Microsoft.EntityFrameworkCore;

public class Campaign
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public decimal Budget { get; set; }
}

public class CampaignContext : DbContext
{
    public DbSet<Campaign> Campaigns { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("CampaignDatabase");
    }
}

public static class CampaignRepository
{
    public static int DeactivateAll(CampaignContext context)
    {
        return context.Campaigns
            .Where(c => c.IsActive)
            .ExecuteUpdate(setters => setters.SetProperty(c => c.IsActive, false));
    }
}