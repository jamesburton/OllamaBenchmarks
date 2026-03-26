using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public DateTime LastModified { get; set; }
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
}

public static class ProductOperations
{
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        var products = await db.Products.Where(p => p.Category == category).ToListAsync();

        foreach (var product in products)
        {
            product.Price *= (1 - discountPct);
            product.LastModified = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        return products.Count;
    }
}