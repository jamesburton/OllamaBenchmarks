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
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
}

public static class ProductOperations
{
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        return await db.Products
            .Where(p => p.Category == category)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Price, p => p.Price * (1 - discountPct))
                .SetProperty(p => p.LastModified, DateTime.UtcNow));
    }
}