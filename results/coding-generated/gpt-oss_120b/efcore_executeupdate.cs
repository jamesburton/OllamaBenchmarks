using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public string Category { get; set; } = null!;
    public DateTime LastModified { get; set; }
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; } = null!;

    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }
}

public static class ProductOperations
{
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        var products = await db.Products
            .Where(p => p.Category == category)
            .ToListAsync();

        foreach (var product in products)
        {
            product.Price = product.Price * (1 - discountPct);
            product.LastModified = DateTime.UtcNow;
        }

        return await db.SaveChangesAsync();
    }
}