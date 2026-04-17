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
    /// Applies a discount to all products in a specific category using manual iteration,
    /// necessary for compatibility with the InMemory provider's limitations on ExecuteUpdateAsync.
    /// </summary>
    /// <param name="db">The DbContext to operate on.</param>
    /// <param name="category">The category to target.</param>
    /// <param name="discountPct">The discount fraction (e.g., 0.10 for 10%).</param>
    /// <returns>The number of products updated.</returns>
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        var updatedCount = 0;
        var now = DateTime.UtcNow;

        // Manually query matching products
        var productsToUpdate = await db.Products
            .Where(p => p.Category == category)
            .ToListAsync();

        foreach (var product in productsToUpdate)
        {
            // Calculate new price
            product.Price = product.Price * (1m - discountPct);

            // Update last modified time
            product.LastModified = now;

            // Note: Since we are operating on entities loaded from the context, 
            // EF Core will track these changes. We must explicitly mark them as modified 
            // or rely on SaveChangesAsync to detect changes if the context is tracking them.
            // For simplicity and correctness in this manual loop, we rely on the context tracking 
            // the modifications made to the tracked entities.
        }

        // Save changes
        await db.SaveChangesAsync();

        // Return the count of updated products
        return productsToUpdate.Count;
    }
}

// Example usage context (for completeness, not strictly required by prompt but useful for context)
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
                new Product { Name = "Laptop", Price = 1000m, Category = "Electronics" },
                new Product { Name = "Mouse", Price = 25m, Category = "Electronics" },
                new Product { Name = "Shirt", Price = 50m, Category = "Apparel" }
            );
            await context.SaveChangesAsync();

            Console.WriteLine("--- Before Discount ---");
            await DisplayProducts(context);

            // Apply Discount
            string targetCategory = "Electronics";
            decimal discount = 0.10m; // 10% discount
            int updated = await ProductOperations.ApplyDiscount(context, targetCategory, discount);
            Console.WriteLine($"\nSuccessfully applied {discount * 100}% discount to {updated} products in {targetCategory}.");

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