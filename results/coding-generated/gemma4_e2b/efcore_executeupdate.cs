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

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }
}

// 3. Static Operations Class
public static class ProductOperations
{
    /// <summary>
    /// Applies a discount to all products in a specific category using a manual update loop,
    /// necessary for compatibility with in-memory providers.
    /// </summary>
    /// <param name="db">The DbContext.</param>
    /// <param name="category">The category to target.</param>
    /// <param name="discountPct">The discount rate (e.g., 0.10 for 10%).</param>
    /// <returns>The number of products updated.</returns>
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        var updatedCount = 0;
        var now = DateTime.UtcNow;

        // Step 1: Query the products to be updated
        var productsToUpdate = await db.Products
            .Where(p => p.Category == category)
            .ToListAsync();

        // Step 2: Manually update the properties
        foreach (var product in productsToUpdate)
        {
            // Calculate new price
            product.Price = product.Price * (1m - discountPct);

            // Update modification time
            product.LastModified = now;
        }

        // Step 3: Save changes
        await db.SaveChangesAsync();

        // Step 4: Return the count
        return productsToUpdate.Count;
    }
}

// Example Usage (Optional, for context/testing purposes)
/*
public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup In-Memory Database
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using (var context = new ProductDbContext(options))
        {
            context.Database.EnsureCreated();

            // Seed Data
            context.Products.AddRange(
                new Product { Name = "Laptop", Price = 1000m, Category = "Electronics" },
                new Product { Name = "Shirt", Price = 50m, Category = "Apparel" },
                new Product { Name = "Monitor", Price = 300m, Category = "Electronics" }
            );
            await context.SaveChangesAsync();

            Console.WriteLine("--- Before Discount ---");
            await DisplayProducts(context);

            // Apply Discount (10% off Electronics)
            int updated = await ProductOperations.ApplyDiscount(context, "Electronics", 0.10m);
            Console.WriteLine($"\nSuccessfully updated {updated} products.");

            Console.WriteLine("\n--- After Discount ---");
            await DisplayProducts(context);
        }
    }

    private static async Task DisplayProducts(ProductDbContext context)
    {
        var products = await context.Products.OrderBy(p => p.Id).ToListAsync();
        foreach (var p in products)
        {
            Console.WriteLine($"ID: {p.Id}, Name: {p.Name}, Price: {p.Price:C}, Category: {p.Category}, Modified: {p.LastModified}");
        }
    }
}
*/