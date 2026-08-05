using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Product Entity Class
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public DateTime LastModified { get; set; }
}

// 2. ProductDbContext Class
public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    // No OnModelCreating required as per instructions
}

// 3. Static Operations Class
public static class ProductOperations
{
    /// <summary>
    /// Applies a discount to all products in a specific category using manual iteration 
    /// (necessary for InMemory provider compatibility).
    /// </summary>
    /// <param name="db">The DbContext instance.</param>
    /// <param name="category">The category to target.</param>
    /// <param name="discountPct">The fractional discount (e.g., 0.10 for 10%).</param>
    /// <returns>The number of products updated.</returns>
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        var updatedCount = 0;
        var now = DateTime.UtcNow;

        // 1. Query the products to be updated
        var productsToUpdate = await db.Products
            .Where(p => p.Category == category)
            .ToListAsync();

        if (!productsToUpdate.Any())
        {
            return 0;
        }

        // 2. Manually update the properties
        foreach (var product in productsToUpdate)
        {
            // Calculate new price
            product.Price = product.Price * (1m - discountPct);

            // Update modification time
            product.LastModified = now;
        }

        // 3. Save changes
        await db.SaveChangesAsync();

        // 4. Return the count
        return productsToUpdate.Count;
    }
}

// Example Usage (Optional, for context/testing purposes, not required in the final output)
/*
public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup InMemory Database
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using (var context = new ProductDbContext(options))
        {
            // Seed Data
            context.Products.AddRange(
                new Product { Name = "Laptop", Price = 1000m, Category = "Electronics" },
                new Product { Name = "Keyboard", Price = 50m, Category = "Electronics" },
                new Product { Name = "Shirt", Price = 20m, Category = "Apparel" }
            );
            await context.SaveChangesAsync();

            Console.WriteLine("--- Before Discount ---");
            var electronicsCount = await context.Products.Where(p => p.Category == "Electronics").CountAsync();
            Console.WriteLine($"Electronics count: {electronicsCount}");

            // Apply Discount (10% off Electronics)
            decimal discount = 0.10m;
            int updated = await ProductOperations.ApplyDiscount(context, "Electronics", discount);

            Console.WriteLine($"\nSuccessfully updated {updated} products in Electronics.");

            Console.WriteLine("\n--- After Discount ---");
            var electronicsAfter = await context.Products.Where(p => p.Category == "Electronics").ToListAsync();
            foreach (var p in electronicsAfter)
            {
                Console.WriteLine($"Product: {p.Name}, Price: {p.Price:C}, Modified: {p.LastModified}");
            }
        }
    }
}
*/