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

// 3. Static class for operations
public static class ProductOperations
{
    /// <summary>
    /// Applies a discount to all products in a given category using manual loading and saving,
    /// simulating the limitation of the InMemory provider regarding bulk updates.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="category">The category to apply the discount to.</param>
    /// <param name="discountPct">The discount percentage (e.g., 0.10 for 10%).</param>
    /// <returns>The number of products updated.</returns>
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        // 1. Query matching products
        var productsToUpdate = await db.Products
            .Where(p => p.Category == category)
            .ToListAsync();

        int updatedCount = 0;

        // 2. Loop through and update manually
        foreach (var product in productsToUpdate)
        {
            // Calculate new price: Price * (1 - discountPct)
            decimal newPrice = product.Price * (1 - discountPct);

            // Update the entity state
            product.Price = newPrice;
            product.LastModified = DateTime.UtcNow;

            // Attach/Update the entity (since we loaded it, we just need to modify the tracked object)
            // In a real scenario, if we were using a fresh context, we would use Add/Update.
            // Since we loaded them, modifying the object and calling SaveChanges is sufficient.

            // 3. Save changes for this product
            await db.SaveChangesAsync();
            updatedCount++;
        }

        return updatedCount;
    }
}

// Example usage (optional, but helpful for testing the structure)
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
            // Seed Data
            context.Products.AddRange(
                new Product { Id = 1, Name = "Laptop", Price = 1000m, Category = "Electronics", LastModified = DateTime.UtcNow.AddDays(-1) },
                new Product { Id = 2, Name = "T-Shirt", Price = 20m, Category = "Apparel", LastModified = DateTime.UtcNow.AddDays(-1) },
                new Product { Id = 3, Name = "Monitor", Price = 300m, Category = "Electronics", LastModified = DateTime.UtcNow.AddDays(-1) },
                new Product { Id = 4, Name = "Jeans", Price = 75m, Category = "Apparel", LastModified = DateTime.UtcNow.AddDays(-1) }
            );
            await context.SaveChangesAsync();

            Console.WriteLine("--- Initial State ---");
            var initialProducts = await context.Products.ToListAsync();
            foreach (var p in initialProducts)
            {
                Console.WriteLine($"ID: {p.Id}, Name: {p.Name}, Price: {p.Price:C}, Category: {p.Category}");
            }

            // Apply 10% discount to Electronics
            Console.WriteLine("\n--- Applying 10% Discount to Electronics ---");
            var updatedCount = await ProductOperations.ApplyDiscount(context, "Electronics", 0.10m);

            Console.WriteLine($"\nSuccessfully updated {updatedCount} products.");

            // Verify results
            Console.WriteLine("\n--- Final State ---");
            var finalProducts = await context.Products.ToListAsync();
            foreach (var p in finalProducts)
            {
                Console.WriteLine($"ID: {p.Id}, Name: {p.Name}, Price: {p.Price:C}, Category: {p.Category}, Modified: {p.LastModified:yyyy-MM-dd HH:mm:ss}");
            }
        }
    }
}
*/