using Microsoft.EntityFrameworkCore;
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

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
}

public static class ProductOperations
{
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        // The InMemory provider does not support ExecuteUpdateAsync.
        if (db.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            // Manual update for InMemory provider
            var products = await db.Products
                .Where(p => p.Category == category)
                .ToListAsync();

            int count = 0;
            foreach (var product in products)
            {
                product.Price = product.Price * (1 - discountPct);
                product.LastModified = DateTime.UtcNow;
                count++;
            }

            await db.SaveChangesAsync();
            return count;
        }

        // Use ExecuteUpdateAsync for other providers (e.g., SQL Server, SQLite)
        return await db.Products
            .Where(p => p.Category == category)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Price, p => p.Price * (1 - discountPct))
                .SetProperty(p => p.LastModified, DateTime.UtcNow));
    }
}