using Microsoft.EntityFrameworkCore;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AppContext : DbContext
{
    public DbSet<Category> Categories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("AppDatabase");
    }
}

public static class CategoryRepository
{
    public static Category? FindByName(AppContext context, string name)
    {
        return context.Categories
            .FirstOrDefault(c => c.Name.ToLower() == name.ToLower());
    }
}