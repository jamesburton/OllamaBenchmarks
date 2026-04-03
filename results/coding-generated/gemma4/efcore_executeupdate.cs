using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

// 1. Product entity class
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public DateTime LastModified { get; set; }
}

// 2. ProductDbContext class
public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
}

// 3. Static class for operations
public static class ProductOperations
{
    /// <summary>
    /// Applies a discount to all products in a given category using manual updates 
    /// suitable for in-memory testing environments.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="category">The category to apply the discount to.</param>
    /// <param name="discountPct">The discount percentage (e.g., 0.10 for 10%).</param>
    /// <returns>The number of products updated.</returns>
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        // 1. Query all products in the specified category
        var productsToUpdate = await db.Products
            .Where(p => p.Category == category)
            .ToListAsync();

        if (productsToUpdate == null || productsToUpdate.Count == 0)
        {
            return 0;
        }

        int updatedCount = 0;
        var now = DateTime.UtcNow;

        // 2. Manually update each product
        foreach (var product in productsToUpdate)
        {
            // Calculate new price: Price * (1 - discountPct)
            product.Price = product.Price * (1 - discountPct);
            product.LastModified = now;
            updatedCount++;
        }

        // 3. Save all changes in one transaction
        await db.SaveChangesAsync();

        return updatedCount;
    }
}