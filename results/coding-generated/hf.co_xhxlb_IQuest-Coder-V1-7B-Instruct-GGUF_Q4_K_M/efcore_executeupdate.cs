using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

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
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        var matchingProducts = await db.Products
            .Where(p => p.Category == category)
            .ToListAsync();

        int updatedCount = 0;
        foreach (var product in matchingProducts)
        {
            product.Price = product.Price * (1 - discountPct);
            product.LastModified = DateTime.UtcNow;
            db.Entry(product).State = EntityState.Modified;
            updatedCount++;
        }

        await db.SaveChangesAsync();
        return updatedCount;
    }
}