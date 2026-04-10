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

// 3. ProductOperations static class
public static class ProductOperations
{
    /// <summary>
    /// Applies a discount to all products in a specific category using SaveChangesAsync loop 
    /// to ensure compatibility with InMemory provider.
    /// </summary>
    /// <param name="db">The database context instance.</param>
    /// <param name="category">The category to apply the discount to.</param>
    /// <param name="discountPct">The discount percentage (e.g., 0.10 for 10%).</param>
    /// <returns>The number of products updated.</returns>
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        // Fetch all products in the target category
        var productsToUpdate = await db.Products
            .Where(p => p.Category == category)
            .ToListAsync();

        int updatedCount = 0;

        foreach (var product in productsToUpdate)
        {
            // Calculate new price
            decimal newPrice = product.Price * (1 - discountPct);

            // Update the entity state manually
            product.Price = newPrice;
            product.LastModified = DateTime.UtcNow;

            // Attach or update the entity state (since we fetched it, we just modify the tracked object)
            // For simplicity and robustness in this specific scenario, we rely on the fact that 
            // modifying the object retrieved via ToListAsync will be tracked by the context.
            // We must ensure the context is tracking changes.
            db.Entry(product).State = EntityState.Modified;
            updatedCount++;
        }

        // Save all changes in one transaction block
        await db.SaveChangesAsync();

        return updatedCount;
    }
}

// Example usage structure (not required in the final output, but useful for testing context)
/*
public class Program
{
    public static async Task Main(string[] args)
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using (var context = new ProductDbContext(options))
        {
            // Seed Data
            context.Products.AddRange(
                new Product { Name = "Laptop", Price = 1000m, Category = "Electronics", LastModified = DateTime.UtcNow },
                new Product { Name = "T-Shirt", Price = 20m, Category = "Apparel", LastModified = DateTime.UtcNow },
                new Product { Name = "Mouse", Price = 50m, Category = "Electronics", LastModified = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            // Apply Discount (10% off Electronics)
            Console.WriteLine("--- Before Discount ---");
            var initialProducts = await context.Products.ToListAsync();
            foreach (var p in initialProducts)
            {
                Console.WriteLine($"{p.Name}: {p.Price:C}");
            }

            var updatedCount = await ProductOperations.ApplyDiscount(context, "Electronics", 0.10m);

            Console.WriteLine($"\n--- After Discount ---");
            Console.WriteLine($"Rows updated: {updatedCount}");

            var finalProducts = await context.Products.ToListAsync();
            foreach (var p in finalProducts)
            {
                Console.WriteLine($"{p.Name}: {p.Price:C} (Modified: {p.LastModified:yyyy-MM-dd HH:mm:ss})");
            }
        }
    }
}
*/