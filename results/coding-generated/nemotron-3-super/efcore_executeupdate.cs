using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

        int count = 0;
        foreach (var p in products)
        {
            p.Price = p.Price * (1 - discountPct);
            p.LastModified = DateTime.UtcNow;
            count++;
        }

        await db.SaveChangesAsync();
        return count;
    }
}