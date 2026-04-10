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

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }
}

// 3. ProductOperations static class
public static class ProductOperations
{
    /// <summary>
    /// Applies a discount to all products in a specific category by manually querying and updating,
    /// necessary because the InMemory provider does not support ExecuteUpdateAsync.
    /// </summary>
    /// <param name="db">The DbContext to operate on.</param>
    /// <param name="category">The category to target.</param>
    /// <param name="discountPct">The discount fraction (e.g., 0.10 for 10%).</param>
    /// <returns>The number of rows affected (updated).</returns>
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        var updatedCount = 0;
        var now = DateTime.UtcNow;

        // Manually query matching products
        var productsToUpdate = await db.Products
            .Where(p => p.Category == category)
            .ToListAsync();

        // Manually update the properties
        foreach (var product in productsToUpdate)
        {
            product.Price = product.Price * (1m - discountPct);
            product.LastModified = now;
        }

        // Save changes
        await db.SaveChangesAsync();

        // Return the count of updated products
        return productsToUpdate.Count;
    }
}

// Example usage context (optional, for completeness, not strictly required by prompt)
/*
public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup In-Memory Database for testing
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using (var context = new ProductDbContext(options))
        {
            // Seed data
            context.Products.AddRange(
                new Product { Name = "Laptop", Price = 1000.00m, Category = "Electronics" },
                new Product { Name = "Mouse", Price = 25.00m, Category = "Electronics" },
                new Product { Name = "Shirt", Price = 50.00m, Category = "Apparel" }
            );
            await context.SaveChangesAsync();

            Console.WriteLine("--- Before Discount ---");
            var electronicsCountBefore = await context.Products.Where(p => p.Category == "Electronics").CountAsync();
            Console.WriteLine($"Electronics count: {electronicsCountBefore}");

            // Apply Discount
            string targetCategory = "Electronics";
            decimal discount = 0.10m; // 10% discount

            var affectedRows = await ProductOperations.ApplyDiscount(context, targetCategory, discount);

            Console.WriteLine($"\n--- After Discount ---");
            Console.WriteLine($"Products in {targetCategory} updated: {affectedRows}");

            var electronicsCountAfter = await context.Products.Where(p => p.Category == "Electronics").CountAsync();
            Console.WriteLine($"Electronics count after update: {electronicsCountAfter}");

            var apparelCount = await context.Products.Where(p => p.Category == "Apparel").CountAsync();
            Console.WriteLine($"Apparel count (unchanged): {apparelCount}");
        }
    }
}
*/