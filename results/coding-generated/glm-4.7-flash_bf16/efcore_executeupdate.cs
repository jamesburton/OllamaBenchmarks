using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

// Entity class
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public DateTime LastModified { get; set; }
}

// DbContext class
public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
}

// Static class with operations
public static class ProductOperations
{
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        // Filter products by the specified category
        var products = await db.Products
            .Where(p => p.Category == category)
            .ToListAsync();

        // Update each product's price and last modified timestamp
        foreach (var product in products)
        {
            product.Price = product.Price * (1 - discountPct);
            product.LastModified = DateTime.UtcNow;
        }

        // Save changes and return the number of rows affected
        return await db.SaveChangesAsync();
    }
}