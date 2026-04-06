using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
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
        // Find all products in the specified category
        var products = await db.Products.Where(p => p.Category == category).ToListAsync();

        // Apply the discount and update LastModified for each found product
        foreach (var p in products)
        {
            p.Price = p.Price * (1 - discountPct);
            p.LastModified = DateTime.UtcNow;
        }

        // Persist the changes
        await db.SaveChangesAsync();

        // Return the number of rows that were updated
        return products.Count;
    }
}