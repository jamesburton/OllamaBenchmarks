using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

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
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
}

public static class ProductOperations
{
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        // Check if we are using the InMemory provider
        bool isInMemory = db.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

        if (isInMemory)
        {
            // Fallback implementation for InMemory provider (which does not support ExecuteUpdateAsync)
            var productsToUpdate = await db.Products
                .Where(p => p.Category == category)
                .ToListAsync();

            int count = productsToUpdate.Count;

            foreach (var product in productsToUpdate)
            {
                product.Price = product.Price * (1 - discountPct);
                product.LastModified = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
            return count;
        }
        else
        {
            // EF Core 10 Bulk Update implementation
            return await db.Products
                .Where(p => p.Category == category)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Price, p => p.Price * (1 - discountPct))
                    .SetProperty(p => p.LastModified, DateTime.UtcNow));
        }
    }
}