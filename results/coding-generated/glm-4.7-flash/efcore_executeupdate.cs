using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public DateTime LastModified { get; set; }
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }
}

public static class ProductOperations
{
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        // Check if the provider is InMemory to determine execution strategy
        var isInMemory = db.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

        if (isInMemory)
        {
            // In-Memory provider does not support ExecuteUpdateAsync.
            // We manually query, update, and save changes.
            var productsToUpdate = await db.Products
                .Where(p => p.Category == category)
                .ToListAsync();

            foreach (var product in productsToUpdate)
            {
                product.Price = product.Price * (1 - discountPct);
                product.LastModified = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
            return productsToUpdate.Count;
        }
        else
        {
            // Use EF Core 10 ExecuteUpdateAsync for other providers
            return await db.Products
                .Where(p => p.Category == category)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Price, p => p.Price * (1 - discountPct))
                    .SetProperty(p => p.LastModified, DateTime.UtcNow));
        }
    }
}