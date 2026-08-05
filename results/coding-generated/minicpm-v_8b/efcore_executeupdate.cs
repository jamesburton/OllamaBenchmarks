using System;
using System.Collections.Generic;
using System.Linq;
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
        : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Empty, as InMemory provider does not need explicit table mapping.
    }
}

public static class ProductOperations
{
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        int rowsAffected = 0;

        var products = await db.Products.Where(p => p.Category == category).ToListAsync();

        foreach (var product in products)
        {
            product.Price *= (1 - discountPct);
            product.LastModified = DateTime.UtcNow;
            rowsAffected++;
        }

        await db.SaveChangesAsync();
        return rowsAffected;
    }
}