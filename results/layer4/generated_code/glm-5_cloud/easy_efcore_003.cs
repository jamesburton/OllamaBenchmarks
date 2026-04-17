using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}

public class ShopContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("ShopDatabase");
    }
}

public static class ProductRepository
{
    public static List<Product> GetAvailable(ShopContext context)
    {
        return context.Products
                      .Where(p => p.IsAvailable)
                      .OrderBy(p => p.Name)
                      .ToList();
    }
}