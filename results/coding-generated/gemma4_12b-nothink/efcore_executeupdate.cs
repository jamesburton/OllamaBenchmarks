using System;
using System.Linq;
using System.Threading.Tasks;
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
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }
}

public static class ProductOperations
{
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        // Check if the provider is In-Memory or another provider that doesn't support ExecuteUpdateAsync.
        // Since the prompt specifies a fallback for InMemory specifically to ensure testability:
        if (db.Database.IsInMemory() || !db.Database.GetExecutingProvider().GetType().Name.Contains("SqlServer"))
        {
            var products = await db.Products
                .Where(p => p.Category == category)
                .ToListAsync();

            int count = 0;
            foreach (var product in products)
            {
                product.Price *= (1 - discountPct);
                product.LastModified = DateTime.UtcNow;
                count++;
            }

            await db.SaveChangesAsync();
            return count;
        }

        // Standard EF Core 10+ Bulk Update for supported providers
        return await db.Products
            .Where(p => p.Category == category)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Price, p => p.Price * (1 - discountPct))
                .SetProperty(p => p.LastModified, DateTime.UtcNow));
    }
}