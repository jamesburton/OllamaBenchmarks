using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Applies a discount to all products in a given category using manual loading and saving 
    /// (required for compatibility with InMemory provider).
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

        int updatedCount = 0;

        // 2. Iterate and update manually (required for InMemory compatibility)
        foreach (var product in productsToUpdate)
        {
            // Calculate new price: Price * (1 - discountPct)
            decimal newPrice = product.Price * (1 - discountPct);

            // Update properties
            product.Price = newPrice;
            product.LastModified = DateTime.UtcNow;

            // Attach the updated entity and mark it as modified
            // Note: Since we loaded the entities into memory (productsToUpdate), 
            // we must explicitly tell the context to track the changes.
            db.Entry(product).State = EntityState.Modified;

            updatedCount++;
        }

        // 3. Save all changes in one transaction
        await db.SaveChangesAsync();

        return updatedCount;
    }
}