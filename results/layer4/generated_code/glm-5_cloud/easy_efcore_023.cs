using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}

public class StoreContext : DbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("StoreDatabase");
    }
}

public static class StoreRepository
{
    public static List<Category> GetCategoriesWithProducts(StoreContext context)
    {
        return context.Categories
                      .Include(c => c.Products)
                      .ToList();
    }
}